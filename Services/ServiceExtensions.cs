using Microsoft.Extensions.DependencyInjection;

public static class ServiceExtensions
{
    public static IServiceCollection RegisterServices(
        this IServiceCollection services)
    {
        services.AddSingleton<ICacheService, CacheService>();
        services.AddTransient<IProductService, ProductService>();
        services.AddTransient<ITradeService, TradeService>();
        services.AddTransient<IAccountService, AccountService>();
        services.AddTransient<IOrderService, OrderService>();
        services.AddTransient<ICategoryService, CategoryService>();
        services.AddTransient<IDebtService, DebtService>();
        services.AddTransient<IStaffService, StaffService>();
        services.AddTransient<IReceivedNoteService, ReceivedNoteService>();
        services.AddTransient<IExcelService, ExcelService>();
        services.AddTransient<ITicketService, TicketService>();
        services.AddTransient<ISqlService, SqlService>();
        services.AddTransient<IImageService, ImageService>();
        services.AddTransient<IActivityService, ActivityService>();
        services.AddTransient<IChatGPTService, ChatGPTService>();
        return services;
    }
}
