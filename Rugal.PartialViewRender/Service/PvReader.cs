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
        protected async Task<IHtmlContent> RenderAsync(string ViewName, string ViewPath, PvOption Option)
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
    public class PvRender<TView> : PvRender
        where TView : Enum
    {
        public string ViewPath { get; private set; }
        public string JsPath { get; private set; }
        public PvRender(string _ViewPath, string _JsPath, IServiceProvider _ServiceProvider) : base(_ServiceProvider)
        {
            ViewPath = _ViewPath.TrimEnd('/', '\\');
            JsPath = _JsPath.TrimEnd('/', '\\');
        }
        public Task<IHtmlContent> FromAsync(TView ViewName, string PvName = null)
        {
            PvName ??= ViewName.ToString();
            var Option = NewOption(Item =>
            {
                Item.PvName = PvName;
                Item.JsPath = JsPath;
            });
            return RenderAsync(ViewName.ToString(), ViewPath, Option);
        }
        public IHtmlContent From(TView ViewName, string PvName = null)
        {
            return FromAsync(ViewName, PvName).Result;
        }
        private static PvOption NewOption(Action<PvOption> OptionFunc = null)
        {
            var Result = new PvOption();
            if (OptionFunc is not null)
                OptionFunc(Result);

            return Result;
        }
    }
}