using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rugal.PartialViewRender.Model;
using Rugal.PartialViewRender.Service;

namespace Rugal.PartialViewRender.Extention
{
    public static class StartupExtention
    {
        public static IServiceCollection AddPvRender<TViewEnum>(this IServiceCollection Services, IConfiguration Configuration)
            where TViewEnum : Enum
        {
            var ViewName = typeof(TViewEnum).Name;
            var ViewPath = Configuration[$"PvRender:{ViewName}"];

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
}