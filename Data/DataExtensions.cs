using Microsoft.Extensions.DependencyInjection;

public static class DataExtensions
{
    public static IServiceCollection RegisterDataServices(
        this IServiceCollection services)
    {
        services.AddTransient<IChartRepository, ChartRepository>();
        services.AddTransient<IProductRepository, ProductRepository>();
        services.AddTransient<IContactRepository, ContactRepository>();
        services.AddTransient<ITradeRepository, TradeRepository>();
        services.AddTransient<IAccountItemRepository, AccountItemRepository>();
        services.AddTransient<IAccountRepository, AccountRepository>();
        services.AddTransient<IOrderRepository, OrderRepository>();
        services.AddTransient<ICategoryRepository, CategoryRepository>();
        services.AddTransient<IDebtRepository, DebtRepository>();
        services.AddTransient<IStaffRepository, StaffRepository>();
        services.AddTransient<IReceivedNoteRepository, ReceivedNoteRepository>();
        services.AddTransient<ITicketRepository, TicketRepository>();
        return services;
    }
}
