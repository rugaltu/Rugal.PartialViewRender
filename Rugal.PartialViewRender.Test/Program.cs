using Rugal.PartialViewRender.Extensions;
using Rugal.PartialViewRender.Models;
using Rugal.PartialViewRender.Test.PartialView.Dtvl;
using Rugal.PartialViewRender.Test.PartialView.Dtvl.View;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddPartialViews<DtvlPv>(builder.Configuration.GetSection("Pvs:DtvlPv"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
