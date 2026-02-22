using Microsoft.Extensions.DependencyInjection;

namespace OFMS_API.Helper.Register
{
    public static class Register
    {
        public static IServiceCollection CommonRegister(this IServiceCollection services)
        {
            //dal class
            //services.AddScoped<IuserBL, UserBL>();
            //services.AddScoped<IMenuCategoryDAL, menuCategoryDAL>();
            //services.AddScoped<IOrderDAL, OrderDAL>();

            ////bl class
            //services.AddScoped<IuserBL, UserBL>();
            //services.AddScoped<IMenuCategoryBL, MenuCategoryBL>();
            //services.AddScoped<IOrderBL, OrderBL>();


            return services;
        }
    }
}
