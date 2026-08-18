using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel((hostingContext, serverOptions) =>
{
    serverOptions.Listen(IPAddress.Any, int.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORT") ?? "5100"));

    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT")))
        serverOptions.Listen(IPAddress.Any, int.Parse(Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT")!), listenOptions
                => listenOptions.UseHttps("./aspnetapp.pfx", "V@%O!%T#C@2020"));
});
builder.Services.AddDistributedMemoryCache();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddCors(options => options.AddPolicy("AllowOrigin", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();
app.UseCors("AllowOrigin");
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
