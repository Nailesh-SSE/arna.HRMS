using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        try
        {
            var result = await _service.GetEmployeesAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.GetBaseException().Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        try
        {
            var result = await _service.GetEmployeeByIdAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.GetBaseException().Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] EmployeeDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreateEmployeeAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.GetBaseException().Message);
        }
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] EmployeeDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest("Invalid employee ID.");

            var result = await _service.UpdateEmployeeAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.GetBaseException().Message);
        }
    }

    [HttpPost("{id:int}/profile-image")]
    public async Task<IActionResult> UploadProfileImageAsync(int id, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("No image file was uploaded.");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("File size cannot exceed 5 MB.");

            using var stream = file.OpenReadStream();
            var result = await _service.UploadProfileImageAsync(id, stream, file.FileName);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.GetBaseException().Message);
        }
    }

    [HttpPost("{id:int}/delete-profile-image")]
    public async Task<IActionResult> DeleteProfileImageAsync(int id)
    {
        try
        {
            var result = await _service.DeleteProfileImageAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.GetBaseException().Message);
        }
    }

    [HttpPost("{id:int}/delete")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        try
        {
            var result = await _service.DeleteEmployeeAsync(id);
            return result.IsSuccess ? Ok(result) : NotFound(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.GetBaseException().Message);
        }
    }
}
