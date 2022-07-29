using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.EntityFrameworkCore;
using Siberia.CoreWebAPI.Models.EntityDataModel;
using Siberia.CoreWebAPI.Models.NordStreamDb;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var odataBatchHandler = new DefaultODataBatchHandler();
odataBatchHandler.MessageQuotas.MaxNestingDepth = 2;
odataBatchHandler.MessageQuotas.MaxOperationsPerChangeset = 10;
odataBatchHandler.MessageQuotas.MaxReceivedMessageSize = 100;

// Add OData route
builder.Services.AddControllers()
    .AddOData(opt => opt.AddRouteComponents("NordStreamDb", NordStreamDbEntityDataModel.GeEntityTypeDataModel(), odataBatchHandler));

// Add DbContext
builder.Services
    .AddDbContext<NordStreamDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("NordStreamDbEntities")));

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy => { policy.WithOrigins("http://example.com"); });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseODataBatching();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
