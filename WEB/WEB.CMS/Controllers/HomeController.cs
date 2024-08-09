using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using DAL.StoreProcedure;
using Entities.ConfigModels;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Repositories.IRepositories;
using WEB.CMS.Customize;
using WEB.CMS.Models;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static DbWorker _DbWorker;
        public HomeController(IOptions<DataBaseConfig> dataBaseConfig, ILogger<HomeController> logger)
        {
            _logger = logger;
            _DbWorker = new DbWorker(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        public IActionResult Index()
        {
            try
            {
                //Assembly asm = Assembly.GetExecutingAssembly();
                //var controlleractionlist = asm.GetTypes()
                //        .Where(type => typeof(Controller).IsAssignableFrom(type))
                //        .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                //        .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
                //        .Select(x => new {
                //            Controller = x.DeclaringType.Name,
                //            Action = x.Name,
                //            ReturnType = x.ReturnType.Name,
                //            Attributes = String.Join(",", x.GetCustomAttributes().Select(a => a.GetType().Name.Replace("Attribute", "")))
                //        })
                //        .OrderBy(x => x.Controller).ThenBy(x => x.Action).ToList();
                var _UserId = "";
                if (HttpContext.User.FindFirst(ClaimTypes.Name) != null)
                {
                    _UserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                }
                ViewBag.UserId = _UserId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return View();
        }

        public IActionResult DataMonitor()
        {
            //return View();
            return RedirectToAction("Error", "Home");
        }

        public IActionResult ExecuteQuery(string dataQuery)
        {
            /*
            try
            {
                var dataSet = _DbWorker.ExecuteSqlString(dataQuery);
             
                if (!string.IsNullOrEmpty(dataQuery))
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Thực thi thành công",
                        dataSet = JsonConvert.SerializeObject(dataSet),
                        tableCount = dataSet.Tables.Count
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Thực thi thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
            */
            return RedirectToAction("Error", "Home");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
