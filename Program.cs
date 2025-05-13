using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

var builder = WebApplication.CreateBuilder(args);

// Konfigūruojame JSON serializavimą
builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
});

var app = builder.Build();

// In-memory produktų sąrašas
var products = new List<Product>();

// 1. Sukurti naują produktą (POST)
app.MapPost("/products", async (HttpContext context) =>
{
    var request = await context.Request.ReadFromJsonAsync<ProductCreateRequest>();
    if (request == null || string.IsNullOrEmpty(request.Title))
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

    context.Response.StatusCode = 201;
    await context.Response.WriteAsJsonAsync(product);
});

// 2. Gauti visų produktų sąrašą (GET)
app.MapGet("/products", () => Results.Ok(products));

// 3. Gauti konkretų produktą pagal ID (GET)
app.MapGet("/products/{id:int}", (int id) =>
{
    var product = products.Find(p => p.Id == id);
    return product is not null
        ? Results.Ok(product)
        : Results.NotFound($"Product with ID {id} not found.");
});

// 4. Atnaujinti produktą pagal ID (PUT)
app.MapPut("/products/{id:int}", async (HttpContext context, int id) =>
{
    var existingProduct = products.Find(p => p.Id == id);
    if (existingProduct == null)
    {
        return Results.NotFound($"Product with ID {id} not found.");
    }

    var updateRequest = await context.Request.ReadFromJsonAsync<ProductUpdateRequest>();
    if (updateRequest == null)
    {
        return Results.BadRequest("Invalid request body.");
    }

    if (!string.IsNullOrEmpty(updateRequest.Title))
    {
        existingProduct.Title = updateRequest.Title;
    }

    if (!string.IsNullOrEmpty(updateRequest.Description))
    {
        existingProduct.Description = updateRequest.Description;
    }

    if (updateRequest.Price.HasValue)
    {
        existingProduct.Price = updateRequest.Price.Value;
    }

    return Results.Ok(existingProduct);
});

// 5. Ištrinti produktą pagal ID (DELETE)
app.MapDelete("/products/{id:int}", (int id) =>
{
    var productToRemove = products.Find(p => p.Id == id);
    if (productToRemove == null)
    {
        return Results.NotFound($"Product with ID {id} not found.");
    }

    products.Remove(productToRemove);
    return Results.Ok($"Product with ID {id} was deleted.");
});

app.Run();

// Modeliai
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

public class ProductUpdateRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public float? Price { get; set; }
}