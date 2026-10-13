using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopWeb.Data;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using ShopWeb.Options;
using ShopWeb.Filters;
using ShopWeb.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ShopApiExceptionFilter>();
});

builder.Services
    .AddOptions<ShopApiOptions>()
    .Bind(builder.Configuration.GetSection(ShopApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<IShopApiClient, ShopApiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<ShopApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
})
.AddStandardResilienceHandler(options =>
{
    // Don't retry POST/PUT/DELETE etc. - a retried POST could create duplicate orders
    options.Retry.DisableForUnsafeHttpMethods();
});

var app = builder.Build();

// Format prices as euro regardless of the machine's regional settings
app.UseRequestLocalization("en-IE");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
