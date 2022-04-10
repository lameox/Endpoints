using Lameox.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSimpleEndpoints();

var app = builder.Build();
app.UseSimpleEndpoints();

app.MapSimpleEndpoints();

app.Run();
