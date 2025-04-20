using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rugal.PartialViewRender.Models;
using Rugal.PartialViewRender.Services;

namespace Rugal.PartialViewRender.Extentions;

public static class StartupExtention
{
    public static IServiceCollection AddPartialViews<TViewEnum>(this IServiceCollection Services, IConfiguration Configuration)
        where TViewEnum : Enum
    {
        var ViewName = typeof(TViewEnum).Name;
        var ViewPath = Configuration[$"PartialViews:{ViewName}"];

        Services.AddHttpContextAccessor();
        Services.AddSingleton(Provider =>
        {
            var Reader = new PvRender<TViewEnum>(ViewPath, Provider);
            return Reader;
        });
        Services.AddScoped<PvLayoutStore>();
        return Services;
    }
}