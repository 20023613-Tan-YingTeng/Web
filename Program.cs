global using xShapes.Models;
global using xShapes.Services;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authentication.Cookies;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
global using Microsoft.AspNetCore.Mvc.Rendering;
global using Microsoft.EntityFrameworkCore;
global using RP.SOI.DotNet.Services;
global using System.ComponentModel.DataAnnotations;
global using System.Security.Claims;
global using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
RotativaConfiguration.Setup(builder.Environment.WebRootPath, "Rotativa");



builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)

                .AddCookie(options =>

                {

                    options.LoginPath = "/Account/Login/";

                    options.AccessDeniedPath = "/Account/Forbidden/";

                });

builder.Services.AddScoped<IDbService, DbService>();

builder.Services.AddDbContext<AppDbContext>(

   options => options.UseSqlServer(

       builder.Configuration.GetConnectionString("DefaultConnection")));





var app = builder.Build();

if (app.Environment.IsDevelopment())

{

    app.UseDeveloperExceptionPage();

}

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();



app.MapControllerRoute(

    name: "default",

    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();