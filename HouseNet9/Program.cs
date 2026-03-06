using HouseNet9.BackgroundJobs;
using HouseNet9.Data;
using HouseNet9.Helpers;
using HouseNet9.Services;
using HouseNet9.Services.Payments;
using Mail;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false) //true na serwer
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<FileUploadService>();
builder.Services.AddScoped<RentalCalculatorService>();
builder.Services.AddScoped<RentalCollisionService>();
builder.Services.AddScoped<IPaymentCalculator, PaymentCalculator>();
//renderowanie maila
builder.Services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
//praca w tle
builder.Services.AddHostedService<ReservationPaymentService>();

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

app.Run();
