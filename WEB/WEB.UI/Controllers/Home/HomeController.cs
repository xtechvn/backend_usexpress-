using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WEB.UI.Models;

namespace WEB.UI.Controllers
{    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        public IActionResult Index()
        {
            //Save token in session object
          //  HttpContext.Session.SetString(WEB.UI.Common.Constants.JWToken, "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiTMOqIELhuqNvIE5hbSIsIlVTRVJJRCI6IjE3MzY3IiwiRU1BSUxJRCI6Im1pbmh0YW1sdW9uZ21zY0BnbWFpbC5jb20iLCJQSE9ORSI6IjAxMjM1MDUxLjM0NSIsIlBBU1NXT1JEIjoiZTEwYWRjMzk0OWJhNTlhYmJlNTZlMDU3ZjIwZjg4M2UiLCJSRUZFUlJBTF9JRCI6IjcxNjMwNjM5ODQ4IiwibmJmIjoxNjMyNDk1MTk2LCJleHAiOjE2MzQyMjMxOTYsImlzcyI6Imh0dHBzOi8vbG9jYWxob3N0OjQ0MzgyLyIsImF1ZCI6Imh0dHBzOi8vbG9jYWxob3N0OjQ0MzgyLyJ9.v3P9sra-2nglYqIZhy8u8-zra8-0sk7k70VD6NSTIVw");

            return View();// Content("Đường link không tồn tại");
        }
      


        //public IActionResult Privacy()
        //{
        //    return View();
        //}

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}
