using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class FestivalHolidayRepository
{
    private readonly IBaseRepository<FestivalHoliday> _baseRepository;

    public FestivalHolidayRepository(IBaseRepository<FestivalHoliday> baseRepository)
    {
        _baseRepository = baseRepository;
    }

    public async Task<List<FestivalHoliday>> GetFestivalHolidaysAsync()
    {
        return await _baseRepository.Query()
            .Where(h => h.IsActive && !h.IsDeleted)
            .OrderByDescending(h => h.Id)
            .ToListAsync(); 
    }

    public async Task<FestivalHoliday?> GetFestivalHolidayByIdAsync(int id)
    {
        return await _baseRepository.Query()
            .Where(h => h.Id == id && h.IsActive && !h.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<List<FestivalHoliday>> GetByMonthAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        return await _baseRepository.Query()
            .Where(h =>
                h.IsActive &&
                !h.IsDeleted &&
                h.Date >= startDate &&
                h.Date < endDate)
            .ToListAsync();
    }

    public Task<FestivalHoliday> CreateFestivalHolidayAsync(FestivalHoliday festivalHoliday)
    {
        return _baseRepository.AddAsync(festivalHoliday);
    }

    public Task<FestivalHoliday> UpdateFestivalHolidayAsync(FestivalHoliday festivalHoliday)
    {
        return _baseRepository.UpdateAsync(festivalHoliday);
    }

    public async Task<bool> DeleteFestivalHolidayAsync(int id)
    {
        var festivalHoliday = await _baseRepository.Query()
            .FirstOrDefaultAsync(h =>
                h.Id == id &&
                h.IsActive &&
                !h.IsDeleted);

        if (festivalHoliday == null)
            return false;

        festivalHoliday.IsActive = false;
        festivalHoliday.IsDeleted = true;
        festivalHoliday.UpdatedOn = DateTime.Now;

        await _baseRepository.UpdateAsync(festivalHoliday);
        return true;
    }

    public async Task<List<FestivalHoliday>> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new List<FestivalHoliday>();

        name = name.Trim().ToLower();

        return await _baseRepository.Query()
            .Where(h =>
                h.IsActive &&
                !h.IsDeleted &&
                h.FestivalName.Trim().ToLower() == name)
            .ToListAsync();
    }

    public async Task<List<FestivalHoliday>> GetByNameAndDateAsync(string name, DateTime date, int id)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new List<FestivalHoliday>();

        name = name.Trim().ToLower();

        var start = date.Date;
        var end = start.AddDays(1);

        return await _baseRepository.Query()
            .Where(h =>
                h.IsActive &&
                !h.IsDeleted &&
                h.Id != id &&
                h.FestivalName.Trim().ToLower() == name &&
                h.Date >= start &&
                h.Date < end)
            .ToListAsync();
    }
}