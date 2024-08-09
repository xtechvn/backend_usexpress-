using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace WEB.UI.Service
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(string viewName, object model);
    }

    public class ViewRenderService : IViewRenderService
    {
        //private readonly IRazorViewEngine _razorViewEngine;
        //private readonly ITempDataProvider _tempDataProvider;
        //private readonly IServiceProvider _serviceProvider;

        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IActionContextAccessor _actionContext;
        private readonly IRazorPageActivator _activator;

        //public ViewRenderService(IRazorViewEngine razorViewEngine,
        //    ITempDataProvider tempDataProvider,
        //    IServiceProvider serviceProvider
        //)
        //{
        //    _razorViewEngine = razorViewEngine;
        //    _tempDataProvider = tempDataProvider;
        //    _serviceProvider = serviceProvider;

        //}
        public ViewRenderService(IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContext,
            IRazorPageActivator activator,
            IActionContextAccessor actionContext)
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;

            _httpContext = httpContext;
            _actionContext = actionContext;
            _activator = activator;
        }

        public async Task<string> RenderToStringAsync(string pageName, object model)
        {
            var actionContext =
                new ActionContext(
                    _httpContext.HttpContext,
                    _httpContext.HttpContext.GetRouteData(),
                    _actionContext.ActionContext.ActionDescriptor
                );

            using (var sw = new StringWriter())
            {
                var result = _razorViewEngine.FindPage(actionContext, pageName);

                if (result.Page == null)
                {
                    throw new ArgumentNullException($"The page {pageName} cannot be found.");
                }

                var view = new RazorView(_razorViewEngine,
                    _activator,
                    new List<IRazorPage>(),
                    result.Page,
                    HtmlEncoder.Default,
                    new DiagnosticListener("ViewRenderService"));


                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                    {
                        Model = model
                    },
                    new TempDataDictionary(
                        _httpContext.HttpContext,
                        _tempDataProvider
                    ),
                    sw,
                    new HtmlHelperOptions()
                );


                var page = (Page)result.Page;
                page.PageContext = new PageContext
                {
                    ViewData = viewContext.ViewData
                };

                page.ViewContext = viewContext;


                _activator.Activate(page, viewContext);
                await page.ExecuteAsync();

                return sw.ToString();
            }
        }
        //public async Task<string> RenderToStringAsync(string viewName, object model)
        //{
        //    var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
        //    //var actionContext = new ActionContext(_contextAccessor.HttpContext, _contextAccessor.HttpContext.GetRouteData(), new ActionDescriptor());

        //    var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        //    using (var sw = new StringWriter())
        //    {
        //        var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);

        //        if (viewResult.View == null)
        //        {
        //            throw new ArgumentNullException($"{viewName} does not match any available view");
        //        }

        //        var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        //        {
        //            Model = model
        //        };
        //        var viewContext = new ViewContext(
        //            actionContext,
        //            viewResult.View,
        //            viewDictionary,
        //            new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
        //            sw,
        //            new HtmlHelperOptions()
        //        );

        //        await viewResult.View.RenderAsync(viewContext);
        //        return sw.ToString();
        //    }
        //}

    }
}
