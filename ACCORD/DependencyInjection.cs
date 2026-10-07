using ACCORD.Application.Common;
using ACCORD.Application.Sessions;
using ACCORD.Application.Tasks;
using ACCORD.Application.Tasks.Commands;
using ACCORD.Application.Tasks.Queries;
using ACCORD.Infrastructure.Supabase;

namespace ACCORD;

public static class DependencyInjection
{
    public static IServiceCollection AddAccordBackend(this IServiceCollection services)
    {
        services.AddHttpClient<SupabaseClient>(client => client.Timeout = TimeSpan.FromSeconds(20));
        services.AddScoped<ITaskReader, SupabaseTaskReader>();
        services.AddScoped<ITaskWriter, SupabaseTaskWriter>();
        services.AddScoped<ISessionGateway, SupabaseSessionGateway>();
        services.AddScoped<IQueryHandler<ListTasksQuery>, ListTasksQueryHandler>();
        services.AddScoped<ICommandHandler<CreateTaskCommand>, CreateTaskCommandHandler>();
        services.AddScoped<ICommandHandler<SetTaskCompletionCommand>, SetTaskCompletionCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteTaskCommand>, DeleteTaskCommandHandler>();
        services.AddScoped<ICommandHandler<CreateGuestSessionCommand>, CreateGuestSessionCommandHandler>();
        services.AddScoped<ICommandHandler<RefreshGuestSessionCommand>, RefreshGuestSessionCommandHandler>();
        return services;
    }
}
