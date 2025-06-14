using marketplace_api.Data;
using marketplace_api.Mapping;
using marketplace_api.Repository.UserRepository;
using marketplace_api.Services.AuthService;
using marketplace_api.Services.UserService;
using Microsoft.EntityFrameworkCore;
using Serilog;
using marketplace_api.Extenions;
using FluentValidation;
using marketplace_api.CustomFilter;
using marketplace_api.Services.RedisService;
using marketplace_api.MappingProfiles;
using marketplace_api.Repository.ProductViewHistoryRepository;
using marketplace_api.Services.ProductViewHistoryService;
using marketplace_api.Repository.CartRepository;
using marketplace_api.Services.CartService;
using Hangfire.PostgreSql;
using Hangfire;
using marketplace_api;
using marketplace_api.Services.CartManegementService;
using marketplace_api.Repository.Rewiew;
using marketplace_api.Services.ReviewService;
using Org.BouncyCastle.Asn1.Cms.Ecc;
using marketplace_api.Repository.OrderRepository;
using marketplace_api.Services.OrderService;
using marketplace_api.ModelsDto;
using Microsoft.AspNetCore.Http.HttpResults;
using marketplace_api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<CustomRoleActionFilter>();
    options.Filters.Add<CustomSessiontokenResousreFilter>();
});
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection(nameof(AuthSettings)));


builder.Services.AddCors(options => options
    .AddPolicy("CorsPolicy", builder => 
    {
        builder.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000", "http://localhost")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    }));

builder.Services.AddAutoMapper(
      typeof(UserProfiles)
    , typeof(ProductProfiles)
    , typeof(CartProductProfiles)
    , typeof(ReviewProfiles)
    , typeof(OrderProfile)
    , typeof(OrderProductProfile)
    , typeof(ProductViewHistoryProfile));


builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration) 
    .ReadFrom.Services(services) 
    .Enrich.FromLogContext()
    .WriteTo.Console() 
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

var connestString = builder.Configuration.GetConnectionString("DataBase");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "my-redis-container";
    options.InstanceName = "docker_network";
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connestString);
});

builder.Services.AddHangfire(h =>
    h.UsePostgreSqlStorage(connestString));
builder.Services.AddHangfireServer();

builder.Services.AddScoped<IUserRepository,UserRepository>();
builder.Services.AddScoped<IUserService,UserService>();
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddScoped<IProductViewHistoryService,ProductViewHistoryService>();

builder.Services.AddScoped<IValidator<UserDto>,UserDtoValidator>();
builder.Services.AddScoped<IValidator<ProductDtoResponse>,ProductDtoValidator>();
builder.Services.AddScoped<IValidator<ReviewRequestDto>,ReviewRequestDtoValidator>();
builder.Services.AddScoped<IValidator<ReviewResponseDto>,ReviewResponseDtoValidator>();
builder.Services.AddScoped<IValidator<AdminDto>,AdminDtoValidate>();
builder.Services.AddScoped<IValidator<ProductDto>, ProductDtoValid>();

builder.Services.AddScoped<IRedisService, RedisService>();
builder.Services.AddScoped<IProductViewHistoryRepository,ProductViewHistoryRepository>();
builder.Services.AddProd();
builder.Services.AddScoped<ICartRepository,CartRepository>();   
builder.Services.AddScoped<ICartService,CartService>();
builder.Services.AddScoped<marketplace_api.Services.MailService>();
builder.Services.AddScoped<ICartManagementService, CartManagementService>();
builder.Services.AddScoped<IReviewRepository,ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();

builder.Services.AddScoped<IValidator<UserLoginDto>, UserLoginDtoValidate>();
builder.Services.AddScoped<CustomRequestValidateReviewFilter>();
builder.Services.AddScoped<CustomResponseValidateReviewFilter>();
builder.Services.AddScoped<CustomAdminValidteFilter>();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.Migrate();
        Console.WriteLine("�������� ������� ���������.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"������ ��� ���������� ��������: {ex.Message}");
        throw; 
    }
}

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/dash", new DashboardOptions
{
    Authorization = new[] { new AllowAllUsersAuthorizationFilter() }
});

app.UseSwagger();
app.UseSwaggerUI(config =>
{
    config.RoutePrefix = string.Empty;
    config.SwaggerEndpoint("swagger/v1/swagger.json", "market API"); 
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


