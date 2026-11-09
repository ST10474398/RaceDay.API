using RaceDay.MVC.Services; // Make sure this matches your MVC project's namespace

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1. Add Session Support (Required for Auth)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 2. Register HttpClient to talk to the Part 2 API
builder.Services.AddHttpClient("RaceDayAPI", client =>
{
    // IMPORTANT: Change the port number below to match your API's port.
    // Look in your RaceDay.API -> Properties -> launchSettings.json to find the https port.
    client.BaseAddress = new Uri("https://localhost:7177/");
});

// 3. Register our custom ApiService
builder.Services.AddScoped<ApiService>();

// (Optional but recommended) Add HttpContextAccessor for reading cookies
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 4. IMPORTANT: UseSession MUST be before UseAuthorization
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();