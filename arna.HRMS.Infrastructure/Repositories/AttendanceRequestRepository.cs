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
        return await _baseRepository.Query()
            .Include(d => d.Employee)
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderByDescending(x => x.Id).ToListAsync();
    }

    public async Task<List<AttendanceRequest>> GetPandingAttendanceRequestes()
    {
        return await _baseRepository.Query()
            .Where(ar => ar.StatusId == Status.Pending && ar.IsActive && !ar.IsDeleted)
            .Include(d => d.Employee)
            .OrderByDescending(d => d.Id)
            .ToListAsync();
    }

    public async Task<AttendanceRequest?> GetAttendanceRequestByIdAsync(int id)
    {
        return await _baseRepository.Query()
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(x=> x.Id == id && x.IsActive && !x.IsDeleted);
    }

    public async Task<List<AttendanceRequest>> GetAttendanceRequestsByEmployee(int employeeId)
    {
        return await _baseRepository.Query()
            .Where(ar => ar.EmployeeId == employeeId && ar.IsActive && !ar.IsDeleted)
            .Include(d => d.Employee)
            .OrderByDescending(d => d.Id)
            .ToListAsync();
    }

    public Task<AttendanceRequest> CreateAttendanceRequestAsync(AttendanceRequest attendance)
    {
        return _baseRepository.AddAsync(attendance);
    }

    public Task<AttendanceRequest> UpdateAttendanceRequestAsync(AttendanceRequest attendanceRequest)
    {
        return _baseRepository.UpdateAsync(attendanceRequest);
    }

    public async Task<bool> UpdateAttendanceRequestStatusAsync(int attendanceRequestId, Status status, int approvedBy)
    {
        var request = await GetAttendanceRequestByIdAsync(attendanceRequestId);

        if (request == null)
            return false;

        if (request.StatusId != Status.Pending)
            return false;

        request.StatusId = status;
        request.ApprovedBy = approvedBy;
        request.ApprovedOn = DateTime.Now;
        request.UpdatedOn = DateTime.Now;

        await _baseRepository.UpdateAsync(request);
        return true;
    }

    public async Task<bool> GetAttendanceRequestCancelAsync(int id, int employeeId)
    {
        var attendanceRequest = await _baseRepository.Query()
            .FirstOrDefaultAsync(ar => ar.Id == id && ar.EmployeeId == employeeId && ar.IsActive && !ar.IsDeleted);

        if (attendanceRequest == null)
            return false;

        if (attendanceRequest.StatusId != Status.Pending)
            return false;

        attendanceRequest.StatusId = Status.Cancelled;
        attendanceRequest.UpdatedOn = DateTime.Now;
        await _baseRepository.UpdateAsync(attendanceRequest);

        return true;
    }
}
