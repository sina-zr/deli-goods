using Microsoft.EntityFrameworkCore;
using DeliShop.Data;
using DeliShop.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Apply pending migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}


app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

// Products API
app.MapGet("/api/products", async (ApplicationDbContext context, DateTime? lastUpdate) =>
{
    var query = context.Products.AsQueryable();
    
    if (lastUpdate.HasValue)
    {
        query = query.Where(p => p.UpdatedAt.HasValue && p.UpdatedAt >= lastUpdate.Value);
    }
    
    var products = await query.ToListAsync();
    return Results.Ok(products);
})
.WithName("GetProducts")
.WithOpenApi();

app.MapPost("/api/products", async (ApplicationDbContext context, Product product) =>
{
    if (product.Id > 0)
    {
        // Edit existing product
        var existingProduct = await context.Products.FindAsync(product.Id);
        if (existingProduct == null)
        {
            return Results.NotFound($"Product with ID {product.Id} not found.");
        }
        
        
        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.Stock = product.Stock;
        existingProduct.UpdatedAt = DateTime.UtcNow;
        
        await context.SaveChangesAsync();
        return Results.Ok(existingProduct);
    }
    else
    {
        // Create new product
        product.Id = 0; // Ensure ID is reset for new entity
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = null;
        
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return Results.Created($"/api/products/{product.Id}", product);
    }
})
.WithName("CreateEditProduct")
.WithOpenApi();

app.MapDelete("/api/products/{id}", async (ApplicationDbContext context, long id) =>
{
    var product = await context.Products.FindAsync(id);
    if (product == null)
    {
        return Results.NotFound($"Product with ID {id} not found.");
    }
    
    context.Products.Remove(product);
    await context.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteProduct")
.WithOpenApi();

app.Run();