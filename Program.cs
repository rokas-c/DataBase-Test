using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Visi produktai
app.MapGet("/", async () =>
{
    try
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync("https://fakestoreapi.com/products");
        return Results.Text(response, "application/json");
    }
    catch (Exception ex)
    {
        return Results.Text($"Error: {ex.Message}", "text/plain", statusCode: 500);
    }
});

// Vienas produktas pagal ID
app.MapGet("/products/{id:int}", async (int id) =>
{
    try
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync($"https://fakestoreapi.com/products/{id}");
        return Results.Text(response, "application/json");
    }
    catch (HttpRequestException ex)
    {
        return Results.Text($"Product not found or API error: {ex.Message}", "text/plain", statusCode: 404);
    }
    catch (Exception ex)
    {
        return Results.Text($"Unexpected error: {ex.Message}", "text/plain", statusCode: 500);
    }
});

app.Run();
