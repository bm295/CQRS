using MediatR;
using WebApplication.Application.Commands;
using WebApplication.Application.Queries;
using WebApplication.Infrastructure.Persistence;
using WebApplication.Infrastructure.ReadModels;

var builder = global::Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IInfrastructureChangeRequestRepository, InMemoryInfrastructureChangeRequestRepository>();
builder.Services.AddSingleton<IChangeReadModelStore, InMemoryChangeReadModelStore>();
builder.Services.AddSingleton<ProjectionUpdater>();
builder.Services.AddMediatR(typeof(CreateChangeRequestCommand).Assembly);

builder.Services.AddScoped<CreateChangeRequestCommandHandler>();
builder.Services.AddScoped<SubmitForApprovalCommandHandler>();
builder.Services.AddScoped<ApproveChangeRequestCommandHandler>();
builder.Services.AddScoped<RejectChangeRequestCommandHandler>();
builder.Services.AddScoped<ScheduleExecutionCommandHandler>();
builder.Services.AddScoped<MarkExecutionStartedCommandHandler>();
builder.Services.AddScoped<MarkExecutionCompletedCommandHandler>();
builder.Services.AddScoped<MarkExecutionFailedCommandHandler>();

builder.Services.AddScoped<GetChangeRequestByIdQueryHandler>();
builder.Services.AddScoped<ListPendingApprovalsQueryHandler>();
builder.Services.AddScoped<ListScheduledChangesQueryHandler>();
builder.Services.AddScoped<ListFailedChangesQueryHandler>();
builder.Services.AddScoped<GetOperationalChangeSummaryQueryHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/infra/dashboard");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=InfraPages}/{action=Dashboard}/{id?}");

app.Run();

public partial class Program
{
}
