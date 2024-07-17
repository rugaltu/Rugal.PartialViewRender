using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Rugal.PartialViewRender.Model;

namespace Rugal.PartialViewRender.Service
{
    public abstract class PvRender
    {
        private readonly IServiceProvider ServiceProvider;
        public PvRender(IServiceProvider _ServiceProvider)
        {
            ServiceProvider = _ServiceProvider;
        }
        protected async Task<IHtmlContent> RenderAsync<TPvType>(string ViewName, string ViewPath, PvOption<TPvType> Option)
            where TPvType : Enum
        {
            var Http = ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            var HttpContext = Http.HttpContext;

            var RouteData = HttpContext.GetRouteData();
            var NewActionContext = new ActionContext(HttpContext, RouteData, new ActionDescriptor());

            var ViewEngine = ServiceProvider.GetRequiredService<IRazorViewEngine>();

            ViewName = ConvertViewName(ViewPath, ViewName);
            var GetView = ViewEngine.GetView(ViewName, ViewName, false) ??
                throw new ArgumentNullException($"{ViewName} is not found");

            if (Option is null)
                throw new Exception("PvOption cannot be null");

            var ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = Option,
            };

            var TempDataProvider = ServiceProvider.GetRequiredService<ITempDataProvider>();
            var NewTempData = new TempDataDictionary(NewActionContext.HttpContext, TempDataProvider);
            using var Writer = new StringWriter();
            var HtmlOption = new HtmlHelperOptions();
            var NewViewContext = new ViewContext(
                NewActionContext, GetView.View, ViewData, NewTempData, Writer, HtmlOption);

            await GetView.View.RenderAsync(NewViewContext);
            return new HtmlString(Writer.ToString());
        }
        private static string ConvertViewName(string ViewPath, string ViewName)
        {
            if (ViewName.Contains('\\') || ViewName.Contains('/'))
                return ViewName;

            if (!ViewName.Contains(".cshtml", StringComparison.CurrentCultureIgnoreCase))
                ViewName = $"{ViewName}.cshtml";

            ViewPath = ViewPath.TrimEnd('/', '\\');
            var Result = $"{ViewPath}/{ViewName}";
            return Result;
        }
    }
    public class PvRender<TPvType> : PvRender
        where TPvType : Enum
    {
        public string ViewPath { get; private set; }
        public PvRender(string _ViewPath, IServiceProvider _ServiceProvider) : base(_ServiceProvider)
        {
            ViewPath = _ViewPath.TrimEnd('/', '\\');
        }
        public Task<IHtmlContent> FromAsync(TPvType ViewName, string PvName = null)
        {
            return BaseFromAsync(ViewName, PvName, out _);
        }
        public Task<IHtmlContent> FromAsync(TPvType ViewName, string PvName, out PvOption<TPvType> Option)
        {
            return BaseFromAsync(ViewName, PvName, out Option);
        }
        public IHtmlContent From(TPvType ViewName, string PvName = null)
        {
            return FromAsync(ViewName, PvName).Result;
        }
        public IHtmlContent From(TPvType ViewName, string PvName, out PvOption<TPvType> Option)
        {
            return FromAsync(ViewName, PvName, out Option).Result;
        }
        private Task<IHtmlContent> BaseFromAsync(TPvType PvType, string PvName, out PvOption<TPvType> Option)
        {
            PvName ??= PvType.ToString();
            Option = new PvOption<TPvType>(PvName, PvType);
            return RenderAsync(PvType.ToString(), ViewPath, Option);
        }
    }
}