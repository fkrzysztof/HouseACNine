using HouseNet9.BackgroundJobs;
using HouseNet9.BackgroundService;
using HouseNet9.Data;
using HouseNet9.Helpers;
using HouseNet9.Services;
using HouseNet9.Services.Payments;
using Mail;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// tylko w trybie Development
if (builder.Environment.IsDevelopment())
{
    //builder.Configuration.AddUserSecrets<Program>();
    builder.Configuration.AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly());
}

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false) //true na serwer
//    .AddEntityFrameworkStores<ApplicationDbContext>();
//Role
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // <-- Razor Pages JEST KONIECZNE DO IDENTITY

builder.Services.AddScoped<FileUploadService>();
builder.Services.AddScoped<RentalCalculatorService>();
builder.Services.AddScoped<RentalCollisionService>();
builder.Services.AddScoped<IPaymentCalculator, PaymentCalculator>();
//renderowanie maila
builder.Services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
//praca w tle
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("ReservationPaymentJob");

    q.AddJob<ReservationPaymentJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("ReservationPaymentTrigger")
        .WithSimpleSchedule(x => x
            .WithIntervalInMinutes(1)//.WithIntervalInHours(6)
            .RepeatForever()));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

//wysylanie maili
builder.Services.AddScoped<IReservationNotificationService, ReservationNotificationService>();
builder.Services.AddScoped<ICommentNotificationService, CommentNotificationService>();



//emial
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();


//session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
//

var app = builder.Build();

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
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication(); //dodane
app.UseAuthorization();



app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string adminRole = "Admin";
    string adminEmail = "admin@example.com";
    string adminPassword = "Haslo123!";

    // 1️⃣ Tworzymy rolę, jeśli jeszcze nie istnieje
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    // 2️⃣ Sprawdzamy, czy użytkownik istnieje
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);

        // 3️⃣ Jeśli udało się utworzyć użytkownika, przypisujemy rolę
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }
    }
}
await app.RunAsync();
//app.Run();
