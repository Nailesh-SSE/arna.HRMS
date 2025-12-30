using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;

namespace arna.HRMS.Infrastructure.Repositories;

public class AttendanceRepository
{
    private readonly IBaseRepository<Attendance> _baseRepository;

    public AttendanceRepository(IBaseRepository<Attendance> baseRepository)
    {
        _baseRepository = baseRepository;
    }

    public Task<IEnumerable<Attendance>> GetAttendenceAsync()
    {
        return _baseRepository.GetAllAsync();
    }

    public async Task<Attendance?> GetAttendanceByIdAsync(int id)
    {
        var attendence = await _baseRepository.GetByIdAsync(id).ConfigureAwait(false);
        return attendence;
    }

    public Task<Attendance> CreateAttendanceDtoAsync(Attendance attendance)
    {
        return _baseRepository.AddAsync(attendance);
    }
    public async Task<IEnumerable<Attendance>> GetAttendanceByMonthAsync(int year, int month, int EmpId)
    {
        var allAttendance = await _baseRepository.GetAllAsync();
        return allAttendance.Where(a => a.Date.Year == year && a.Date.Month == month && a.EmployeeId == EmpId);
    }
}
