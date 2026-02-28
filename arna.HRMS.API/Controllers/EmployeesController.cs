using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeesController(IEmployeeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        var result = await _service.GetEmployeesAsync();

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetEmployeeByIdAsync(id);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] EmployeeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.CreateEmployeeAsync(dto);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] EmployeeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != dto.Id)
            return BadRequest("Invalid employee ID.");

        var result = await _service.UpdateEmployeeAsync(dto);

        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteEmployeeAsync(id);

        return result.IsSuccess ? Ok(result) : NotFound(result.Message);
    }
}