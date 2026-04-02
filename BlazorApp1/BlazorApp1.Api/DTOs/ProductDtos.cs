namespace BlazorApp1.Api.DTOs;

public record CreateProductDto(
    string Name,
    decimal Price,
    string? Description
);

public record UpdateProductDto(
    string Name,
    decimal Price,
    string? Description
);
