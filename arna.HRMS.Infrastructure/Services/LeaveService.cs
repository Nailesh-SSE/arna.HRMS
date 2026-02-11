using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class LeaveService : ILeaveService
{
    private readonly LeaveRepository _leaveRepository;
    private readonly IFestivalHolidayService _festivalHoliday;
    private readonly AttendanceRepository _attendanceService;

    private readonly IMapper _mapper;

    public LeaveService(LeaveRepository LeaveRepository, IMapper mapper, IFestivalHolidayService festivalHoliday, AttendanceRepository attendanceService)
    {
        _leaveRepository = LeaveRepository;
        _mapper = mapper;
        _festivalHoliday = festivalHoliday;
        _attendanceService = attendanceService;
    }

    //Leave Type Methods
    public async Task<ServiceResult<List<LeaveTypeDto>>> GetLeaveTypeAsync()
    {
        var leave = await _leaveRepository.GetLeaveTypeAsync();
        var list = _mapper.Map<List<LeaveTypeDto>>(leave);
        return list.Count != 0
            ? ServiceResult<List<LeaveTypeDto>>.Success(list)
            : ServiceResult<List<LeaveTypeDto>>.Fail("Not Found"); ;

    }
    public async Task<ServiceResult<LeaveTypeDto>> GetLeaveTypeByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<LeaveTypeDto>.Fail("Invalid ID");

        var leave = await _leaveRepository.GetLeaveTypeByIdAsync(id);
        if (leave == null)
            return ServiceResult<LeaveTypeDto>.Success(null, "leave not found");

        var data = leave == null ? new LeaveTypeDto() : _mapper.Map<LeaveTypeDto>(leave);
        return data!=null
            ? ServiceResult<LeaveTypeDto>.Success(data)
            : ServiceResult<LeaveTypeDto>.Fail("Not Found"); ;
    }

    public async Task<ServiceResult<LeaveTypeDto>> CreateLeaveTypeAsync(LeaveTypeDto LeaveTypeDto)
    {
        if (LeaveTypeDto == null)
            return ServiceResult<LeaveTypeDto>.Fail("Data not Found");
        
        if (!Enum.IsDefined(typeof(LeaveName), LeaveTypeDto.LeaveNameId))
            return ServiceResult<LeaveTypeDto>.Fail("Leave name is required");

        if (LeaveTypeDto.MaxPerYear <= 0)
            return ServiceResult<LeaveTypeDto>.Fail("number of days is required");

        var existingLeaves = await _leaveRepository.LeaveExistsAsync(LeaveTypeDto.LeaveNameId);
        
        if (existingLeaves)
        {
            return ServiceResult<LeaveTypeDto>.Fail(
                $"Leave '{LeaveTypeDto.LeaveNameId}' already exists"
            );
        }

        var leave = _mapper.Map<LeaveType>(LeaveTypeDto);
        var createdLeaveType = await _leaveRepository.CreateLeaveTypeAsync(leave);
        var Data = _mapper.Map<LeaveTypeDto>(createdLeaveType);
        return Data!=null
            ? ServiceResult<LeaveTypeDto>.Success(Data)
            : ServiceResult<LeaveTypeDto>.Fail("Leave Type Not Found");
    }
    public async Task<ServiceResult<bool>> DeleteLeaveTypeAsync(int id)
    {
        var exist = await GetLeaveTypeByIdAsync(id);
        if(exist == null)
        {
            return ServiceResult<bool>.Fail("Leave Type not found");
        }

        var Data = await _leaveRepository.DeleteLeaveTypeAsync(id);
        return Data
            ?ServiceResult<bool>.Success(Data)
            :ServiceResult<bool>.Fail("Leave Type not found"); ;
    }

    public async Task<ServiceResult<LeaveTypeDto>> UpdateLeaveTypeAsync(LeaveTypeDto LeaveTypeDto)
    {
        if (LeaveTypeDto.Id <= 0 || LeaveTypeDto.LeaveNameId == 0 || LeaveTypeDto.MaxPerYear <=0 )
        {
            return ServiceResult<LeaveTypeDto>.Fail("Failed to update Leave Type");
        }
        if (!Enum.IsDefined(typeof(LeaveName), LeaveTypeDto.LeaveNameId))
            return ServiceResult<LeaveTypeDto>.Fail("Leave name is required");

        var existingLeaveType = await _leaveRepository.GetLeaveTypeByIdAsync(LeaveTypeDto.Id);

        if (existingLeaveType == null)  
            return ServiceResult<LeaveTypeDto>.Fail("No Such Data Found");

        var existingLeaveTypeName = await _leaveRepository.LeaveExistsAsync(LeaveTypeDto.LeaveNameId);

        if (existingLeaveTypeName)
            return ServiceResult<LeaveTypeDto>.Fail($"Leave '{LeaveTypeDto.LeaveNameId}' already exists");

        var leave = _mapper.Map<LeaveType>(LeaveTypeDto);
        var updatedLeaveType = await _leaveRepository.UpdateLeaveTypeAsync(leave);
        if (updatedLeaveType == null)
        {
            return ServiceResult<LeaveTypeDto>.Fail("Leave Type not found");
        }
        
        var Data = _mapper.Map<LeaveTypeDto>(updatedLeaveType);
        return Data!=null
            ? ServiceResult<LeaveTypeDto>.Success(Data)
            : ServiceResult<LeaveTypeDto>.Fail("Leave Type Not Found");
    }

    //Leave Request Methods
    public async Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestAsync()
    {
        var leave = await _leaveRepository.GetLeaveRequestAsync();
        var list = _mapper.Map<List<LeaveRequestDto>>(leave);
        return list.Count != 0
            ? ServiceResult<List<LeaveRequestDto>>.Success(list)
            : ServiceResult<List<LeaveRequestDto>>.Fail("Data Not Found");
    }

    public async Task<ServiceResult<List<LeaveRequestDto>>> GetByFilterAsync(Status? status, int? employeeId)
    {
        var leave = await _leaveRepository.GetLeaveRequestsByFilterAsync(status, employeeId);
        var list = _mapper.Map<List<LeaveRequestDto>>(leave);
        return list.Count != 0
            ? ServiceResult<List<LeaveRequestDto>>.Success(list)
            : ServiceResult<List<LeaveRequestDto>>.Fail("Data Not Found"); ;
    }

    public async Task<ServiceResult<LeaveRequestDto>> GetLeaveRequestByIdAsync(int id)
    {
        if(id<=0)
            return ServiceResult<LeaveRequestDto>.Fail("Invalid ID");

        var leave = await _leaveRepository.GetLeaveRequestByIdAsync(id);
        var Data = leave == null ? new LeaveRequestDto() : _mapper.Map<LeaveRequestDto>(leave);
        return leave != null 
            ? ServiceResult<LeaveRequestDto>.Success(Data) 
            : ServiceResult<LeaveRequestDto>.Fail("leave request not found");
    }

    public async Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestByEmployeeIdAsync(int employeeId)
    {
        if(employeeId <= 0)
            return ServiceResult<List<LeaveRequestDto>>.Fail("Invalid Employee Id");
        var leave = await _leaveRepository.GetLeaveRequestByEmployeeIdAsync(employeeId);
        var Data = _mapper.Map<List<LeaveRequestDto>>(leave);
        return Data.Count!=0
            ? ServiceResult<List<LeaveRequestDto>>.Success(Data)
            : ServiceResult<List<LeaveRequestDto>>.Fail("leave requests not found for this employee");
    }

    public async Task<ServiceResult<LeaveRequestDto>> CreateLeaveRequestAsync(LeaveRequestDto LeaveRequestDto)
    {
        if (
            (LeaveRequestDto.StartDate.Date > LeaveRequestDto.EndDate.Date && LeaveRequestDto.StartDate.Date != LeaveRequestDto.EndDate.Date)
            || LeaveRequestDto.StartDate.Date < DateTime.Now.Date 
            || LeaveRequestDto.EndDate.Date < DateTime.Now.Date
            || LeaveRequestDto.StartDate == default
            || LeaveRequestDto.EndDate == default
           )
        {
            return ServiceResult<LeaveRequestDto>.Fail("Invalid Date you select ");
        }

        if (LeaveRequestDto.LeaveTypeId == 0)
        {
            return ServiceResult<LeaveRequestDto>.Fail("Invalid Leave Type Id");
        }
        if (string.IsNullOrWhiteSpace(LeaveRequestDto.Reason))
        {
            return ServiceResult<LeaveRequestDto>.Fail("Reason is required");
        }

        if (LeaveRequestDto.EmployeeId<=0)
        {
            return ServiceResult<LeaveRequestDto>.Fail("Invalid Employee Id");
        }
               
        var festivalDates = (await _festivalHoliday.GetFestivalHolidayAsync())
                .Data?
                .Select(f => f.Date.Date)
                .ToHashSet()
                ?? new HashSet<DateTime>();

            int actualLeaveDays = CalculateActualLeaveDays(
                LeaveRequestDto.StartDate,
                LeaveRequestDto.EndDate,
                festivalDates);


        LeaveRequestDto.LeaveDays= actualLeaveDays;
        var leave = _mapper.Map<LeaveRequest>(LeaveRequestDto);
        var createdLeaveRequest = await _leaveRepository.CreateLeaveRequestAsync(leave);
        var Data = _mapper.Map<LeaveRequestDto>(createdLeaveRequest);
        return Data!=null
            ? ServiceResult<LeaveRequestDto>.Success(Data)
            : ServiceResult<LeaveRequestDto>.Fail("Not Found");
    }
    public async Task<ServiceResult<bool>> DeleteLeaveRequestAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid ID");
        var Data = await _leaveRepository.DeleteLeaveRequestAsync(id);
        return Data
            ? ServiceResult<bool>.Success(true, "deleted successfully")
            : ServiceResult<bool>.Fail("not found");
    }

    public async Task<ServiceResult<LeaveRequestDto>> UpdateLeaveRequestAsync(LeaveRequestDto LeaveRequestDto)
    {
        if (LeaveRequestDto.Id <=0)
        {
            return ServiceResult<LeaveRequestDto>.Fail("No Data Found");
        }
        var existingLeaveRequest = await _leaveRepository.GetLeaveRequestByIdAsync(LeaveRequestDto.Id);
        if (existingLeaveRequest == null)
        {
            return ServiceResult<LeaveRequestDto>.Fail("No Such Data Found");
        }
        if (
            LeaveRequestDto.StartDate > LeaveRequestDto.EndDate
            || LeaveRequestDto.StartDate.Date <= DateTime.Now.Date
            || LeaveRequestDto.EndDate.Date <= DateTime.Now.Date
            || LeaveRequestDto.StartDate == default
            || LeaveRequestDto.EndDate == default
           )
        {
            return ServiceResult<LeaveRequestDto>.Fail("Invalid Date you select ");
        }
        if (LeaveRequestDto.LeaveTypeId == 0)
        {
            return ServiceResult<LeaveRequestDto>.Fail("Invalid Leave Type Id");
        }
        if (string.IsNullOrWhiteSpace(LeaveRequestDto.Reason))
        {
            return ServiceResult<LeaveRequestDto>.Fail("Reason is required");
        }

        if (LeaveRequestDto.EmployeeId <= 0)
        {
            return ServiceResult<LeaveRequestDto>.Fail("Invalid Employee Id");
        }

        var festivalDates = (await _festivalHoliday.GetFestivalHolidayAsync())
                .Data?
                .Select(f => f.Date.Date)
                .ToHashSet()
                ?? new HashSet<DateTime>();

        int actualLeaveDays = CalculateActualLeaveDays(
            LeaveRequestDto.StartDate,
            LeaveRequestDto.EndDate,
            festivalDates);

        LeaveRequestDto.LeaveDays= actualLeaveDays;

        var Festival = _mapper.Map<LeaveRequest>(LeaveRequestDto);
        var updatedLeaveRequest = await _leaveRepository.UpdateLeaveRequestAsync(Festival);
        var Data = _mapper.Map<LeaveRequestDto>(updatedLeaveRequest);
        return Data!=null
            ? ServiceResult<LeaveRequestDto>.Success(Data)
            : ServiceResult<LeaveRequestDto>.Fail("Not Found");
    }

    public async Task<ServiceResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, Status status, int approvedBy)
    {
        if (leaveRequestId <= 0)
        {
            return ServiceResult<bool>.Fail("Invalid Leave Request Id");
        }

        var updated = await _leaveRepository.UpdateLeaveStatusAsync(leaveRequestId, status, approvedBy);

        if (status == Status.Approved)
        {
            if (!updated || status != Status.Approved)
                return ServiceResult<bool>.Fail("Failed to update leave status or status is not approved.");

            var leaveRequest = await _leaveRepository.GetLeaveRequestByIdAsync(leaveRequestId);

            if (leaveRequest == null)
                return ServiceResult<bool>.Fail("Failed to find Leave Request");

            var festivalDates = (await _festivalHoliday.GetFestivalHolidayAsync())
                .Data?
                .Select(f => f.Date.Date)
                .ToHashSet()
                ?? new HashSet<DateTime>();

            var leaveDates = Enumerable
                .Range(0, (leaveRequest.EndDate.Date - leaveRequest.StartDate.Date).Days + 1)
                .Select(offset => leaveRequest.StartDate.Date.AddDays(offset));

            var workingLeaveDates = leaveDates
                 .Where(date =>
                     date.DayOfWeek != DayOfWeek.Saturday &&
                     date.DayOfWeek != DayOfWeek.Sunday &&
                     !festivalDates.Contains(date)
                 );

            foreach (var date in workingLeaveDates)
            {
                var attendance = new AttendanceDto
                {
                    EmployeeId = leaveRequest.EmployeeId,
                    Date = date,
                    ClockInTime = null,
                    ClockOutTime = null,
                    WorkingHours = TimeSpan.Zero,
                    StatusId = AttendanceStatus.Leave,
                    Notes = leaveRequest.Reason,
                    Latitude = null,
                    Longitude = null
                };
                var dto = _mapper.Map<Attendance>(attendance);
                await _attendanceService.CreateAttendanceAsync(dto);
            }
        }
        return ServiceResult<bool>.Success(true);
    }

    private static int CalculateActualLeaveDays(DateTime startDate, DateTime endDate, HashSet<DateTime> festivalDates)
    {
        int leaveDays = 0;

        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            // Skip weekends
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                continue;

            // Skip festival holidays
            if (festivalDates.Contains(date))
                continue;

            leaveDays++;
        }

        return leaveDays;
    }

    public async Task<ServiceResult<bool>> UpdateLeaveRequestStatusCancelAsync(int id, int employeeId)
    {
        if (id <= 0 || employeeId <= 0)
            return ServiceResult<bool>.Fail("Invalid Id");

        var employeenotExist = await _leaveRepository.GetLeaveRequestByEmployeeIdAsync(employeeId);


        var leaveRequest = await _leaveRepository.GetLeaveRequestByIdAsync(id);
        if (leaveRequest == null)
            return ServiceResult<bool>.Fail("Leave Request not found");
        if (leaveRequest.EmployeeId != employeeId)
            return ServiceResult<bool>.Fail("Unauthorized: Employee does not own this leave request");
        if (leaveRequest.StatusId == Status.Approved)
            return ServiceResult<bool>.Fail("Cannot cancel an approved leave request");
        if (leaveRequest.StatusId == Status.Cancelled)
            return ServiceResult<bool>.Fail("Leave request is already cancelled.");
        if (leaveRequest.StatusId == Status.Rejected)
            return ServiceResult<bool>.Fail("Cannot cancel an Rejected leave request");
        var updated = await _leaveRepository.UpdateLeaveRequestStatusCancel(id, employeeId);
        return updated
            ? ServiceResult<bool>.Success(updated)
            : ServiceResult<bool>.Fail("Not Found");
    }

}
