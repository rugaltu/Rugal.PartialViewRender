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
using Rugal.PartialViewRender.Models;

namespace Rugal.PartialViewRender.Services;

public abstract class PvRender
{
    private readonly IServiceProvider ServiceProvider;
    public PvRender(IServiceProvider _ServiceProvider)
    {
        ServiceProvider = _ServiceProvider;
    }
    protected async Task<IHtmlContent> RenderAsync<TPvType>(string ViewName, string ViewPath, PvOption<TPvType> Option) where TPvType : Enum
    {
        var Http = ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var HttpContext = Http.HttpContext;

        var RouteData = HttpContext.GetRouteData();
        var NewActionContext = new ActionContext(HttpContext, RouteData, new ActionDescriptor());

        var ViewEngine = ServiceProvider.GetRequiredService<IRazorViewEngine>();
        ViewName = ConvertViewName(ViewPath, ViewName);
        var GetView = ViewEngine
            .GetView(ViewName, ViewName, false) ?? throw new Exception($"{ViewName} is not found");

        ArgumentNullException.ThrowIfNull(Option, nameof(Option));

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
public class PvRender<TPvType> : PvRender where TPvType : Enum
{
    public string ViewPath { get; private set; }
    public PvRender(string _ViewPath, IServiceProvider _ServiceProvider) : base(_ServiceProvider)
    {
        ViewPath = _ViewPath.TrimEnd('/', '\\');
    }
    public IHtmlContent Render(TPvType ViewName)
    {
        return BaseRender(ViewName).Result;
    }
    public IHtmlContent Render(TPvType ViewName, PvOption<TPvType> SetOption)
    {
        return BaseRender(ViewName, SetOption).Result;
    }
    public IHtmlContent Render(TPvType ViewName, Action<PvOption<TPvType>> SetOptionFunc)
    {
        var NewOption = new PvOption<TPvType>(ViewName);
        SetOptionFunc(NewOption);
        return Render(ViewName, NewOption);
    }
    public IHtmlContent Render(TPvType ViewName, out PvOption<TPvType> OutOption)
    {
        OutOption = new PvOption<TPvType>(ViewName);
        return BaseRender(ViewName, OutOption).Result;
    }


    public PvResult<TPvType> Create(TPvType ViewName)
    {
        return new PvResult<TPvType>(this, ViewName);
    }
    public PvResult<TPvType> Create(TPvType ViewName, PvOption<TPvType> SetOption)
    {
        return new PvResult<TPvType>(this, ViewName)
            .WithOption(SetOption);
    }
    public PvResult<TPvType> Create(TPvType ViewName, Action<PvOption<TPvType>> SetOptionFunc)
    {
        var NewOption = new PvOption<TPvType>(ViewName);
        SetOptionFunc(NewOption);
        return Create(ViewName, NewOption);
    }

    public Task<IHtmlContent> FromAsync(TPvType ViewName, string PvName = null)
    {
        var SetOption = new PvOption<TPvType>(ViewName, PvName);
        return BaseRender(ViewName, SetOption);
    }
    public Task<IHtmlContent> FromAsync(TPvType ViewName, string PvName, out PvOption<TPvType> OutOption)
    {
        OutOption = new PvOption<TPvType>(ViewName, PvName);
        return BaseRender(ViewName, OutOption);
    }

    private Task<IHtmlContent> BaseRender(TPvType PvType, PvOption<TPvType> SetOption)
    {
        SetOption ??= new PvOption<TPvType>(PvType);
        return RenderAsync(PvType.ToString(), ViewPath, SetOption);
    }
    private Task<IHtmlContent> BaseRender(TPvType PvType)
    {
        var SetOption = new PvOption<TPvType>(PvType);
        return BaseRender(PvType, SetOption);
    }
}