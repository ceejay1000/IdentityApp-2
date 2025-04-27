using System.Security.Claims;
using Api;
using Api.Services;
using API.Data;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()
));

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ContextSeedService>();


builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireUppercase = false;

    options.SignIn.RequireConfirmedEmail = true;
})
    .AddRoles<IdentityRole>()
    .AddRoleManager<RoleManager<IdentityRole>>()
    .AddEntityFrameworkStores<Context>()
    .AddSignInManager<SignInManager<User>>()
    .AddUserManager<UserManager<User>>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                               System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidateIssuer = true,
            ValidateAudience = false,
        };
    });

builder.Services.AddCors();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.ModelState
        .Where(x => x.Value.Errors.Count > 0)
        .SelectMany(x => x.Value.Errors)
        .Select(x => x.ErrorMessage).ToArray();

        var toReturn = new
        {
            Errors = errors
        };

        return new BadRequestObjectResult(toReturn);
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(SD.Admin));
    options.AddPolicy("RequireManagerRole", policy => policy.RequireRole(SD.Manager));
    options.AddPolicy("RequirePlayerRole", policy => policy.RequireRole(SD.Player));
    // options.AddPolicy("RequireAdminOrManagerRole", policy => policy.RequireRole(SD.Admin, SD.Manager));
    options.AddPolicy("RequireAdminOrManagerPolicy", policy => policy.RequireRole(SD.Admin, SD.Manager));
    options.AddPolicy("RequirePlayerOrManagerPolicy", policy => policy.RequireRole(SD.Player, SD.Manager));
    options.AddPolicy("RequireAdminAndManagerPolicy", policy => policy.RequireRole(SD.Admin).RequireRole(SD.Manager));

    options.AddPolicy("AdminEmailPolicy", policy =>
    {
        policy.RequireClaim(ClaimTypes.Email, "admin@mail.com");
    });

    options.AddPolicy("managerNamePolicy", policy =>
    {
        policy.RequireClaim(ClaimTypes.Name, "manager");
    });

    options.AddPolicy("VIPPolicy", policy =>
    {
        policy.RequireAssertion(context => SD.VIPPolicy(context));
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors(apt =>
{
    apt.AllowAnyOrigin()
       .AllowAnyMethod()
       .AllowAnyHeader()
       .AllowCredentials()
       .WithOrigins("http://localhost:4200");
});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

#region 
using (var scope = app.Services.CreateScope())
{
    var contextSeedService = scope.ServiceProvider.GetRequiredService<ContextSeedService>();
    await contextSeedService.InitializeContextAsync();
}
#endregion

app.Run();
