using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class LeaveRepository
{
    private readonly IBaseRepository<LeaveType> _leaveTypeRepository;
    private readonly IBaseRepository<LeaveRequest> _leaveRequestRepository;

    public LeaveRepository(
        IBaseRepository<LeaveType> leaveTypeRepository,
        IBaseRepository<LeaveRequest> leaveRequestRepository)
    {
        _leaveTypeRepository = leaveTypeRepository;
        _leaveRequestRepository = leaveRequestRepository;
    }

    // =========================
    // Leave Types
    // =========================

    public async Task<List<LeaveType>> GetLeaveTypesAsync()
    {
        return await _leaveTypeRepository.Query()
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<LeaveType?> GetLeaveTypeByIdAsync(int id)
    {
        return await _leaveTypeRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    public async Task<bool> LeaveTypeExistsAsync(LeaveName leaveName, int? id)
    {
        return await _leaveTypeRepository.Query()
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AnyAsync(x => x.LeaveNameId == leaveName && x.Id != id);
    }

    public Task<LeaveType> CreateLeaveTypeAsync(LeaveType leaveType)
    {
        return _leaveTypeRepository.AddAsync(leaveType);
    }

    public Task<LeaveType> UpdateLeaveTypeAsync(LeaveType leaveType)
    {
        return _leaveTypeRepository.UpdateAsync(leaveType);
    }

    public async Task<bool> DeleteLeaveTypeAsync(int id)
    {
        var leaveType = await _leaveTypeRepository.Query()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (leaveType == null)
            return false;

        leaveType.IsActive = false; 
        leaveType.IsDeleted = true;
        leaveType.UpdatedOn = DateTime.Now;

        await _leaveTypeRepository.UpdateAsync(leaveType);
        return true;
    }

    // =========================
    // Leave Requests
    // =========================

    public async Task<List<LeaveRequest>> GetLeaveRequestsAsync()
    {
        // Perf 5 FIX: Default to last 12 months to avoid loading entire history
        var twelveMonthsAgo = DateTime.Today.AddMonths(-12);

        return await _leaveRequestRepository.Query()
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.StartDate >= twelveMonthsAgo)
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .Include(x => x.ApprovedByEmployee)
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<List<LeaveRequest>> GetLeaveRequestsByFilterAsync(Status? status, int? employeeId)
    {
        IQueryable<LeaveRequest> query = _leaveRequestRepository.Query()
        .AsNoTracking()
        .Where(x => !x.IsDeleted)
        .Include(x => x.Employee)
        .Include(x => x.LeaveType)
        .Include(x => x.ApprovedByEmployee);

        if (status.HasValue)
            query = query.Where(x => x.StatusId == status.Value);

        if (employeeId.HasValue)
            query = query.Where(x => x.EmployeeId == employeeId.Value);

        return await query
            .OrderByDescending(x => x.Id)
            .ToListAsync(); 
    }

    public async Task<LeaveRequest?> GetLeaveRequestByIdAsync(int id)
    {
        return await _leaveRequestRepository.Query()
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .Include(x => x.ApprovedByEmployee)
            .FirstOrDefaultAsync();
    }

    public async Task<List<LeaveRequest>> GetLeaveRequestByEmployeeIdAsync(int employeeId)
    {
        return await _leaveRequestRepository.Query()
            .AsNoTracking()
            .Where(x =>
                x.EmployeeId == employeeId &&
                !x.IsDeleted &&
                x.StatusId != Status.Cancelled)
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .Include(x => x.ApprovedByEmployee)
            .OrderByDescending(x => x.Id)
            .ToListAsync(); 
    }

    public Task<LeaveRequest> CreateLeaveRequestAsync(LeaveRequest leaveRequest)
    {
        return _leaveRequestRepository.AddAsync(leaveRequest);
    }

    public Task<LeaveRequest> UpdateLeaveRequestAsync(LeaveRequest leaveRequest)
    {
        return _leaveRequestRepository.UpdateAsync(leaveRequest);
    }

    public async Task<bool> DeleteLeaveRequestAsync(int id)
    {
        var leaveRequest = await _leaveRequestRepository.Query()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (leaveRequest == null || leaveRequest.StatusId == Status.Approved)
            return false;

        leaveRequest.IsActive = false;
        leaveRequest.IsDeleted = true;
        leaveRequest.UpdatedOn = DateTime.Now;

        await _leaveRequestRepository.UpdateAsync(leaveRequest);
        return true;
    }

    public async Task<bool> UpdateLeaveRequestStatusAsync(int leaveRequestId, Status status, int approvedBy)
    { 
        var leaveRequest = await _leaveRequestRepository.Query()
            .FirstOrDefaultAsync(x =>
                x.Id == leaveRequestId &&
                !x.IsDeleted);

        if (leaveRequest == null)
            return false;

        bool canUpdate =
            leaveRequest.StatusId == Status.Pending ||
            (leaveRequest.StatusId == Status.Approved && status == Status.Rejected) ||
            (leaveRequest.StatusId == Status.Rejected && status == Status.Approved);

        if (!canUpdate)
            return false;

        leaveRequest.StatusId = status;
        leaveRequest.ApprovedBy = approvedBy;
        leaveRequest.ApprovedDate = DateTime.Now;
        leaveRequest.UpdatedOn = DateTime.Now;

        await _leaveRequestRepository.UpdateAsync(leaveRequest);
        return true;
    }

    public async Task<bool> CancelLeaveRequestAsync(int id, int employeeId)
    {
        var leaveRequest = await _leaveRequestRepository.Query()
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.EmployeeId == employeeId &&
                !x.IsDeleted);

        if (leaveRequest == null || leaveRequest.StatusId != Status.Pending)
            return false;

        leaveRequest.StatusId = Status.Cancelled;
        leaveRequest.UpdatedOn = DateTime.Now;

        await _leaveRequestRepository.UpdateAsync(leaveRequest);
        return true;
    }

    public async Task<List<LeaveRequest>> GetApprovedLeavesCoveringDateAsync(int employeeId, DateTime date)
    {
        var targetDate = date.Date;
        return await _leaveRequestRepository.Query()
            .Where(x =>
                x.EmployeeId == employeeId &&
                x.IsActive && !x.IsDeleted &&
                x.StatusId == Status.Approved &&
                x.StartDate.Date <= targetDate &&
                x.EndDate.Date >= targetDate)
            .ToListAsync();
    }

    public async Task<bool> SoftDeleteLeaveRequestAsync(LeaveRequest leave)
    {
        leave.IsActive = false;
        leave.IsDeleted = true;
        leave.StatusId = Status.Cancelled;
        leave.UpdatedOn = DateTime.Now;

        await _leaveRequestRepository.UpdateAsync(leave);
        return true;
    }

    // Bug 5 & Multi-case FIX: Soft-delete the previous leave and create new auto-approved segment(s)
    public async Task<bool> EmployeeWorkedOnDateAsync(int employeeId, DateTime date)
    {
        var targetDate = date.Date;
        var leaves = await GetApprovedLeavesCoveringDateAsync(employeeId, targetDate);

        if (!leaves.Any())
            return true;

        foreach (var leave in leaves)
        {
            // 1. Soft-delete the original leave request
            await SoftDeleteLeaveRequestAsync(leave);

            // 2. If there are dates before targetDate, create the left segment
            if (targetDate > leave.StartDate.Date)
            {
                var leftStart = leave.StartDate.Date;
                var leftEnd = targetDate.AddDays(-1);
                var leftDays = (leftEnd - leftStart).Days + 1;

                if (leftDays > 0)
                {
                    var leftLeave = new LeaveRequest
                    {
                        EmployeeId = leave.EmployeeId,
                        LeaveTypeId = leave.LeaveTypeId,
                        StartDate = leftStart,
                        EndDate = leftEnd,
                        LeaveDays = leftDays,
                        Reason = leave.Reason,
                        StatusId = Status.Approved,
                        ApprovalNotes = "Auto Adjusted",
                        ApprovedBy = leave.ApprovedBy,
                        ApprovedDate = leave.ApprovedDate ?? DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedOn = DateTime.Now
                    };
                    await _leaveRequestRepository.AddAsync(leftLeave);
                }
            }

            // 3. If there are dates after targetDate, create the right segment
            if (targetDate < leave.EndDate.Date)
            {
                var rightStart = targetDate.AddDays(1);
                var rightEnd = leave.EndDate.Date;
                var rightDays = (rightEnd - rightStart).Days + 1;

                if (rightDays > 0)
                {
                    var rightLeave = new LeaveRequest
                    {
                        EmployeeId = leave.EmployeeId,
                        LeaveTypeId = leave.LeaveTypeId,
                        StartDate = rightStart,
                        EndDate = rightEnd,
                        LeaveDays = rightDays,
                        Reason = leave.Reason,
                        StatusId = Status.Approved,
                        ApprovalNotes = "Auto Adjusted",
                        ApprovedBy = leave.ApprovedBy,
                        ApprovedDate = leave.ApprovedDate ?? DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedOn = DateTime.Now
                    };
                    await _leaveRequestRepository.AddAsync(rightLeave);
                }
            }
        }

        return true;
    }
}