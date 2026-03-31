using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using BlazorApp1.Client.Models;
using Microsoft.Extensions.Http;

namespace BlazorApp1.Client.Services;

public interface IProductService
{
    Task<ApiResult<List<Product>>> GetAllAsync();
    Task<ApiResult<Product>> GetByIdAsync(int id);
    Task<ApiResult<Product>> CreateAsync(Product product);
    Task<ApiResult<bool>> UpdateAsync(Product product);
    Task<ApiResult<bool>> DeleteAsync(int id);
}

/// <summary>Discriminated union returned by every product service call.</summary>
public record ApiResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    public bool IsUnauthorized { get; init; }
    public bool IsForbidden { get; init; }

    public static ApiResult<T> Ok(T data) => new() { IsSuccess = true, Data = data };
    public static ApiResult<T> Fail(string error) => new() { IsSuccess = false, Error = error };
    public static ApiResult<T> Unauthorized() => new() { IsSuccess = false, IsUnauthorized = true, Error = "You are not logged in." };
    public static ApiResult<T> Forbidden() => new() { IsSuccess = false, IsForbidden = true, Error = "You do not have permission to perform this action." };
}

public class ProductService : IProductService
{
    private readonly HttpClient _http;

    public ProductService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("ApiClient");
    }

    public async Task<ApiResult<List<Product>>> GetAllAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/products");
            return await HandleListResponse(response);
        }
        catch (HttpRequestException)
        {
            return ApiResult<List<Product>>.Fail("Unable to reach the server.");
        }
    }

    public async Task<ApiResult<Product>> GetByIdAsync(int id)
    {
        try
        {
            var response = await _http.GetAsync($"api/products/{id}");
            return await HandleSingleResponse(response);
        }
        catch (HttpRequestException)
        {
            return ApiResult<Product>.Fail("Unable to reach the server.");
        }
    }

    public async Task<ApiResult<Product>> CreateAsync(Product product)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/products", product);
            return await HandleSingleResponse(response);
        }
        catch (HttpRequestException)
        {
            return ApiResult<Product>.Fail("Unable to reach the server.");
        }
    }

    public async Task<ApiResult<bool>> UpdateAsync(Product product)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/products/{product.Id}", product);
            return await HandleBoolResponse(response);
        }
        catch (HttpRequestException)
        {
            return ApiResult<bool>.Fail("Unable to reach the server.");
        }
    }

    public async Task<ApiResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/products/{id}");
            return await HandleBoolResponse(response);
        }
        catch (HttpRequestException)
        {
            return ApiResult<bool>.Fail("Unable to reach the server.");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task<ApiResult<List<Product>>> HandleListResponse(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return ApiResult<List<Product>>.Unauthorized();
        if (response.StatusCode == HttpStatusCode.Forbidden)
            return ApiResult<List<Product>>.Forbidden();
        if (!response.IsSuccessStatusCode)
            return ApiResult<List<Product>>.Fail($"Server error: {(int)response.StatusCode}");

        var data = await response.Content.ReadFromJsonAsync<List<Product>>() ?? [];
        return ApiResult<List<Product>>.Ok(data);
    }

    private static async Task<ApiResult<Product>> HandleSingleResponse(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return ApiResult<Product>.Unauthorized();
        if (response.StatusCode == HttpStatusCode.Forbidden)
            return ApiResult<Product>.Forbidden();
        if (response.StatusCode == HttpStatusCode.NotFound)
            return ApiResult<Product>.Fail("Product not found.");
        if (!response.IsSuccessStatusCode)
            return ApiResult<Product>.Fail($"Server error: {(int)response.StatusCode}");

        var data = await response.Content.ReadFromJsonAsync<Product>();
        return data is null
            ? ApiResult<Product>.Fail("Empty response from server.")
            : ApiResult<Product>.Ok(data);
    }

    private static Task<ApiResult<bool>> HandleBoolResponse(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return Task.FromResult(ApiResult<bool>.Unauthorized());
        if (response.StatusCode == HttpStatusCode.Forbidden)
            return Task.FromResult(ApiResult<bool>.Forbidden());
        if (!response.IsSuccessStatusCode)
            return Task.FromResult(ApiResult<bool>.Fail($"Server error: {(int)response.StatusCode}"));

        return Task.FromResult(ApiResult<bool>.Ok(true));
    }
}
