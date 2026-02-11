using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Repositories.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class FestivalHolidayRepository
{
    private readonly IBaseRepository<FestivalHoliday> _baseRepository;

    public FestivalHolidayRepository(IBaseRepository<FestivalHoliday> baseRepository)
    {
        _baseRepository = baseRepository;
    }

    public async Task<List<FestivalHoliday>> GetFestivalHolidayAsync()
    {
        return await _baseRepository.Query()
            .Where(h => h.IsActive && !h.IsDeleted)
            .OrderByDescending(x => x.Id).ToListAsync();
    }

    public async Task<FestivalHoliday?> GetFestivalHolidayByIdAsync(int id)
    {
        return await _baseRepository.Query()
            .FirstOrDefaultAsync(h => h.Id == id && h.IsActive && !h.IsDeleted); 
    }

    public async Task<List<FestivalHoliday>> GetByMonthAsync(int year, int month)
    {
        return await _baseRepository.Query()
            .Where(h => h.Date.Year == year && h.Date.Month == month && h.IsActive && !h.IsDeleted)
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
        var festivalHoliday = await GetFestivalHolidayByIdAsync(id);

        if (festivalHoliday == null)
            return false;

        festivalHoliday.IsActive = false;
        festivalHoliday.IsDeleted = true;
        festivalHoliday.UpdatedOn = DateTime.Now;

        await _baseRepository.UpdateAsync(festivalHoliday);
        return true;
    }

    public async Task<List<FestivalHoliday?>> GetByNameAsync(string name)
    {
        var existingHoliday = await _baseRepository.Query()
            .Where(h => h.FestivalName.ToLower().Trim() == name.ToLower().Trim() && h.IsActive && !h.IsDeleted).ToListAsync();
        return existingHoliday;
    }

    public async Task<List<FestivalHoliday?>> GetByNameAndDateAsync(string name, DateTime date)
    {
        var existingHoliday = await _baseRepository.Query()
            .Where(h => 
                h.FestivalName.ToLower().Trim() == name.ToLower().Trim() 
                && h.IsActive 
                && !h.IsDeleted 
                && h.Date.Date == date.Date
            ).ToListAsync();
        return existingHoliday;
    }

}
