using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Repositories.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class AttendanceRepository
{
    private readonly IBaseRepository<Attendance> _baseRepository;

    public AttendanceRepository(IBaseRepository<Attendance> baseRepository)
    {
        _baseRepository = baseRepository;
    }
    
    public async Task<List<Attendance>> GetAttendenceAsync()
    {
        return await _baseRepository.Query()
            .Include(x => x.Employee)
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<Attendance?> GetAttendanceByIdAsync(int id)
    {
        var attendence = await _baseRepository.GetByIdAsync(id);
        return attendence;
    }

    public Task<Attendance> CreateAttendanceAsync(Attendance attendance)
    {
        return _baseRepository.AddAsync(attendance);
    }

    public async Task<IEnumerable<Attendance>> GetAttendanceByMonthAsync(int year, int month, int EmpId)
    {
        var allAttendance = await _baseRepository.GetAllAsync();
        return allAttendance.Where(a => a.Date.Year == year && a.Date.Month == month && a.EmployeeId == EmpId);
    }

    public async Task<DateTime?> GetLastAttendanceDateAsync(int employeeId)
    {
        return await _baseRepository.Query()
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.Date)
            .Select(x => (DateTime?)x.Date.Date)
            .FirstOrDefaultAsync();
    }

    public async Task<Attendance?> GetLastAttendanceTodayAsync(int employeeId)
    {
        return await _baseRepository.Query()
            .Where(a =>
                a.EmployeeId == employeeId &&
                a.Date.Date == DateTime.Today)
            .OrderByDescending(a => a.Id) 
            .FirstOrDefaultAsync();
    }

}
