using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Infrastructure.Services;
using arna.HRMS.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService __departmentService)
    {
       _departmentService = __departmentService;
    }

    [HttpGet]
    public async Task<ActionResult<DepartmentDto>> GetDepartment()
    {
        var department = await _departmentService.GetDepartmentAsync(); 
        return Ok(department);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDepartment(int id)
    {
        var department = await _departmentService.GetDepartmentByIdAsync(id);
        return department == null ? NotFound("Department not found") : Ok(department);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDepartment([FromBody] DepartmentDto departmentDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var createdDepartment = await _departmentService.CreateDepartmentAsync(departmentDto);

        return Ok(createdDepartment);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var deleted = await _departmentService.DeleteDepartmentAsync(id);
        return deleted
            ? Ok()
            : NotFound("Department not found"); ;
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] DepartmentDto departmentDto)
    {
        if (id != departmentDto.Id)
            return BadRequest("Invalid User ID");
        var updated=await _departmentService.UpdateDepartmentAsync(departmentDto);

        return Ok(updated);
    }
}
