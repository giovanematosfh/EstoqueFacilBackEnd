using System.Text;
using EstoqueFacil.API.Filters;
using EstoqueFacil.API.Middlewares;
using EstoqueFacil.API.Tenancy;
using EstoqueFacil.Application.Constants;
using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Application.Services;
using EstoqueFacil.Infrastructure.Data;
using EstoqueFacil.Infrastructure.Identity;
using EstoqueFacil.Infrastructure.Repositories;
using EstoqueFacil.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = false;
        options.User.RequireUniqueEmail = true;
        // TODO: reativar quando o SMTP estiver configurado corretamente.
        // options.SignIn.RequireConfirmedAccount = true;
    })
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpTenantProvider>();
builder.Services.AddScoped<IPlanLimitService, PlanLimitService>();

// Arquivos enviados (fotos de produtos). Em produção, aponte Storage:RootPath para fora da pasta publicada.
var storageRoot = Path.GetFullPath(builder.Configuration["Storage:RootPath"] ?? Path.Combine(builder.Environment.ContentRootPath, "storage"));
Directory.CreateDirectory(Path.Combine(storageRoot, ProductService.ImageFolder));
builder.Services.AddSingleton<IFileStorage>(new LocalFileStorage(storageRoot));

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStockMovementService, StockMovementService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IProductStockService, ProductStockService>();
builder.Services.AddScoped<IPlanService, PlanService>();

builder.Services.AddValidatorsFromAssemblyContaining<CategoryService>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Adiciona os serviços necessários
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Informe apenas o token JWT (sem o prefixo 'Bearer ')."
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(); // disponível em /swagger
}

await PlatformBootstrap.RunAsync(app.Services, app.Configuration);

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("PermitirFrontend");

// Servido sob /api para passar pelo mesmo proxy do Nginx.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(storageRoot, ProductService.ImageFolder)),
    RequestPath = "/api/uploads/" + ProductService.ImageFolder
});

app.UseAuthentication();

app.UseMiddleware<CompanyStatusMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
