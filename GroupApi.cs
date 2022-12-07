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
            
            int n = context.TreeInfos.Count();
            int m = Comon.Rand(n);
            if (m == 0) return await context.TreeInfos.OrderBy(x => x).Skip(m).Take(m + 4).ToListAsync();
            else return await context.TreeInfos.OrderBy(x => x).Skip(m).Take(n).ToListAsync(); 
        }

        public static async Task<TreeHolesInfo> GetInfo(TreeDbContext context, int id) => await context.TreeInfos.Where(e => e.InfoId == id).FirstAsync<TreeHolesInfo>();

        public static async Task<int> CreateInfo(TreeDbContext context,[FromBody]string msg,int mediaCode=1)
        {
            TreeHolesInfo info = new(msg, mediaCode);
            context.Entry<TreeHolesInfo>(info).State = EntityState.Added;
            return await context.SaveChangesAsync();
        }

        public static async Task<int> DeleteInfo(TreeDbContext context, int id)
        {
            TreeHolesInfo info = await context.TreeInfos.Where(e => e.InfoId == id).FirstAsync<TreeHolesInfo>();
            context.Entry<TreeHolesInfo>(info).State = EntityState.Deleted;
            return await context.SaveChangesAsync();
        }
    }
}
