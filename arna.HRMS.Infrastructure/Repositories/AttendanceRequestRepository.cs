using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class AttendanceRequestRepository
{
    private readonly IBaseRepository<AttendanceRequest> _baseRepository;

    public AttendanceRequestRepository(IBaseRepository<AttendanceRequest> baseRepository)
    {
        _baseRepository = baseRepository;
    }

    public async Task<List<AttendanceRequest>> GetAttendanceRequests(int? employeeId, Status? status)
    {
        var query = _baseRepository.Query()
            .Where(ar => ar.IsActive && !ar.IsDeleted);

        if (employeeId.HasValue)
            query = query.Where(ar => ar.EmployeeId == employeeId.Value);

        if (status.HasValue)
            query = query.Where(ar => ar.StatusId == status.Value);

        return await query
            .Include(ar => ar.Employee)
            .OrderByDescending(ar => ar.Id)
            .ToListAsync();
    }

    public async Task<List<AttendanceRequest>> GetPendingAttendanceRequests()
    {
        return await _baseRepository.Query()
            .Where(ar =>
                ar.IsActive &&
                !ar.IsDeleted &&
                ar.StatusId == Status.Pending)
            .Include(ar => ar.Employee)
            .OrderByDescending(ar => ar.Id)
            .ToListAsync();
    }

    public async Task<AttendanceRequest?> GetAttendanceRequestByIdAsync(int id)
    {
        return await _baseRepository.Query()
            .Where(ar => ar.Id == id && ar.IsActive && !ar.IsDeleted)
            .Include(ar => ar.Employee)
            .FirstOrDefaultAsync();
    }

    public async Task<AttendanceRequest> CreateAttendanceRequestAsync(AttendanceRequest entity)
    {
        return await _baseRepository.AddAsync(entity);
    }

    public async Task<AttendanceRequest> UpdateAttendanceRequestAsync(AttendanceRequest entity)
    {
        return await _baseRepository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAttendanceRequestAsync(int id)
    {
        var entity = await _baseRepository.Query()
            .FirstOrDefaultAsync(ar =>
                ar.Id == id &&
                ar.IsActive &&
                !ar.IsDeleted);

        if (entity == null)
            return false;

        entity.IsActive = false;
        entity.IsDeleted = true;
        entity.UpdatedOn = DateTime.Now;

        await _baseRepository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> UpdateAttendanceRequestStatusAsync(int id, Status status, int approvedBy)
    {
        var entity = await _baseRepository.Query()
            .FirstOrDefaultAsync(ar =>
                ar.Id == id &&
                ar.IsActive &&
                !ar.IsDeleted);

        if (entity == null)
            return false;

        entity.StatusId = status;
        entity.ApprovedBy = approvedBy;
        entity.ApprovedOn = DateTime.Now;
        entity.UpdatedOn = DateTime.Now;

        await _baseRepository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> CancelAttendanceRequestAsync(int id, int employeeId)
    {
        var entity = await _baseRepository.Query()
            .FirstOrDefaultAsync(ar =>
                ar.Id == id &&
                ar.EmployeeId == employeeId &&
                ar.IsActive &&
                !ar.IsDeleted);

        if (entity == null || entity.StatusId != Status.Pending)
            return false;

        entity.StatusId = Status.Cancelled;
        entity.UpdatedOn = DateTime.Now;

        await _baseRepository.UpdateAsync(entity);
        return true;
    }
}