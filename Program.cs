using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using vulnerablewebpage.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (!app.Environment.IsEnvironment("Lab"))
{
    throw new InvalidOperationException(
        "This intentionally vulnerable application runs only with ASPNETCORE_ENVIRONMENT=Lab. " +
        "Do not deploy it to shared, public, or production infrastructure.");
}

// A new SQL Server container may need time to initialize.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var retryEnabled = string.Equals(
        Environment.GetEnvironmentVariable("LAB_DB_RETRY"), "true",
        StringComparison.OrdinalIgnoreCase);
    var maxAttempts = retryEnabled ? 40 : 1;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await db.Database.EnsureCreatedAsync();
            break;
        }
        catch (SqlException) when (attempt < maxAttempts)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}

var localLab = app.Environment.IsEnvironment("Lab");
if (!app.Environment.IsDevelopment() && !localLab)
{
    app.UseExceptionHandler("/XssLab/Index");
    app.UseHsts();
}

if (!localLab)
    app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? string.Empty;
    var isNewExercise = path.Equals("/labs", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/labs/", StringComparison.OrdinalIgnoreCase);
    // New unsafe examples are available only in an explicitly enabled lab.
    if (isNewExercise && !localLab)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    var isLabPage = path == "/" || isNewExercise ||
        path.StartsWith("/XssLab", StringComparison.OrdinalIgnoreCase);
    if (isLabPage)
    {
        context.Response.Headers["Cache-Control"] = "no-store";
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        if (HttpMethods.IsGet(context.Request.Method) &&
            (isNewExercise ||
             string.Equals(context.Request.Query["csp"].ToString(), "strict",
                 StringComparison.OrdinalIgnoreCase)))
        {
            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; script-src 'self'; style-src 'self' https://cdn.jsdelivr.net; " +
                "img-src 'self' data:; font-src 'self' data:; connect-src 'self'; " +
                "object-src 'none'; base-uri 'none'; form-action 'self'; frame-ancestors 'none'";
        }
    }
    await next();
});
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=XssLab}/{action=Index}/{id?}");
app.Run();
