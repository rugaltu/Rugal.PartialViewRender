using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rugal.PartialViewRender.Models;
using Rugal.PartialViewRender.Services;

namespace Rugal.PartialViewRender.Extensions;

public static class StartupExtension
{
    public static IServiceCollection AddPartialViews<TPvs>(this IServiceCollection Services, string Path)
        where TPvs : Enum
    {
        if (string.IsNullOrWhiteSpace(Path))
            throw new ArgumentNullException(nameof(Path));

        Services.AddHttpContextAccessor();
        Services.AddScoped(Provider =>
        {
            var Reader = new PvRender<TPvs>(Path, Provider);
            return Reader;
        });
        Services.AddScoped<PvLayoutStore>();
        Services.AddScoped<PvGlobalSlotsStore<TPvs>>();
        return Services;
    }
    public static IServiceCollection AddPartialViews<TPvs>(this IServiceCollection Services, IConfigurationSection Configuration)
        where TPvs : Enum
    {
        AddPartialViews<TPvs>(Services, Configuration.Value);
        return Services;
    }
}