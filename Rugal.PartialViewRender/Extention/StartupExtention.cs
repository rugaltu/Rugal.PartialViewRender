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
            var ViewPath = Configuration[$"PvReader:{ViewName}"];

            Services.AddHttpContextAccessor();
            Services.AddSingleton(Provider =>
            {
                var Reader = new PvRender<TViewEnum>(ViewPath, Provider);
                return Reader;
            });
            return Services;
        }
    }
}
