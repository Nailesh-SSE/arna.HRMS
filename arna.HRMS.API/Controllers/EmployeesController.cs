using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Models.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly IMapper _mapper;

    public EmployeesController(IEmployeeService employeeService, IMapper mapper)
    {
        _employeeService = employeeService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
    {
        var employees = await _employeeService.GetEmployeesAsync(); 
        return Ok(_mapper.Map<IEnumerable<EmployeeDto>>(employees));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null) return NotFound();
        return Ok(employee);
    }

    [HttpPost]
    //[Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(CreateEmployeeRequest employeeDto)
    {
        var employee = _mapper.Map<Employee>(employeeDto);
        var createdEmployee = await _employeeService.CreateEmployeeAsync(employee);

        return CreatedAtAction(
            nameof(GetEmployee),
            new { id = createdEmployee.Id },
            _mapper.Map<EmployeeDto>(createdEmployee));
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var deleted = await _employeeService.DeleteEmployeeAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{id}")]
    //[Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeRequest employeeDto)
    {
        if (id != employeeDto.Id) return BadRequest();

        var employee = _mapper.Map<Employee>(employeeDto);

        await _employeeService.UpdateEmployeeAsync(employee);

        return NoContent();
    }
}
