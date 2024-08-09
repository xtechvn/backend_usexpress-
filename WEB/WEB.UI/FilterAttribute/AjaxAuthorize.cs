using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace WEB.UI.FilterAttribute
{
    //custom AjaxAuthorize filter inherits from ActionFilterAttribute because there is an issue with 
    //a inheriting from AuthorizeAttribute.
    //post about issue: 
    //https://stackoverflow.com/questions/64017688/custom-authorization-filter-not-working-in-asp-net-core-3

    //The statuses for AJAX calls are handled in InitializeGlobalAjaxEventHandlers JS function.

    //While this filter was made to be used on actions that are called by AJAX, it can also handle
    //authorization not called through AJAX.
    //When using this filter always place it above any others as it is not guaranteed to run first.

    //usage: [AjaxAuthorize(new[] {"RoleName", "AnotherRoleName"})]
    public class AjaxAuthorize : ActionFilterAttribute
    {
        //public string[] Roles { get; set; }

        //public AjaxAuthorize(params string[] roles)
        //{
        //    Roles = roles;
        //}

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string back_link = context.HttpContext.Request.Path.Value;            

            string signInPageUrl = "/account/login-popup/" + System.Net.WebUtility.UrlEncode(back_link); // Chuyển về trang chủ và bật Lightbox login lên
            
            //Session.SetString(WEB.UI.Common.Constants.JWToken, "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiTMOqIELhuqNvIE5hbSIsIlVTRVJJRCI6IjE3MzY3IiwiRU1BSUxJRCI6Im1pbmh0YW1sdW9uZ21zY0BnbWFpbC5jb20iLCJQSE9ORSI6IjAxMjM1MDUxLjM0NSIsIlBBU1NXT1JEIjoiZTEwYWRjMzk0OWJhNTlhYmJlNTZlMDU3ZjIwZjg4M2UiLCJSRUZFUlJBTF9JRCI6IjcxNjMwNjM5ODQ4IiwibmJmIjoxNjMyNDk1MTk2LCJleHAiOjE2MzQyMjMxOTYsImlzcyI6Imh0dHBzOi8vbG9jYWxob3N0OjQ0MzgyLyIsImF1ZCI6Imh0dHBzOi8vbG9jYWxob3N0OjQ0MzgyLyJ9.v3P9sra-2nglYqIZhy8u8-zra8-0sk7k70VD6NSTIVw");   

            // string notAuthorizedUrl = "/account/login";
            bool IsAjaxRequest = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                if (IsAjaxRequest)
                {
                    context.HttpContext.Response.StatusCode = 403;
                    JsonResult jsonResult = new JsonResult(new { is_show_popup_login = true, msg = "Xin vui lòng đăng nhập tài khoản", back_link = back_link });
                    context.Result = jsonResult;
                }
                else
                {
                    context.Result = new RedirectResult(signInPageUrl);
                }
            }
        }

    }
}
