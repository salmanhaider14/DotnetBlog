using Blog.Components;
using Blog.Components.Account;
using Blog.Data;
using Blog.Data.Models;
using Blog.Services;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Temporary debug code - remove after troubleshooting
Console.WriteLine("=== Configuration Values ===");
Console.WriteLine("DefaultConnection: " + builder.Configuration.GetConnectionString("DefaultConnection"));
Console.WriteLine("CloudinaryUrl: " + builder.Configuration["CloudinaryUrl"]);
Console.WriteLine("SendGridKey: " + builder.Configuration["AuthMessageSenderOptions:SendGridKey"]);
Console.WriteLine("SendGridDomain: " + builder.Configuration["AuthMessageSenderOptions:SendGridDomain"]);
Console.WriteLine("=== Environment Variables ===");
Console.WriteLine("DOTNET_ENVIRONMENT: " + Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
Console.WriteLine("ASPNETCORE_ENVIRONMENT: " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

// Add services to the container.
builder.Services.AddRazorComponents(options =>
    options.DetailedErrors = builder.Environment.IsDevelopment())
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();


builder.Services.AddSingleton<IEmailSender<ApplicationUser>, EmailSender>();
builder.Services.AddMudServices();
builder.Services.AddScoped<PostService>();
builder.Services.AddHttpClient();


builder.Services.AddTransient<ImageUploadService>();
string cloudinaryUrl = builder.Configuration["CloudinaryUrl"]
                       ?? throw new InvalidOperationException("Cloudinary URL not found.");
Cloudinary cloudinary = new Cloudinary(cloudinaryUrl);
cloudinary.Api.Secure = true;
builder.Services.AddSingleton(cloudinary);

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB in bytes
});

builder.Services.Configure<AuthMessageSenderOptions>(
    builder.Configuration.GetSection("AuthMessageSenderOptions"));
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(5);
    options.SlidingExpiration = true;
});
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
    options.TokenLifespan = TimeSpan.FromHours(3));

builder.Services.AddScoped<CommentsService>();
var app = builder.Build();

    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Test connection first
            bool canConnect = false;
            try
            {
                canConnect = dbContext.Database.CanConnect();
                Console.WriteLine("Database connection test: " + (canConnect ? "Success" : "Failed"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database connection error: " + ex.Message);
            }
            
            if (canConnect)
            {
                // Apply migrations
                dbContext.Database.Migrate();
                
                // Initialize roles
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                string[] roles = { "Admin", "Author", "Reader" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }
            else
            {
                Console.WriteLine("Skipping migrations as database connection failed");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error during startup: " + ex.ToString());
    }


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
