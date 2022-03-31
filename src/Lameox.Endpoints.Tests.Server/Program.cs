using Lameox.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpoints();

var app = builder.Build();
app.UseEndpoints();


app.MapGet("/", () => "Hello World!");

app.Run();
