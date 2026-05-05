using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Shared;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase {
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db) => _db = db;

    [HttpGet] public async Task<ActionResult> Get() => Ok(await _db.Products.ToListAsync());
    
    [HttpPost] public async Task<ActionResult> Post(Product p) {
        _db.Products.Add(p);
        await _db.SaveChangesAsync();
        return Ok(p);
    }

    [HttpPut] public async Task<ActionResult> Put(Product p) {
        _db.Entry(p).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")] public async Task<ActionResult> Delete(int id) {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound();
        _db.Products.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
