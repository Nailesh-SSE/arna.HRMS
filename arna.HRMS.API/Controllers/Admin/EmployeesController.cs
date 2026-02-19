using arna.HRMS.Core.DTOs;
using arna.HRMS.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers.Admin;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployees()
    {
        var result = await _employeeService.GetEmployeesAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEmployeeById(int id)
    {
        var result = await _employeeService.GetEmployeeByIdAsync(id);

        if (!result.IsSuccess)
            return NotFound(result.Message);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] EmployeeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _employeeService.CreateEmployeeAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        dto.Id = id;

        var result = await _employeeService.UpdateEmployeeAsync(dto);

        if (!result.IsSuccess)
            return BadRequest(result.Message);

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var result = await _employeeService.DeleteEmployeeAsync(id);

        if (!result.IsSuccess)
            return NotFound(result.Message);

        return Ok(result);
    }
}
