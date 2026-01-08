using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
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

    public async Task<bool> UpdateAttendanceRequestStatusAsync(int id)
    {
        var attendanceStatus= await _baseRepository.GetByIdAsync(id);

        if (attendanceStatus == null)
            return false;

        attendanceStatus.IsApproved = true;
        attendanceStatus.ApprovedBy = DateTime.Now;
        attendanceStatus.UpdatedBy = DateTime.Now;

        await _baseRepository.UpdateAsync(attendanceStatus);
        return true; 
    }
}
