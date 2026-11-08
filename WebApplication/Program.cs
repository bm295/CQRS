using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApplication.Application.AI;
using WebApplication.Infrastructure.AI;
using WebApplication.Application.Commands;
using WebApplication.Application.Queries;
using WebApplication.Infrastructure.Persistence;
using WebApplication.Infrastructure.ReadModels;

var builder = global::Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDirectory);
builder.Services.AddDbContext<AdvisorDb>(o => o.UseSqlite(builder.Configuration.GetConnectionString("Advisor") ?? $"Data Source={Path.Combine(dataDirectory, "advisor.db")}"));
builder.Services.AddScoped<IInfrastructureChangeRequestRepository, SqliteChangeRepository>();
builder.Services.AddScoped<ContextBuilder>();
var advisorEndpoint = builder.Configuration["Advisor:Endpoint"];
if (string.IsNullOrWhiteSpace(advisorEndpoint)) builder.Services.AddScoped<IChangeAdvisor, RuleChangeAdvisor>();
else
{
    var uri = new Uri(advisorEndpoint);
    if (uri.Scheme != "https") throw new InvalidOperationException("Advisor endpoint must use HTTPS.");
    void ConfigureAdvisorClient(HttpClient client)
    {
        client.BaseAddress = uri; client.Timeout = TimeSpan.FromSeconds(30);
        var key = builder.Configuration["Advisor:ApiKey"];
        if (!string.IsNullOrWhiteSpace(key)) client.DefaultRequestHeaders.Authorization = new("Bearer", key);
    }
    if (builder.Configuration["Advisor:Protocol"] == "chat-completions")
        builder.Services.AddHttpClient<IChangeAdvisor, ChatCompletionAdvisor>(ConfigureAdvisorClient);
    else builder.Services.AddHttpClient<IChangeAdvisor, HttpChangeAdvisor>(ConfigureAdvisorClient);
}
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
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AdvisorDb>();
    db.Database.EnsureCreated();
    var projections = scope.ServiceProvider.GetRequiredService<ProjectionUpdater>();
    foreach (var change in db.Changes.AsNoTracking()) await projections.UpdateAsync(change);
}

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
