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
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<LeaveType?> GetLeaveTypeByIdAsync(int id)
    {
        return await _leaveTypeRepository.Query()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive && !x.IsDeleted);
    }

    public async Task<bool> LeaveTypeExistsAsync(LeaveName leaveName, int? id)
    {
        return await _leaveTypeRepository.Query()
            .Where(x => x.IsActive && !x.IsDeleted)
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
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive && !x.IsDeleted);

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
        return await _leaveRequestRepository.Query()
            .Where(x => x.IsActive && !x.IsDeleted)
            .Include(x => x.Employee)
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<List<LeaveRequest>> GetLeaveRequestsByFilterAsync(Status? status, int? employeeId)
    {
        IQueryable<LeaveRequest> query = _leaveRequestRepository.Query()
        .Where(x => x.IsActive && !x.IsDeleted)
        .Include(x => x.Employee);

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
            .Where(x => x.Id == id && x.IsActive && !x.IsDeleted)
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
            .FirstOrDefaultAsync();
    }

    public async Task<List<LeaveRequest>> GetLeaveRequestByEmployeeIdAsync(int employeeId)
    {
        return await _leaveRequestRepository.Query()
            .Where(x =>
                x.EmployeeId == employeeId &&
                x.IsActive &&
                !x.IsDeleted &&
                x.StatusId != Status.Cancelled)
            .Include(x => x.Employee)
            .Include(x => x.LeaveType)
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
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive && !x.IsDeleted);

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
                x.IsActive && 
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
                x.IsActive &&
                !x.IsDeleted);

        if (leaveRequest == null || leaveRequest.StatusId != Status.Pending)
            return false;

        leaveRequest.StatusId = Status.Cancelled;
        leaveRequest.UpdatedOn = DateTime.Now;

        await _leaveRequestRepository.UpdateAsync(leaveRequest);
        return true;
    }
}