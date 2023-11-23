using Dotnet8App.Api.Infrastructure;
using Dotnet8App.EFCore;
using Dotnet8App.EFCore.EFRepository;
using Dotnet8App.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var MyAllOrigins = "allowAll";
builder.Services.AddCors(option =>
    option.AddPolicy(name: MyAllOrigins, policy =>
    {
        policy.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader();
    })
);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<IdentityUser>().AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer("name=DefaultConnection"));

builder.Services.Scan(scan =>
    scan.FromAssembliesOf(typeof(IdentityRepository), typeof(IdentityService), typeof(CaptchaService))
        .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Manager", StringComparison.OrdinalIgnoreCase)
                                               || t.Name.EndsWith("Service", StringComparison.OrdinalIgnoreCase)
                                               || t.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase)
                                               || t.Name.EndsWith("UnitOfWork", StringComparison.OrdinalIgnoreCase)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(1800);
});

ConfigurationManager configuration = builder.Configuration;
var jwtSignKey = builder.Configuration.GetValue<string>("JwtSettings:SignKey");

if (!string.IsNullOrEmpty(jwtSignKey))
{
    builder.Services.AddScoped<JwtManager>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
            options.IncludeErrorDetails = true; // 預設值為 true，有時會特別關閉

            options.TokenValidationParameters = new TokenValidationParameters
            {
                // 透過這項宣告，就可以從 "sub" 取值並設定給 User.Identity.Name
                NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                // 透過這項宣告，就可以從 "roles" 取值，並可讓 [Authorize] 判斷角色
                RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

                // 一般我們都會驗證 Issuer
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),

                // 通常不太需要驗證 Audience
                ValidateAudience = false,
                //ValidAudience = "JwtAuthDemo", // 不驗證就不需要填寫

                // 一般我們都會驗證 Token 的有效期間
                ValidateLifetime = true,

                // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
                ValidateIssuerSigningKey = false,

                // "1234567890123456" 應該從 IConfiguration 取得
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSignKey))
            };
        });
}

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseDefaultFiles();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.MapIdentityApiEndpointRoute(app.Environment.IsDevelopment());

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
