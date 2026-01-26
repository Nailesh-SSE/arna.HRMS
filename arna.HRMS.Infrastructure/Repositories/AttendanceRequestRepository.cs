using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class AttendanceRequestRepository
{
    private readonly IBaseRepository<AttendanceRequest> _baseRepository;

    public AttendanceRequestRepository(IBaseRepository<AttendanceRequest> baseRepository)
    {
        _baseRepository = baseRepository;
    }

    public async Task<List<AttendanceRequest>> GetAttendanceRequestAsync()
    {
        return await _baseRepository.Query().Include(d=>d.Employee).ToListAsync();
    }

    public async Task<AttendanceRequest?> GetAttendanceRequestByIdAsync(int id)
    {
        var attendence = await _baseRepository.GetByIdAsync(id);
        return attendence;
    }

    public Task<AttendanceRequest> CreateAttendanceRequestAsync(AttendanceRequest attendance)
    {
        return _baseRepository.AddAsync(attendance);
    }

    public Task<AttendanceRequest> UpdateAttendanceRequestAsync(AttendanceRequest attendanceRequest)
    {
        return _baseRepository.UpdateAsync(attendanceRequest);
    }

    public async Task<bool> UpdateAttendanceRequestStatusAsync(int AttendanceRequestId, Status status, int approvedBy)
    {
        var Request = await _baseRepository.Query()
            .FirstOrDefaultAsync(lr =>
                lr.Id == AttendanceRequestId &&
                lr.IsActive &&
                !lr.IsDeleted);

        if (Request == null)
            return false;

        if (Request.Status != Status.Pending)
            return false;

        Request.Status = status;
        Request.ApprovedBy = approvedBy;
        Request.ApprovedOn = DateTime.Now;
        Request.UpdatedOn = DateTime.Now;

        await _baseRepository.UpdateAsync(Request);
        return true;
    }

    public async Task<bool> GetAttendanceRequestCancelAsync(int id, int employeeId)
    {
        var attendanceRequest = await _baseRepository.Query()
            .FirstOrDefaultAsync(ar => ar.Id == id && ar.EmployeeId == employeeId && ar.IsActive && !ar.IsDeleted);
        if (attendanceRequest == null)
            return false;
        if (attendanceRequest.Status != Status.Pending)
            return false;
        attendanceRequest.Status = Status.Cancelled;
        attendanceRequest.UpdatedOn = DateTime.Now;
        await _baseRepository.UpdateAsync(attendanceRequest);

        return true;
    }
}
