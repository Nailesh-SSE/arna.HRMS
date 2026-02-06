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
        return await _baseRepository.Query().OrderByDescending(x => x.Id).ToListAsync();
    }

    public async Task<List<FestivalHoliday>> GetByMonthAsync(int year, int month)
    {
        return await _baseRepository.Query()
            .Where(h => h.Date.Year == year && h.Date.Month == month)
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
        var festivalHoliday = await _baseRepository.GetByIdAsync(id);

        if (festivalHoliday == null)
            return false;

        festivalHoliday.IsActive = false;
        festivalHoliday.IsDeleted = true;
        festivalHoliday.UpdatedOn = DateTime.Now;

        await _baseRepository.UpdateAsync(festivalHoliday);
        return true;
    }

    public async Task<FestivalHoliday?> GetByIdAsync(int id)
    {
        var data =await _baseRepository.GetByIdAsync(id);

        if (data == null || !data.IsActive || data.IsDeleted)
            return null;
        return data;
    }


    public async Task<FestivalHoliday?> GetByNameAsync(string name)
    {
        return await _baseRepository.Query()
            .FirstOrDefaultAsync(h => h.FestivalName.ToLower().Trim() == name.ToLower().Trim() && h.IsActive && !h.IsDeleted);
    }
}
