using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace internetbursa.Models
{
    public class AdminRoleFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Kullanıcı admin değilse anasayfaya yönlendirilir
            if (HttpContext.Current.Session["UserRole"] == null || HttpContext.Current.Session["UserRole"].ToString() != "Admin")
            {
                // Eğer admin değilse anasayfaya yönlendiriyoruz
                filterContext.Result = new RedirectResult("~/Home/Index");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}