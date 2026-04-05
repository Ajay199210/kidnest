using KidNest.Core.Entities;
using KidNest.Core.Interfaces;
using KidNest.Infrastructure.Repositories;
using KidNest.Services.Interfaces;
using KidNest.Services.Services;
using KidNest.Web.Hubs;
using Microsoft.AspNetCore.Identity;
using NToastNotify;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

DbConnectionFactory.Initialize(builder.Configuration);

builder.Services.AddScoped<IUsersRespository, UsersRepository>();
builder.Services.AddScoped<IUsersService, UsersService>();

builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();
builder.Services.AddScoped<ICategoriesService, CategoriesService>();

builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
builder.Services.AddScoped<IProductsService, ProductsService>();

builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();
builder.Services.AddScoped<IOrdersService, OrdersService>();

builder.Services.AddScoped<IContentsRepository, ContentsRepository>();
builder.Services.AddScoped<IContentsService, ContentsService>();

builder.Services.AddScoped<ICartService, CartService>();

builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.AddScoped<ISettingsService, SettingsService>();

builder.Services.AddScoped<IMdColorsRepository, MdColorsRepository>();
builder.Services.AddScoped<IMdColorsService, MdColorsService>();

builder.Services.AddScoped<IMdSizesRepository, MdSizesRepository>();
builder.Services.AddScoped<IMdSizesService, MdSizesService>();

builder.Services.AddScoped<IFileStorageService, FileStorageService>();

builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // default: 20 mins
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // required for GDPR compliance
    options.Cookie.MaxAge = TimeSpan.FromDays(10); // remember me (persistent session cookie)
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // only send cookies on HTTPS
});

builder.Services.AddSignalR();

builder.Services.AddHttpContextAccessor();

builder.Services.AddMvc().AddNToastNotifyToastr(new ToastrOptions()
{
    CloseButton = true,
    PositionClass = ToastPositions.BottomRight,
    ProgressBar = true,
    PreventDuplicates = true,
    //ShowMethod = "fadeIn",
    NewestOnTop = true,
    TapToDismiss = true,
    TimeOut = 10000 // 10s
});

// Add authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "UserScheme";
})
    .AddCookie("UserScheme", options =>
    {
        options.Cookie.Name = "KidNestAuthCookie";
        options.LoginPath = "/Account/Login";  // Login path for users
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS ONLY
        options.Cookie.SameSite = SameSiteMode.Strict;  // Optional, enhances CSRF protection
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(1);  // Set cookie expiration
    })
    .AddCookie("AdminScheme", options =>
    {
        options.Cookie.Name = "AdminAuthCookie";
        //options.Cookie.Path = "/";  // Matches area path
        options.LoginPath = "/Admin/Home/Login";  // Full path to login
        options.AccessDeniedPath = "/Admin/Home/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(25);

        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.Redirect("/Admin/Home/Login?returnUrl=" + Uri.EscapeDataString(ctx.Request.Path));
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy =>
    {
        policy.AuthenticationSchemes.Add("AdminScheme");
        policy.RequireRole("Admin");
        policy.RequireAuthenticatedUser();
    })
    .AddPolicy("UserOnly", policy =>
    {
        policy.AuthenticationSchemes.Add("UserScheme");
        policy.RequireRole("User");
    });

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password Settings
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    //options.Password.RequiredUniqueChars = 1;
});

// Bundling and minification with WebOptimizer (production only via UseWebOptimizer placement)
builder.Services.AddWebOptimizer(assetPipeline =>
{
    assetPipeline.MinifyCssFiles("css/styles.css", "css/admin.css");
    assetPipeline.MinifyJsFiles("js/script.js", "js/login.js", "js/admin.js");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/Generic");
    app.UseHsts();

    app.UseWebOptimizer();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseNToastNotify();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// General admin route for other admin pages
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

// Default route for normal website
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Map the SignalR Hub to a route
app.MapHub<StoreHub>("/storeHub");

app.Run();
