using DocumentValidator.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentValidator.Api.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _db;

    public EmployeesController(AppDbContext db) => _db = db;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var employees = await _db.Employees
            .AsNoTracking()
            .Select(e => new { e.Id, e.Name, e.Email, e.EmployeeCode, e.Department, e.Role, e.CreatedAt })
            .ToListAsync();

        return Ok(employees);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var e = await _db.Employees.AsNoTracking()
            .Select(x => new { x.Id, x.Name, x.Email, x.EmployeeCode, x.Department, x.Role, x.CreatedAt })
            .FirstOrDefaultAsync(x => x.Id == id);

        return e is null ? NotFound() : Ok(e);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeRequest req)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null) return NotFound();

        employee.Name = req.Name ?? employee.Name;
        employee.Department = req.Department ?? employee.Department;
        employee.Role = req.Role ?? employee.Role;

        await _db.SaveChangesAsync();
        return Ok(new { employee.Id, employee.Name, employee.Department, employee.Role });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null) return NotFound();
        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public class UpdateEmployeeRequest
{
    public string? Name { get; set; }
    public string? Department { get; set; }
    public string? Role { get; set; }
}
