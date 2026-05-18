using PatientJournal.Core.Entities;
using PatientJournal.Core.Interfaces;
using PatientJournal.Infrastructure;
using Microsoft.EntityFrameworkCore;
using PatientJournal.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<PatientJournalContext>(opt => 
    opt.UseInMemoryDatabase("WebAppDb"));

builder.Services.AddScoped<IRepository<AuditLog>, Repository<AuditLog>>();
builder.Services.AddScoped<IRepository<JournalEntry>, Repository<JournalEntry>>();
builder.Services.AddScoped<IRepository<Patient>, Repository<Patient>>();
builder.Services.AddScoped<IRepository<PatientDocument>, Repository<PatientDocument>>();
builder.Services.AddScoped<IRepository<StaffMember>, Repository<StaffMember>>();
//builder.Services.AddScoped<IItemManager, ItemManager>();
builder.Services.AddTransient<DbInitializer, DbInitializer>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Initialize the database.
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<PatientJournalContext>();
        var dbInitializer = services.GetRequiredService<IDbInitializer>();
        await dbInitializer.InitializeAsync();
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
