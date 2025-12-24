using arna.HRMS.Core.DTOs.Requests;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Models.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly IMapper _mapper;

    public DepartmentController(IDepartmentService __departmentService, IMapper mapper)
    {
       _departmentService = __departmentService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartment()
    {
        var department = await _departmentService.GetDepartmentAsync(); 
        return Ok(_mapper.Map<IEnumerable<DepartmentDto>>(department));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
    {
        var department = await _departmentService.GetDepartmentByIdAsync(id);
        if (department == null) return NotFound();
        return Ok(department);
    }

    [HttpPost]
    //[Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment(CreateDepartmentRequest departmentDto)
    {
        var department = _mapper.Map<Department>(departmentDto);
        var createdDepartment = await _departmentService.CreateDepartmentAsync(department);

        return CreatedAtAction(
            nameof(GetDepartment),
            new { id = createdDepartment.Id },
            _mapper.Map<DepartmentDto>(createdDepartment));
    }
    [HttpDelete]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var deleted = await _departmentService.DeleteDepartmentAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{id}")]
    //[Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdateDepartment(int id, UpdateDepartmentRequest departmentDto)
    {
        if (id != departmentDto.Id) return BadRequest();

        var department = _mapper.Map<Department>(departmentDto);

        await _departmentService.UpdateDepartmentAsync(department);

        return NoContent();
    }
}
