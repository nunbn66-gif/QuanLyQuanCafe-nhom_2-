using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;
namespace QuanLyQuanCafeWeb.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class SessionAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string[] _vaiTroChoPhep;
        public SessionAuthorizeAttribute(params string[] vaiTroChoPhep)
        {
            _vaiTroChoPhep = vaiTroChoPhep ?? Array.Empty<string>();
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var session = context.HttpContext.Session;
            if (session.GetInt32("MaTK") == null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }
            var vaiTro = session.GetString("VaiTro");
            if (vaiTro == null || !_vaiTroChoPhep.Contains(vaiTro, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }
            await next();
        }
    }
}