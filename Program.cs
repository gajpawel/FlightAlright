using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FlightAlright.Data;
using FlightAlright.Middleware;
using FlightAlright.Models;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
Stripe.StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers(); // Dodaje kontrolery API
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Czas wygaśnięcia sesji
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<FlightAlrightContext>(options =>
    options.UseSqlite("Data Source=FlightAlright.db"));

builder.Services.AddHttpContextAccessor(); // Dodana nowa linia do obsługi IHttpContextAccessor

builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccessDenied";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthorization();

//dodawnanie permisji wg folderu w którym znajduje się strona
var routePermissions = new Dictionary<string, PermissionController.UserTypes[]>
{
    { "/Clients", [PermissionController.UserTypes.Client, PermissionController.UserTypes.Administrator, PermissionController.UserTypes.Employee] },
    {"/PaymentResults", [PermissionController.UserTypes.Employee, PermissionController.UserTypes.Administrator, PermissionController.UserTypes.Client] },
    { "/Employees", [PermissionController.UserTypes.Employee, PermissionController.UserTypes.Administrator] },
    { "/Admin", [PermissionController.UserTypes.Administrator] },
};

//rejestracja permisji wykorzystując middleware
foreach (var routePermission in routePermissions)
{
    app.UseWhen(context => context.Request.Path.StartsWithSegments(routePermission.Key), appBuilder =>
    {
        appBuilder.UsePermission(routePermission.Value);
    });
}

// Dodaj obsługę kontrolerów API
app.MapControllers(); // To umożliwia obsługę endpointów kontrolerów API
app.MapRazorPages();

app.Run();
