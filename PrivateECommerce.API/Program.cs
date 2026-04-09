using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PrivateECommerce.API.Configurations;
using PrivateECommerce.API.Data;
using PrivateECommerce.API.Models;
using PrivateECommerce.API.Seed;
using PrivateECommerce.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;



using System.Text;

var builder = WebApplication.CreateBuilder(args);



// ======================= DATABASE =======================

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

string connectionString;

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    Console.WriteLine("✅ Using Railway DATABASE_URL");

    if (databaseUrl.StartsWith("postgres://") || databaseUrl.StartsWith("postgresql://"))
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);

        connectionString =
            $"Host={uri.Host};" +
            $"Port={uri.Port};" +
            $"Database={uri.AbsolutePath.TrimStart('/')};" +
            $"Username={userInfo[0]};" +
            $"Password={userInfo[1]};" +
            $"SSL Mode=Require;" +
            $"Trust Server Certificate=true";
    }
    else
    {
        connectionString = databaseUrl;
    }
}
else
{
    Console.WriteLine("⚡ Using Railway PUBLIC fallback connection string");

    // fallback for local Visual Studio migrations
    connectionString =
        "Host=tramway.proxy.rlwy.net;" +
        "Port=44809;" +
        "Database=railway;" +
        "Username=postgres;" +
        "Password=XLmfzjyYepLprsGfeglueljVsQqUCTMG;" +
        "SSL Mode=Require;" +
        "Trust Server Certificate=true";
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});



// ======================= SERVICES =======================
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAdminReportService, AdminReportService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAdminSalesExecutiveService, AdminSalesExecutiveService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISalesProductService, SalesProductService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IAdminWarehouseUserService, AdminWarehouseUserService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();




var redisUrl = builder.Configuration["REDIS_URL"];

if (!string.IsNullOrEmpty(redisUrl))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisUrl;
        options.InstanceName = "PrivateECommerce:";
    });
}









// ======================= CLOUDINARY =======================
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddControllers();

// ======================= JWT =======================
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new Exception("❌ JWT Key missing in environment variables");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ======================= SWAGGER =======================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo { Title = "PrivateECommerce.API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ======================= CORS =======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(
    "https://safamedico.com",
    "https://www.safamedico.com",
    "https://your-vercel-project.vercel.app",
    "http://localhost:5173"
)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (ValidationException ex)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (NotFoundException ex)
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (Exception)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Something went wrong. Please contact admin."
        });
    }
});


// ======================= PIPELINE =======================
app.UseSwagger();
app.UseSwaggerUI();
// ======================= MIGRATION + SEED =======================
using (var scope = app.Services.CreateScope())
{
    try
    {
        Console.WriteLine("🔄 Running database migrations...");
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Database.Migrate(); // ensure tables exist
        DbSeeder.SeedAdmin(db); // create admin user

        Console.WriteLine("✅ Database ready");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Migration failed:");
        Console.WriteLine(ex.Message);
    }
}





app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseMiddleware<ExceptionMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();



app.Run();