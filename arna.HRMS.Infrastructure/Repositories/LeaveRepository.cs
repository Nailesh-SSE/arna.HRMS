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

    // Bug 5 FIX: Directly cancel the leave instead of calling DeleteLeaveRequestAsync
    // which refuses to delete approved leave (returns false), causing silent failures
    public async Task<bool> EmployeeWorkedOnDateAsync(int employeeId, DateTime date)
    {
        var leave = await _leaveRequestRepository.Query()
            .FirstOrDefaultAsync(x =>
                x.EmployeeId == employeeId &&
                x.IsActive && !x.IsDeleted &&
                x.StatusId == Status.Approved &&
                x.StartDate <= date &&
                x.EndDate >= date);

        if (leave == null)
            return true;

        if (leave.StartDate.Date == leave.EndDate.Date)
        {
            // Single-day leave: cancel it directly (not via DeleteLeaveRequestAsync which blocks approved)
            leave.StatusId = Status.Cancelled;
            leave.UpdatedOn = DateTime.Now;
            await _leaveRequestRepository.UpdateAsync(leave);
            return true;
        }

        if (leave.StartDate.Date == date.Date)
        {
            leave.StartDate = date.AddDays(1);
            RecalculateLeaveDays(leave);

            await _leaveRequestRepository.UpdateAsync(leave);
            return true;
        }

        if (leave.EndDate.Date == date.Date)
        {
            leave.EndDate = date.AddDays(-1);
            RecalculateLeaveDays(leave);

            await _leaveRequestRepository.UpdateAsync(leave);
            return true;
        }


        var secondHalf = new LeaveRequest
        {
            EmployeeId = leave.EmployeeId,
            LeaveTypeId = leave.LeaveTypeId,
            StartDate = date.AddDays(1),
            EndDate = leave.EndDate,
            Reason = leave.Reason,
            StatusId = leave.StatusId,
            ApprovedBy = leave.ApprovedBy,
            ApprovedDate = leave.ApprovedDate,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        };

        RecalculateLeaveDays(secondHalf);

        // Update first half
        leave.EndDate = date.AddDays(-1);
        RecalculateLeaveDays(leave);

        await _leaveRequestRepository.UpdateAsync(leave);
        await _leaveRequestRepository.AddAsync(secondHalf);

        return true;
    }

    private void RecalculateLeaveDays(LeaveRequest leave)
    {
        leave.LeaveDays = (leave.EndDate.Date - leave.StartDate.Date).Days + 1;
    }
}