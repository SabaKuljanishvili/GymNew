using GymMembershipManagement.API.Middleware;
using GymMembershipManagement.DAL.Repositories;
using GymMembershipManagement.DATA;
using GymMembershipManagement.SERVICE;
using GymMembershipManagement.SERVICE.Interfaces;
using GymMembershipManagement.SERVICE.Mapping;
using GymMembershipManagement.SERVICE.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace GymMembershipManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. CORS კონფიგურაცია
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()   // ნებას რთავს ნებისმიერ საიტს
                          .AllowAnyMethod()   // ნებას რთავს POST, PUT, DELETE, GET
                          .AllowAnyHeader();  // ნებას რთავს ნებისმიერ Header-ს (Content-Type და ა.შ.)
                });
            });

            // JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidateAudience = true,
                        ValidAudience = jwtSettings["Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // DB connection
            builder.Services.AddDbContext<GymDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("GymDbConnection"));
            });

            builder.Services.AddScoped<DbContext, GymDbContext>();

            // AutoMapper
            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>(), typeof(MappingProfile).Assembly);

            // Controllers
            builder.Services.AddControllers();

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Gym Membership API",
                    Version = "v1"
                });

                // Add JWT Authentication to Swagger
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme.\r\n\r\nEnter your token in the text input below.\r\n\r\nExample: \"eyJhbGciOiJIUzI1NiIs...\""
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });

            // Repositories
            builder.Services.AddScoped<IGymClassRepository, GymClassRepository>();
            builder.Services.AddScoped<IMembershipRepository, MembershipRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IPersonRepository, PersonRepository>();
            builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();

            // Services
            builder.Services.AddScoped<IGymClassService, GymClassService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IMembershipService, MembershipService>();
            builder.Services.AddScoped<ITrainerService, TrainerService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<GymMembershipManagement.API.Services.IRoleIntegrityService, GymMembershipManagement.API.Services.RoleIntegrityService>();

            var app = builder.Build();

            // 2. CORS Middleware-ის გააქტიურება (მნიშვნელოვანია იყოს Routing-სა და Authorization-ს შორის)
            app.UseCors("AllowAll");

            // Authentication and Authorization middleware
            app.UseAuthentication();
            app.UseAuthorization();

            // ავტომატური მიგრაციის ბლოკი
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GymDbContext>();
                try
                {
                    dbContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Migration error: {ex.Message}");
                }
            }

            // Global exception handling
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gym API V1");
                c.RoutePrefix = "swagger";
            });

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.MapGet("/", context =>
            {
                context.Response.Redirect("/swagger/index.html");
                return Task.CompletedTask;
            });

            app.Run();
        }
    }
}