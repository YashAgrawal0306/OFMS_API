using OFMS_API.BL.Imple;
using OFMS_API.BL.Interface;
using OFMS_API.DAL.Imple;
using OFMS_API.DAL.Interface;

namespace OFMS_API.Helper.Register
{
    public static class Register
    {
        public static IServiceCollection CommonRegister(this IServiceCollection services)
        {
            //dal class
            services.AddScoped<IuserDAL, userDAL>();
            services.AddScoped<IMenuCategoryDAL, menuCategoryDAL>();
            services.AddScoped<IOrderDAL, OrderDAL>();

            //bl class
            services.AddScoped<IuserBL, UserBL>();
            services.AddScoped<IMenuCategoryBL, MenuCategoryBL>();
            services.AddScoped<IOrderBL, OrderBL>();


            return services;
        }
    }
}
