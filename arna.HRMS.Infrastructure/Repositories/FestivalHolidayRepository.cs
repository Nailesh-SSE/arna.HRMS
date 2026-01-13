using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
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
        return await _baseRepository.Query().ToListAsync();
    }

    public async Task<List<FestivalHoliday>> GetByMonthAsync(int year, int month)
    {
        return await _baseRepository.Query()
            .Where(h => h.Date.Year == year && h.Date.Month == month)
            .ToListAsync();
    }
}
