using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class LeaveRepository
{
    private readonly IBaseRepository<LeaveMaster> _leaveMasterRepo;
    private readonly IBaseRepository<LeaveRequest> _leaveRequestRepo;
    private readonly IBaseRepository<EmployeeLeaveBalance> _leaveBalanceRepo;


    public LeaveRepository(IBaseRepository<LeaveMaster> leaveMasterRepo, IBaseRepository<LeaveRequest> leaveRequestRepo, IBaseRepository<EmployeeLeaveBalance> leaveBalanceRepo)
    {
        _leaveMasterRepo = leaveMasterRepo;
        _leaveRequestRepo = leaveRequestRepo;
        _leaveBalanceRepo = leaveBalanceRepo;
    }

    //Leave Master related methods can be added here as needed
    public async Task<List<LeaveMaster>> GetLeaveMasterAsync()
    {
        return await _leaveMasterRepo.Query()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }
    public async Task<LeaveMaster?> GetLeaveMasterByIdAsync(int id)
    {
        return await _leaveMasterRepo.Query()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive && !x.IsDeleted);
    }
    public Task<LeaveMaster> CreateLeaveMasterAsync(LeaveMaster leaveMaster)
    {
        return _leaveMasterRepo.AddAsync(leaveMaster);
    }

    public Task<LeaveMaster> UpdateLeaveMasterAsync(LeaveMaster leaveMaster)
    {
        return _leaveMasterRepo.UpdateAsync(leaveMaster);
    }

    public async Task<bool> DeleteLeaveMasterAsync(int id)
    {
        var leaveMaster = await _leaveMasterRepo.GetByIdAsync(id);

        if (leaveMaster == null)
            return false;

        leaveMaster.IsActive = false;
        leaveMaster.IsDeleted = true;
        leaveMaster.UpdatedOn = DateTime.Now;

        await _leaveMasterRepo.UpdateAsync(leaveMaster);
        return true;
    }
    public async Task<bool> LeaveExistsAsync(string Name)
    {
        if (string.IsNullOrWhiteSpace(Name))
            return false;

        Name = Name?.Trim().ToLower() ?? string.Empty;

        return await _leaveMasterRepo.Query().AnyAsync(e =>
            e.LeaveName.ToLower() == Name);
    }

    //Leave Request related methods can be added here as needed
    public async Task<List<LeaveRequest>> GetLeaveRequestAsync()
    {
        return await _leaveRequestRepo.Query()
            .Include(lr => lr.Employee)
            .Where(x => x.IsActive && !x.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<LeaveRequest>> GetLeaveRequestsByStatusAsync(Status status)
    {
        if (status == null)
        {
            return await GetLeaveRequestAsync();
        }
        else
        {
            return await _leaveRequestRepo.Query()
                .Include(lr => lr.Employee)
                .Where(x => x.Status == status && x.IsActive && !x.IsDeleted)
                .OrderByDescending(o => o.Id)
                .ToListAsync();        }
            
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
            .Where(e => e.EmployeeId == employeeId && e.IsActive && !e.IsDeleted && e.Status != Status.Cancelled)
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
        var LeaveRequest = await _leaveRequestRepo.GetByIdAsync(id);

        if (LeaveRequest == null || LeaveRequest.Status == Status.Pending)
            return false;

        LeaveRequest.IsActive = false;
        LeaveRequest.IsDeleted = true;
        LeaveRequest.UpdatedOn = DateTime.Now;

        await _leaveRequestRepo.UpdateAsync(LeaveRequest);
        return true;
    }
    public async Task<Dictionary<string, int>> GetUsedLeavesByTypeAsync(int employeeId)
    {
        return await _leaveRequestRepo.Query()
            .Include(lr => lr.LeaveType)
            .Where(lr =>
                lr.EmployeeId == employeeId &&
                lr.Status == Status.Approved &&
                lr.IsActive &&
                !lr.IsDeleted)
            .GroupBy(lr => lr.LeaveType.LeaveName)
            .Select(g => new
            {
                LeaveName = g.Key,
                UsedDays = g.Sum(x => x.TotalDays)
            })
            .ToDictionaryAsync(x => x.LeaveName, x => x.UsedDays);
    }

    public async Task<bool> UpdateLeaveStatusAsync(int leaveRequestId, Status status, int approvedBy)
    {
        var leaveRequest = await _leaveRequestRepo.Query()
            .FirstOrDefaultAsync(lr =>
                lr.Id == leaveRequestId &&
                lr.IsActive &&
                !lr.IsDeleted);

        if (leaveRequest == null)
            return false;

        if (leaveRequest.Status != Status.Pending)
            return false;

        leaveRequest.Status = status;
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
        if (attendanceRequest.Status != Status.Pending)
            return false;
        attendanceRequest.Status = Status.Cancelled;
        attendanceRequest.UpdatedOn = DateTime.Now;
        await _leaveRequestRepo.UpdateAsync(attendanceRequest);

        return true;
    }

    //Leave Balance related methods can be added here as needed
    public async Task<List<EmployeeLeaveBalance>> GetLeaveBalanceAsync()
    {
        return await _leaveBalanceRepo.Query().ToListAsync();
    }
    public async Task<List<EmployeeLeaveBalance>> GetLeaveBalanceByEmployeeAsync(int id)
    {
        return await _leaveBalanceRepo.Query()
            .Include(x => x.LeaveMaster)
            .Where(elb => elb.EmployeeId == id && elb.IsActive && !elb.IsDeleted)
            .OrderByDescending(elb => elb.Id)
            .ToListAsync();

    }
    public Task<EmployeeLeaveBalance> CreateLeaveBalanceAsync(EmployeeLeaveBalance leaveBalance)
    {
        return _leaveBalanceRepo.AddAsync(leaveBalance);
    }

    public Task<EmployeeLeaveBalance> UpdateLeaveBalanceAsync(EmployeeLeaveBalance leaveBalance)
    {
        return _leaveBalanceRepo.UpdateAsync(leaveBalance);
    }

    public async Task<bool> DeleteLeaveBalanceAsync(int id)
    {
        var leaveBalance = await _leaveBalanceRepo.GetByIdAsync(id);

        if (leaveBalance == null || (leaveBalance.IsActive==false && leaveBalance.IsDeleted==true))
            return false;

        leaveBalance.IsActive = false;
        leaveBalance.IsDeleted = true;
        leaveBalance.UpdatedOn = DateTime.Now;

        await _leaveBalanceRepo.UpdateAsync(leaveBalance);
        return true;
    }
    public async Task<EmployeeLeaveBalance?> GetLatestByEmployeeAndLeaveTypeAsync(int employeeId, int leaveMasterId)
    {
        return await _leaveBalanceRepo.Query()
            .Where(x =>
                x.EmployeeId == employeeId &&
                x.LeaveMasterId == leaveMasterId &&
                x.IsActive &&
                !x.IsDeleted)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();
    }
}
