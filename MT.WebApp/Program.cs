using Microsoft.Extensions.Configuration;
using MT.AzureStorageLib.Services.Concrete;
using MT.AzureStorageLib.Services.Interfaces;
using MT.WebApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddRazorPages();
ConnectionString.AzureStorageConnectionString = builder.Configuration.GetSection("AzureConnectionStrings")["StorageCloudConnStr"];
builder.Services.AddScoped( typeof(TableStorage<>));
builder.Services.AddSingleton<IBlobStorage, BlobStorage>();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

//app.MapRazorPages();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<NotificationHub>("/NotificationHub");
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
