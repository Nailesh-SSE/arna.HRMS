using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Repository;
using arna.HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class LeaveRepository
{
    private readonly IBaseRepository<LeaveType> _leaveTypeRepository;
    private readonly IBaseRepository<LeaveRequest> _leaveRequestRepository;
    private readonly ApplicationDbContext _context;

    public LeaveRepository(
        IBaseRepository<LeaveType> leaveTypeRepository,
        IBaseRepository<LeaveRequest> leaveRequestRepository,
        ApplicationDbContext? context = null)
    {
        _leaveTypeRepository = leaveTypeRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _context = context;
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

    public async Task<LeaveType> CreateLeaveTypeAsync(LeaveType leaveType)
    {
        var existing = await _leaveTypeRepository.Query()
            .FirstOrDefaultAsync(x => x.LeaveNameId == leaveType.LeaveNameId);

        if (existing != null)
        {
            existing.IsDeleted = false;
            existing.IsActive = leaveType.IsActive;
            existing.MaxPerYear = leaveType.MaxPerYear;
            existing.IsPaid = leaveType.IsPaid;
            existing.Description = leaveType.Description;
            existing.UpdatedOn = DateTime.Now;
            return await _leaveTypeRepository.UpdateAsync(existing);
        }

        return await _leaveTypeRepository.AddAsync(leaveType);
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

        if (leaveRequest == null || leaveRequest.StatusId == Status.Approved || leaveRequest.StatusId == Status.Rejected)
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

        if (leaveRequest.StatusId != Status.Pending)
            return false;

        leaveRequest.StatusId = status;
        leaveRequest.ApprovedBy = approvedBy;
        leaveRequest.ApprovedDate = DateTime.Now;
        leaveRequest.UpdatedOn = DateTime.Now;

        await _leaveRequestRepository.UpdateAsync(leaveRequest);
        return true;
    }

    public async Task<List<LeaveRequest>> BulkApproveLeaveRequestsAsync(List<int> requestIds, int approvedBy)
    {
        if (requestIds == null || !requestIds.Any())
            return new List<LeaveRequest>();

        var requestsToApprove = await _context.LeaveRequests
            .Where(lr => requestIds.Contains(lr.Id) && lr.StatusId == Status.Pending && !lr.IsDeleted)
            .ToListAsync();

        if (!requestsToApprove.Any())
            return new List<LeaveRequest>();

        var now = DateTime.Now;
        requestsToApprove.ForEach(lr =>
        {
            lr.StatusId = Status.Approved;
            lr.ApprovedBy = approvedBy;
            lr.ApprovedDate = now;
            lr.UpdatedOn = now;
        });

        await _context.SaveChangesAsync();
        return requestsToApprove;
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

    public async Task<List<LeaveRequest>> GetApprovedLeavesOverlappingRangeAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var start = startDate.Date;
        var end = endDate.Date;
        return await _leaveRequestRepository.Query()
            .Where(x =>
                x.EmployeeId == employeeId &&
                x.IsActive && !x.IsDeleted &&
                x.StatusId == Status.Approved &&
                x.StartDate.Date <= end &&
                x.EndDate.Date >= start)
            .ToListAsync();
    }

    public Task<List<LeaveRequest>> GetApprovedLeavesCoveringDateAsync(int employeeId, DateTime date)
    {
        return GetApprovedLeavesOverlappingRangeAsync(employeeId, date, date);
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

    public async Task<bool> EmployeeWorkedOnDateAsync(int employeeId, DateTime date)
    {
        var targetDate = date.Date;
        var leaves = await GetApprovedLeavesCoveringDateAsync(employeeId, targetDate);

        if (!leaves.Any())
            return true;

        await leaves.Aggregate(Task.CompletedTask, async (prev, leave) =>
        {
            await prev;
            await ProcessWorkedDayLeaveAsync(leave, targetDate);
        });

        return true;
    }

    private async Task ProcessWorkedDayLeaveAsync(LeaveRequest leave, DateTime targetDate)
    {
        await SoftDeleteLeaveRequestAsync(leave);

        if (targetDate > leave.StartDate.Date)
        {
            await CreateLeaveSegmentAsync(leave, leave.StartDate.Date, targetDate.AddDays(-1));
        }

        if (targetDate < leave.EndDate.Date)
        {
            await CreateLeaveSegmentAsync(leave, targetDate.AddDays(1), leave.EndDate.Date);
        }
    }

    private async Task CreateLeaveSegmentAsync(LeaveRequest source, DateTime start, DateTime end)
    {
        var days = (end - start).Days + 1;
        if (days <= 0) return;

        var segment = new LeaveRequest
        {
            EmployeeId = source.EmployeeId,
            LeaveTypeId = source.LeaveTypeId,
            StartDate = start,
            EndDate = end,
            LeaveDays = days,
            Reason = source.Reason,
            StatusId = Status.Approved,
            ApprovalNotes = "Auto Adjusted",
            ApprovedBy = source.ApprovedBy,
            ApprovedDate = source.ApprovedDate ?? DateTime.Now,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        };
        await _leaveRequestRepository.AddAsync(segment);
    }
}