using DocumentValidator.Domain.Entities;
using DocumentValidator.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentValidator.Api.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ClientsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var total = await _db.Clients.CountAsync();
        var items = await _db.Clients.AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = await _db.Clients.FindAsync(id);
        return client is null ? NotFound() : Ok(client);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest req)
    {
        var client = new Client
        {
            Name = req.Name,
            Email = req.Email,
            DocumentType = req.DocumentType,
            DocumentValue = req.DocumentValue,
            Country = req.Country
        };

        _db.Clients.Add(client);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateClientRequest req)
    {
        var client = await _db.Clients.FindAsync(id);
        if (client is null) return NotFound();

        client.Name = req.Name;
        client.Email = req.Email;
        client.DocumentType = req.DocumentType;
        client.DocumentValue = req.DocumentValue;
        client.Country = req.Country;

        await _db.SaveChangesAsync();
        return Ok(client);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var client = await _db.Clients.FindAsync(id);
        if (client is null) return NotFound();
        _db.Clients.Remove(client);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public class CreateClientRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentValue { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
