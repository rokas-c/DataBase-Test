using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

var builder = WebApplication.CreateBuilder(args);

// Konfigūruojame JSON serializavimą
builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
});

var app = builder.Build();

var products = new List<Product>();

app.MapPost("/products", async (HttpContext context) =>
{
    var request = await context.Request.ReadFromJsonAsync<ProductCreateRequest>();
    if (request == null)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid request body.");
        return;
    }

    var product = new Product
    {
        Id = products.Count + 1,
        Title = request.Title,
        Description = request.Description,
        Price = request.Price
    };

    products.Add(product);
    await context.Response.WriteAsJsonAsync(product);
});

app.MapGet("/products/{id:int}", (int id) =>
{
    var product = products.Find(p => p.Id == id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.Run();

public class Product
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public float Price { get; set; }
}

public class ProductCreateRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public float Price { get; set; }
}