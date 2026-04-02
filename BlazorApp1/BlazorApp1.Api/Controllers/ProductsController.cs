using BlazorApp1.Api.Data;
using BlazorApp1.Api.DTOs;
using BlazorApp1.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<UpdateProductDto> _updateValidator;

    public ProductsController(
        ApplicationDbContext context,
        IValidator<CreateProductDto> createValidator,
        IValidator<UpdateProductDto> updateValidator)
    {
        _context = context;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
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
    public async Task<ActionResult<Product>> Create([FromBody] CreateProductDto dto)
    {
        var result = await _createValidator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Description = dto.Description
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT api/products/{id}
    // Requires a valid JWT AND the 'Admin' role.
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var result = await _updateValidator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        var product = await _context.Products.FindAsync(id);
        if (product is null)
            return NotFound();

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.Description = dto.Description;

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
