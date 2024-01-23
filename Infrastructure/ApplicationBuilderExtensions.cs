using Microsoft.EntityFrameworkCore;
using RandomShop.Data;

namespace RandomShop.Infrastructure
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder PrepareDatabase(this IApplicationBuilder app)
        {
            using var scopedServices = app.ApplicationServices.CreateAsyncScope();

            var data = scopedServices.ServiceProvider.GetService<ShopContext>();

            data.Database.Migrate();


            return app;
        }
    }
}
