using BaseProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Strategy.Models;
using WebApp.Strategy.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AppIdentityDbContext>()
  .AddDefaultTokenProviders();


#region strategy design Pattern add ioc.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IProductRepository>(sp =>
{

    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();

    var claim = httpContextAccessor.HttpContext.User.Claims.Where(x => x.Type == Settings.ClaimDatabaseType).FirstOrDefault();

    var context = sp.GetRequiredService<AppIdentityDbContext>();

    if (claim == null) return new ProductRepositoryFromSqlServer(context);

    var databaseType = (EDatabaseType)int.Parse(claim.Value);
    return databaseType switch
    {
        EDatabaseType.SqlServer => new ProductRepositoryFromSqlServer(context),
        EDatabaseType.MongoDb => new ProductRepositoryFromMongoDb(builder.Configuration),
        _ => throw new NotImplementedException()
    };

});
#endregion

var app = builder.Build();


#region seed data
using (var scope = app.Services.CreateAsyncScope())
{
    var identityDbContext = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    // Veritabanı migrasyonları yapılıyor. her ayağa kalkmada migraiton yapmamalı. test et.
    await identityDbContext.Database.MigrateAsync();

    // Kullanıcılar var mı kontrol et
    if (!await userManager.Users.AnyAsync())
    {
        var users = new List<AppUser>
            {
                new AppUser() { UserName = "admin", Email = "admin@gmail.com", Age=39 },
                new AppUser() { UserName = "user1", Email = "user1@gmail.com", Age=22 },
                new AppUser() { UserName = "user2", Email = "user2@gmail.com", Age=28 },
                new AppUser() { UserName = "user3", Email = "user3@gmail.com" },
                new AppUser() { UserName = "user4", Email = "user4@gmail.com" },
                new AppUser() { UserName = "user5", Email = "user5@gmail.com" }
            };

        foreach (var user in users)
        {
            var result = await userManager.CreateAsync(user, "Password12*");
            if (!result.Succeeded)
            {
                // Hata yönetimi: Kullanıcı oluşturulurken bir hata oluştu.
                throw new Exception($"Kullanıcı oluşturulurken hata: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

    }
}
#endregion


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
