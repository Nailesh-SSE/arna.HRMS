using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class LeaveRepository
{
    private readonly IBaseRepository<LeaveType> _leaveTypeRepo;
    private readonly IBaseRepository<LeaveRequest> _leaveRequestRepo;


    public LeaveRepository(IBaseRepository<LeaveType> leaveTypeRepo, IBaseRepository<LeaveRequest> leaveRequestRepo)
    {
        _leaveTypeRepo = leaveTypeRepo;
        _leaveRequestRepo = leaveRequestRepo;
    }

    //Leave Type related methods can be added here as needed
    public async Task<List<LeaveType>> GetLeaveTypeAsync()
    {
        return await _leaveTypeRepo.Query()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<LeaveType?> GetLeaveTypeByIdAsync(int id)
    {
        return await _leaveTypeRepo.Query()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive && !x.IsDeleted);
    }

   public async Task<bool> LeaveExistsAsync(LeaveName Name)
    {
        return await _leaveTypeRepo.Query()
            .AnyAsync(x => x.LeaveNameId == Name && x.IsActive && !x.IsDeleted);
    }

    public Task<LeaveType> CreateLeaveTypeAsync(LeaveType leaveType)
    {
        return _leaveTypeRepo.AddAsync(leaveType);
    }

    public async Task<LeaveType> UpdateLeaveTypeAsync(LeaveType leaveType)
    {
        return await _leaveTypeRepo.UpdateAsync(leaveType);
    }

    public async Task<bool> DeleteLeaveTypeAsync(int id)
    {
        var leaveType = await GetLeaveTypeByIdAsync(id);

        if (leaveType == null)
            return false;

        leaveType.IsActive = false;
        leaveType.IsDeleted = true;
        leaveType.UpdatedOn = DateTime.Now;

        await _leaveTypeRepo.UpdateAsync(leaveType);
        return true;
    }

    

    //Leave Request related methods can be added here as needed
    public async Task<List<LeaveRequest>> GetLeaveRequestAsync()
    {
        return await _leaveRequestRepo.Query()
            .Include(lr => lr.Employee)
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderByDescending(o => o.Id)
            .ToListAsync();
    }

    public async Task<List<LeaveRequest>> GetLeaveRequestsByFilterAsync(Status? status, int? employeeId)
    {
        if (status == null && employeeId == null)
        {
            return await GetLeaveRequestAsync();
        }
        else if (status == null && employeeId != null)
        {
            return await _leaveRequestRepo.Query()
                .Include(lr => lr.Employee)
                .Where(x => x.EmployeeId == employeeId && x.IsActive && !x.IsDeleted)
                .OrderByDescending(o => o.Id)
                .ToListAsync();
        }
        else if (status != null && employeeId == null)
        {
            return await _leaveRequestRepo.Query()
                .Include(lr => lr.Employee)
                .Where(x => x.StatusId == status && x.IsActive && !x.IsDeleted)
                .OrderByDescending(o => o.Id)
                .ToListAsync();
        }
        else
        {
            return await _leaveRequestRepo.Query()
                .Include(lr => lr.Employee)
                .Where(x => x.StatusId == status && x.EmployeeId == employeeId && x.IsActive && !x.IsDeleted)
                .OrderByDescending(o => o.Id)
                .ToListAsync();
        }
    }

    public async Task<LeaveRequest?> GetLeaveRequestByIdAsync(int id)
    {

        return await _leaveRequestRepo.Query()
            .Include(e => e.Employee)
            .Include(e => e.LeaveType)
            .FirstOrDefaultAsync(e => e.Id == id && e.IsActive && !e.IsDeleted);
    }

    public async Task<List<LeaveRequest>> GetLeaveRequestByEmployeeIdAsync(int employeeId)
    {
        return await _leaveRequestRepo.Query()
            .Include(e => e.Employee)
            .Include(e => e.LeaveType)
            .Where(e => e.EmployeeId == employeeId && e.IsActive && !e.IsDeleted && e.StatusId != Status.Cancelled)
            .OrderByDescending(o => o.Id)
            .ToListAsync();
    }

    public Task<LeaveRequest> CreateLeaveRequestAsync(LeaveRequest leaveRequest)
    {
        return _leaveRequestRepo.AddAsync(leaveRequest);
    }

    public Task<LeaveRequest> UpdateLeaveRequestAsync(LeaveRequest leaveRequest)
    {
        return _leaveRequestRepo.UpdateAsync(leaveRequest);
    }

    public async Task<bool> DeleteLeaveRequestAsync(int id)
    {
        var LeaveRequest = await GetLeaveRequestByIdAsync(id);

        if (LeaveRequest == null || LeaveRequest.StatusId == Status.Pending)
            return false;

        LeaveRequest.IsActive = false;
        LeaveRequest.IsDeleted = true;
        LeaveRequest.UpdatedOn = DateTime.Now;

        await _leaveRequestRepo.UpdateAsync(LeaveRequest);
        return true;
    }

    public async Task<bool> UpdateLeaveStatusAsync(int leaveRequestId, Status status, int approvedBy)
    {
        var leaveRequest = await GetLeaveRequestByIdAsync(leaveRequestId);

        if (leaveRequest == null)
            return false;

        if (leaveRequest.StatusId != Status.Pending)
            return false;

        leaveRequest.StatusId = status;
        leaveRequest.ApprovedBy = approvedBy;
        leaveRequest.ApprovedDate = DateTime.Now;
        leaveRequest.UpdatedOn = DateTime.Now;

        await _leaveRequestRepo.UpdateAsync(leaveRequest);
        return true;
    }

    public async Task<bool> UpdateLeaveRequestStatusCancel(int id, int employeeId)
    {
        var attendanceRequest = await _leaveRequestRepo.Query()
            .FirstOrDefaultAsync(ar => ar.Id == id && ar.EmployeeId == employeeId && ar.IsActive && !ar.IsDeleted);
        if (attendanceRequest == null)
            return false;
        if (attendanceRequest.StatusId != Status.Pending)
            return false;
        attendanceRequest.StatusId = Status.Cancelled;
        attendanceRequest.UpdatedOn = DateTime.Now;
        await _leaveRequestRepo.UpdateAsync(attendanceRequest);

        return true;
    }
}
