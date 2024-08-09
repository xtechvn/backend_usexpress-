using Entities.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace WEB.UI.ViewComponents
{
    public class menuTopBarViewComponent: ViewComponent
    {
        /// <summary>
        /// form dang nhap he thong
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            //HttpContext.Session.SetString(WEB.UI.Common.Constants.JWToken, "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiTMOqIELhuqNvIE5hbSIsIlVTRVJJRCI6IjE3MzY3IiwiRU1BSUxJRCI6Im1pbmh0YW1sdW9uZ21zY0BnbWFpbC5jb20iLCJQSE9ORSI6IjAxMjM1MDUxLjM0NSIsIlBBU1NXT1JEIjoiZTEwYWRjMzk0OWJhNTlhYmJlNTZlMDU3ZjIwZjg4M2UiLCJSRUZFUlJBTF9JRCI6IjcxNjMwNjM5ODQ4IiwibmJmIjoxNjMyNDk1MTk2LCJleHAiOjE2MzQyMjMxOTYsImlzcyI6Imh0dHBzOi8vbG9jYWxob3N0OjQ0MzgyLyIsImF1ZCI6Imh0dHBzOi8vbG9jYWxob3N0OjQ0MzgyLyJ9.v3P9sra-2nglYqIZhy8u8-zra8-0sk7k70VD6NSTIVw");
            //var a = HttpContext.Request.ActionArguments.Keys.FirstOrDefault(x => x.ToString() == "sis_open_login");

            return View();
        }
    }
}
