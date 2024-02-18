using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rugal.PartialViewRender.Service;

namespace Rugal.PartialViewRender.Extention
{
    public static class StartupExtention
    {
        public static IServiceCollection AddPvRender<TViewEnum>(this IServiceCollection Services, IConfiguration Configuration)
            where TViewEnum : Enum
        {
            var ViewName = typeof(TViewEnum).Name;
            var PvSection = Configuration.GetSection($"PvRender:{ViewName}");
            var ViewPath = PvSection["View"];
            var JsPath = PvSection["Js"];

            Services.AddHttpContextAccessor();
            Services.AddSingleton(Provider =>
            {
                var Reader = new PvRender<TViewEnum>(ViewPath, JsPath, Provider);
                return Reader;
            });
            return Services;
        }
    }
}
