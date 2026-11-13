using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Repository;
using arna.HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class AttendanceRequestRepository
{
    private readonly IBaseRepository<AttendanceRequest> _baseRepository;
    private readonly ApplicationDbContext _context;

    public AttendanceRequestRepository(
        IBaseRepository<AttendanceRequest> baseRepository,
        ApplicationDbContext? context = null)
    {
        _baseRepository = baseRepository;
        _context = context;
    }

    public async Task<List<AttendanceRequest>> GetAttendanceRequests(int? employeeId, Status? status)
    {
        var query = _baseRepository.Query()
            .AsNoTracking()
            .Where(ar => !ar.IsDeleted);

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
            .AsNoTracking()
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
            .Where(ar => ar.Id == id && !ar.IsDeleted)
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

    public async Task<List<AttendanceRequest>> BulkApproveAttendanceRequestsAsync(List<int> requestIds, int approvedBy)
    {
        if (requestIds == null || !requestIds.Any())
            return new List<AttendanceRequest>();

        // Query matching pending requests using LINQ
        var requestsToApprove = await _context.AttendanceRequest
            .Where(ar => requestIds.Contains(ar.Id) && ar.StatusId == Status.Pending && !ar.IsDeleted)
            .ToListAsync();

        if (!requestsToApprove.Any())
            return new List<AttendanceRequest>();

        var now = DateTime.Now;
        requestsToApprove.ForEach(ar =>
        {
            ar.StatusId = Status.Approved;
            ar.ApprovedBy = approvedBy;
            ar.ApprovedOn = now;
            ar.UpdatedOn = now;
        });

        await _context.SaveChangesAsync();
        return requestsToApprove;
    }

    public async Task<bool> CancelAttendanceRequestAsync(int id, int employeeId)
    {
        var entity = await _baseRepository.Query()
            .FirstOrDefaultAsync(ar =>
                ar.Id == id &&
                ar.EmployeeId == employeeId &&
                !ar.IsDeleted);

        if (entity == null || entity.StatusId != Status.Pending)
            return false;

        entity.StatusId = Status.Cancelled;
        entity.UpdatedOn = DateTime.Now;

        await _baseRepository.UpdateAsync(entity);
        return true;
    }
}
