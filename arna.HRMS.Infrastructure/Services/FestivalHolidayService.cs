using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class FestivalHolidayService : IFestivalHolidayService
{
    private readonly FestivalHolidayRepository _festivalHolidayRepository;
    private readonly IMapper _mapper;
    private readonly FestivalHolidayValidator _validator;

    public FestivalHolidayService(
        FestivalHolidayRepository holidayRepository,
        IMapper mapper,
        FestivalHolidayValidator validator)
    {
        _festivalHolidayRepository = holidayRepository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<ServiceResult<List<FestivalHolidayDto>>> GetFestivalHolidayAsync()
    {
        var holiday = await _festivalHolidayRepository.GetFestivalHolidayAsync();
        return ServiceResult<List<FestivalHolidayDto>>.Success(_mapper.Map<List<FestivalHolidayDto>>(holiday));
    }

    public async Task<ServiceResult<List<FestivalHolidayDto>>> GetFestivalHolidayByMonthAsync(int year, int month)
    {
        var validation = _validator.ValidationByMonthAndYearAsync(year, month);
        if (!validation.IsValid)
            return ServiceResult<List<FestivalHolidayDto>>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var holidays = await _festivalHolidayRepository.GetByMonthAsync(year, month);
        var list = _mapper.Map<List<FestivalHolidayDto>>(holidays);

        return list.Any()
            ? ServiceResult<List<FestivalHolidayDto>>.Success(list)
            : ServiceResult<List<FestivalHolidayDto>>.Fail("No Data Found");
    }

    public async Task<ServiceResult<FestivalHolidayDto?>> GetFestivalHolidayByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<FestivalHolidayDto?>.Fail("Invalid FestivalHoliday ID");

        var holiday = await _festivalHolidayRepository.GetFestivalHolidayByIdAsync(id);

        if (holiday == null)
            return ServiceResult<FestivalHolidayDto?>.Fail("Festival holiday not found");
        var dto = _mapper.Map<FestivalHolidayDto>(holiday);
        return dto!=null
            ? ServiceResult<FestivalHolidayDto?>.Success(dto)
            : ServiceResult<FestivalHolidayDto?>.Fail("Fail to Finf Festival");
    }

    public async Task<ServiceResult<FestivalHolidayDto>> CreateFestivalHolidayAsync(FestivalHolidayDto dto)
    {
        var validation = await _validator.ValidateCreateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<FestivalHolidayDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var festival = _mapper.Map<FestivalHoliday>(dto);
        var created = await _festivalHolidayRepository.CreateFestivalHolidayAsync(festival);
        var Data = _mapper.Map<FestivalHolidayDto>(created);
        return Data!=null
            ? ServiceResult<FestivalHolidayDto>.Success(Data, "Festival holiday created successfully")
            : ServiceResult<FestivalHolidayDto>.Fail("Fail to create Festival");
    }

    public async Task<ServiceResult<FestivalHolidayDto>> UpdateFestivalHolidayAsync(FestivalHolidayDto dto)
    {
        var validation = await _validator.ValidateUpdateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<FestivalHolidayDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var festival = _mapper.Map<FestivalHoliday>(dto);
        var updated = await _festivalHolidayRepository.UpdateFestivalHolidayAsync(festival);
        var data = _mapper.Map<FestivalHolidayDto>(updated);
        return data!=null
            ? ServiceResult<FestivalHolidayDto>.Success(data, "Festival holiday updated successfully")
            : ServiceResult<FestivalHolidayDto>.Fail("Fail to update Festival");
    }

    public async Task<ServiceResult<bool>> DeleteFestivalHolidayAsync(int id)
    {
        var festival = await GetFestivalHolidayByIdAsync(id);
        if (!festival.IsSuccess || festival.Data == null)
            return ServiceResult<bool>.Fail("Festival holiday not found");

        var deleted = await _festivalHolidayRepository.DeleteFestivalHolidayAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(deleted, "Festival holiday deleted successfully")
            : ServiceResult<bool>.Fail("Fail to delet Festival") ;
    }

    public async Task<ServiceResult<List<FestivalHolidayDto?>>> GetFestivalHolidayByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ServiceResult<List<FestivalHolidayDto?>>.Fail("Festival name is required");

        var holidays = await _festivalHolidayRepository.GetByNameAsync(name);
        var list = _mapper.Map<List<FestivalHolidayDto>>(holidays);

        return list.Any()
            ? ServiceResult<List<FestivalHolidayDto?>>.Success(list)
            : ServiceResult<List<FestivalHolidayDto?>>.Fail("No Data Found");
    }
}
