var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
/*builder.Services.AddDbContext<WebshopContext>(opt => 
    opt.UseInMemoryDatabase("WebshopDb"));

builder.Services.AddScoped<IRepository<Item>, ItemRepository>();
builder.Services.AddScoped<IRepository<Cart>, CartRepository>();
builder.Services.AddScoped<IRepository<Order>, OrderRepository>();
builder.Services.AddScoped<IItemManager, ItemManager>();
builder.Services.AddScoped<ICartManager, CartManager>();
builder.Services.AddScoped<IOrderManager, OrderManager>();
builder.Services.AddTransient<IDbInitializer, DbInitializer>();*/

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Initialize the database.
    /*using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<WebshopContext>();
        var dbInitializer = services.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize(dbContext);
    }*/
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
