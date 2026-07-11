using System.Net;


var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((hostingContext, serverOptions) =>
{
    serverOptions.Listen(IPAddress.Any, int.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORT") ?? "5210"));

    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT")))
        serverOptions.Listen(IPAddress.Any, int.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT")!), listenOptions
                => listenOptions.UseHttps("./aspnetapp.pfx", "V@%O!%T#C@2020"));
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
        ctx.Context.Response.Headers.Append("Pragma", "no-cache");
        ctx.Context.Response.Headers.Append("Expires", "0");
    }
});


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
