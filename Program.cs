using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using MiniMap;

var builder = WebApplication.CreateBuilder(args);
var arg = builder.Configuration["ConnectionStrings:MysqlConnection"];

if(arg !=null)builder.Services.AddDbContext<TreeDbContext>(option => option.UseMySQL(arg));

var app = builder.Build();
// Configure the HTTP request pipeline.


//app.MapGet("/api/treeholes", (TreeDbContext context) => context.TreeInfos.ToList()).WithName("getAll").AddEndpointFilter<EndpointRequestHeadersFilter>().AddEndpointFilter<EndPointFilters>();
//app.MapGet("/api/treeholes/{id:int}", (TreeDbContext context, int id) => context.TreeInfos.Where(e => e.InfoId == id));

//app.MapPost("/api/treeholes", (TreeDbContext context, string arg, int mediaCode) =>
//{
//    TreeHolesInfo info = new(arg, mediaCode);
//    context.Entry<TreeHolesInfo>(info).State = EntityState.Added;
//    return context.SaveChanges();
//});

//app.MapDelete("/api/treeholes/{id:int}", (TreeDbContext context, int id) =>
//{
//    List<TreeHolesInfo> info = context.TreeInfos.Where(e => e.InfoId == id).ToList();
//    context.Entry<TreeHolesInfo>(info[0]).State = EntityState.Deleted;
//    return context.SaveChanges();
//});

app.Map("/", () => "Hello World!Please use the /api/treeholes path");
app.MapGroup("/api/treeholes")
    .MapTreeApi()
    .WithTags("public")
    .AddEndpointFilter<RequestHeadersEndpointFilter>()
    .AddEndpointFilter<EndPointFilters>();

app.UseRouting();

app.Run();