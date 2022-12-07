using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MiniMap
{
    public static class GroupApi //:RouteGroupBuilder
    {
        public static RouteGroupBuilder MapTreeApi(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAll);
            group.MapGet("/{id}", GetInfo);
            group.MapPost("/", CreateInfo).AddEndpointFilter<FillMediaCodeEndpointFilter>();
            group.MapDelete("/{id}", DeleteInfo);

            return group;
        }

        public static async Task<List<TreeHolesInfo>> GetAll(TreeDbContext context) {
            int m = Comon.Rand(context.TreeInfos.Count());
            return await context.TreeInfos.OrderBy(x => x).Skip(m).Take(m + 4).ToListAsync();
        }

        public static async Task<TreeHolesInfo> GetInfo(TreeDbContext context, int id) => await context.TreeInfos.Where(e => e.InfoId == id).FirstAsync<TreeHolesInfo>();

        public static async Task<TreeHolesInfo> CreateInfo(TreeDbContext context,[FromBody]string msg,int mediaCode=1)
        {
            TreeHolesInfo info = new(msg, mediaCode);
            context.Entry<TreeHolesInfo>(info).State = EntityState.Added;
            await context.SaveChangesAsync();
            return info;
        }

        public static async Task<int> DeleteInfo(TreeDbContext context, int id)
        {
            TreeHolesInfo info = await context.TreeInfos.Where(e => e.InfoId == id).FirstAsync<TreeHolesInfo>();
            context.Entry<TreeHolesInfo>(info).State = EntityState.Deleted;
            return await context.SaveChangesAsync();
        }
    }
}
