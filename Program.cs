using Microsoft.EntityFrameworkCore;
using MiniMap;

var builder = WebApplication.CreateBuilder(args);
var arg = builder.Configuration["ConnectionStrings:MysqlConnection"];

if (arg != null) builder.Services.AddDbContext<TreeDbContext>(option => option.UseMySQL(arg));

var app = builder.Build();
// Configure the HTTP request pipeline.

app.Map("/", () => "Hello World!Please use the /api/treeholes path");
app.MapGroup("/api/treeholes")
    .MapTreeApi()
    .AddEndpointFilter<RequestHeadersEndpointFilter>()
    .AddEndpointFilter<EndPointFilters>();

app.UseRouting();

app.Run();