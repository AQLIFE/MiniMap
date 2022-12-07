using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using System.Threading;

namespace MiniMap
{
    [Table("TreeHolesInfo")]
    public class TreeHolesInfo
    {
        [Key]
        public int InfoId { get; set; }

        public string? InfoContext { get; set; }

        [Column("InfoCreated")]
        public DateTime? CreateDate { get; set; }
        [Column("InfoState")]
        public int State { get; set; }

        public TreeHolesInfo(string info, int code)
        {
            this.InfoId = 0;
            this.InfoContext = info;
            this.CreateDate = DateTime.Now;
            this.State = code;
        }

        public TreeHolesInfo() { }
    }

    public class TreeDbContext : DbContext
    {
        public DbSet<TreeHolesInfo>? TreeInfos { get; set; }

        public TreeDbContext(DbContextOptions<TreeDbContext> option) : base(option) { }
    }

    [Description("设备描述代码")]
    public static class MediaCode
    {
        [Description("浏览器")]
        public static int Web = 1;
        [Description("微信")]
        public static int Wx = 2;
        [Description("安卓")]
        public static int Android = 3;
        [Description("电脑")]
        public static int Pc = 4;
        [Description("其他")]
        public static int Other = 5;
    }

    /// <summary>
    /// 用于统一返回信息
    /// </summary>
    public class ResultInfo
    {
        public string message { get; set; }

        public object? data { get; set; }

        public ResultInfo(bool bl, object data)
        {
            this.message = bl ? "success" : "fail";
            this.data = data;
        }

        public ResultInfo(string message, object? data)
        {
            this.message = message;
            this.data = data;
        }

        public ResultInfo(string message)
        {
            this.message = message;
            this.data = null;
        }
    }

    public class ApplicationFilters : IActionFilter
    {
        private static int UnitCheck(string mediaUa)
        {
            string regex = @"WeChat|Android|Windows";
            Regex regex1 = new(regex, RegexOptions.RightToLeft);
            Match li = regex1.Match(mediaUa);
            List<Group> sd = li.Groups.Values.ToList();
            return sd[0].Value switch
            {
                "Android" => MediaCode.Android,
                "Windows" => MediaCode.Pc,
                "WeChat" => MediaCode.Wx,
                _ => MediaCode.Other,
            };
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            HttpRequest request = context.HttpContext.Request;
            Console.WriteLine($"Request:[{request.Method}=>{request.Path}]");
            int mediaCode = request.Headers["User-Agent"] != "" ? UnitCheck(request.Headers["User-Agent"]) : MediaCode.Other;

            #region 添加终结点参数
            KeyValuePair<string, object> objs = new("media", mediaCode);
            context.ActionArguments.Add(objs);
            #endregion
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            HttpRequest request = context.HttpContext.Request;
            ObjectResult result = context.Result as ObjectResult;

            if (context.Canceled) {// 是否被其他 Filters 短路
            } else if (context.Exception != null) {
                ResultInfo info = new(false, "系统不能响应此类操作，请联系系统管理员." + DateTime.Now);
                Console.WriteLine($"Error:[{request.Path}=>{context.Exception.Message}]{DateTime.Now}");
                context.Result = new ObjectResult(info);
            }
            else if (result != null && result.Value != null)
            {
                ResultInfo info = new(true, result.Value);
                Console.WriteLine($"Response:[{request.Path}=>{context.Result}]");
                context.Result = new ObjectResult(info);
            }

        }

    }

    public class EndPointFilters : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext filtersContext, EndpointFilterDelegate next)
        {
            var result = await next(filtersContext);
            
            if (result != null && result is List<TreeHolesInfo> || result is TreeHolesInfo || result is int)
            {
                var obj = new ResultInfo(result != null, result);
                Console.WriteLine($"Response=>[{obj.message}]");
                return obj;
            }
            else return new ResultInfo("后台服务初始化未完成，请联系管理员处理");
        }
    }

    public class RequestHeadersEndpointFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext filtersContext, EndpointFilterDelegate next)
        {
            var request = filtersContext.HttpContext.Request;
            string user = request.Headers["User"].ToString();
            if (user == "" || user == null) return new ResultInfo("你的请求不会被受理，请使用可信客户端进行请求");
            else
            {
                Console.WriteLine($"Request=>[{request.Method}:{request.Path}]-{user}");
                return await next(filtersContext);
            }
        }
    }

    public class FillMediaCodeEndpointFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext filtersContext, EndpointFilterDelegate next)
        {
            var request = filtersContext.HttpContext.Request;
            string userAgent = request.Headers["User-Agent"].ToString();

            filtersContext.Arguments[filtersContext.Arguments.Count - 1] = Comon.UnitCheck(userAgent);
            Console.WriteLine($"mediaCode={filtersContext.GetArgument<int>(filtersContext.Arguments.Count-1)}");
            return await next(filtersContext);
        }
    }

    public class Comon
    {
        public static int Rand(int num) {
            Random rd = new();
            int s = rd.Next(num);
            return s + 4 <= num ? s: 0;
        }

        public static int UnitCheck(string mediaUa)
        {
            string regex = @"WeChat|Android|Windows";

            Regex regex1 = new(regex, RegexOptions.RightToLeft);

            Match li = regex1.Match(mediaUa);

            List<Group> sd = li.Groups.Values.ToList();

            return sd[0].Value switch
            {
                "Android" => MediaCode.Android,
                "Windows" => MediaCode.Pc,
                "WeChat" => MediaCode.Wx,
                _ => MediaCode.Other,
            };
        }
    } 

    //public class ErrorEndpointFilter:IEndpointFilter
    //{
    //    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext filtersContext, EndpointFilterDelegate next)
    //    {
    //        filtersContext.Http
    //    }
    //}
 }
