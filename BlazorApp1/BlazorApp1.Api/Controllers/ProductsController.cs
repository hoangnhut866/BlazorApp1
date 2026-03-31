using BlazorApp1.Api.Data;
using BlazorApp1.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET api/products
    // Requires a valid JWT — any authenticated user (Admin or Member).
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        return Ok(await _context.Products.AsNoTracking().ToListAsync());
    }

    // GET api/products/{id}
    // Requires a valid JWT — any authenticated user (Admin or Member).
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _context.Products.AsNoTracking()
                                             .FirstOrDefaultAsync(p => p.Id == id);
        return product is null ? NotFound() : Ok(product);
    }

    // POST api/products
    // Requires a valid JWT AND the 'Admin' role.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Product>> Create([FromBody] Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT api/products/{id}
    // Requires a valid JWT AND the 'Admin' role.
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
        if (id != product.Id)
            return BadRequest(new { message = "Route id does not match body id." });

        var exists = await _context.Products.AnyAsync(p => p.Id == id);
        if (!exists)
            return NotFound();

        _context.Entry(product).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE api/products/{id}
    // Requires a valid JWT AND the 'Admin' role.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
            return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
