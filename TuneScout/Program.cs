using DataAccess.Contexts;
using DataAccess.Repositories;
using Logic.Interfaces;
using Logic.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddSession();  
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<ISpotifyRepository, SpotifyRepository>();

builder.Services.AddDbContext<TuneScoutContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<ISongRepository, SongRepository>();
builder.Services.AddScoped<SongService>();

builder.Services.AddScoped<ITimelineRepository, TimelineRepository>();
builder.Services.AddScoped<TimelineService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/exeption");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();       
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); 

app.MapRazorPages(); 

app.Run();
