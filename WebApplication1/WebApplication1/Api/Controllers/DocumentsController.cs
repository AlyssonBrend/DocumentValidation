using DocumentValidator.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentValidator.Api.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public DocumentsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Documents.Include(d => d.ValidatedByEmployee).AsNoTracking();
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.ValidatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var doc = await _db.Documents.Include(d => d.ValidatedByEmployee).FirstOrDefaultAsync(d => d.Id == id);
        return doc is null ? NotFound() : Ok(doc);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await _db.Documents.FindAsync(id);
        if (doc is null) return NotFound();
        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
