using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReserveOne.Application;
using ReserveOne.Application.Auth;
using ReserveOne.Application.Reservations;
using ReserveOne.Application.Restaurants;
using ReserveOne.Application.Security;
using ReserveOne.Application.Tenants;
using ReserveOne.Application.Users;
using ReserveOne.Domain.Entities;
using ReserveOne.Infrastructure;
using ReserveOne.Infrastructure.Auditing;
using ReserveOne.Infrastructure.Repositories;
using ReserveOne.Infrastructure.Seeding;
using ReserveOne.Infrastructure.Security;
using ReserveOne.Infrastructure.Services;
using System.Text;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "reserveone-super-secret-key-please-change-32-bytes-minimum-123456"; // 64+ chars

var connectionString = builder.Configuration.GetConnectionString("ReserveOne")
    ?? "Server=localhost;Database=ReserveOneDb;Trusted_Connection=True;TrustServerCertificate=True";

// DbContext
builder.Services.AddDbContext<ReserveOneDbContext>(options =>
{
    options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(ReserveOneDbContext).Assembly.FullName));
});

// Audit interceptor (stub registration)
builder.Services.AddScoped<AuditInterceptor>();

// Repositories
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IRestaurantConfigRepository, RestaurantConfigRepository>();
builder.Services.AddScoped<IReadOnlyReservationRepository, ReadOnlyReservationRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<ISalonRepository, SalonRepository>();
builder.Services.AddScoped<IServicePeriodRepository, ServicePeriodRepository>();

// Services
builder.Services.AddScoped<ICapacityCalculator, CapacityCalculator>();
builder.Services.AddScoped<AvailabilityService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RestaurantConfigVersioningService>();

// Application handlers
builder.Services.AddScoped<CreateReservationHandler>();
builder.Services.AddScoped<CreateTenantHandler>();
builder.Services.AddScoped<CreateRestaurantHandler>();
builder.Services.AddScoped<CreateSalonHandler>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
        };
    });

// Policies for roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("Owner", policy => policy.RequireRole("Owner"));
    options.AddPolicy("Manager", policy => policy.RequireRole("Manager"));
    options.AddPolicy("Operator", policy => policy.RequireRole("Operator"));
});

// Tenant middleware and context
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TenantContext>(sp =>
{
    var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    Guid? tenantId = null;
    var userId = GetUserIdFromToken(httpContext);

    if (httpContext?.Items.TryGetValue("apiKeyTenantId", out var tid) == true && tid is Guid g)
    {
        tenantId = g;
    }
    else
    {
        tenantId = GetTenantIdFromToken(httpContext);
    }

    return new TenantContext(tenantId, userId);
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ReserveOne API", Version = "v1" });

    // Define Bearer auth scheme
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer 12345abcdef'",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer 12345abcdef'",
    });

    // Require Bearer auth globally so Swagger UI attaches the header
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            securityScheme,
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed SuperAdmin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReserveOneDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await SuperAdminSeeder.SeedAsync(db, hasher);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    // TenantMiddleware stub: ensure TenantId is present and not from frontend
    // Extracted from token in services above
    await next();
});

// API Key middleware for Reservations (no JWT required)
app.Use(async (context, next) =>
{
    // Only protect reservation and availability endpoints with API key logic
    var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
    var requiresKey = path.StartsWith("/reservations") || path.StartsWith("/availability");

    if (!requiresKey)
    {
        await next();
        return;
    }

    // Accept API key via header X-Api-Key
    if (!context.Request.Headers.TryGetValue("X-Api-Key", out var key) || string.IsNullOrWhiteSpace(key))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Missing X-Api-Key");
        return;
    }

    // Resolve tenant by API key
    var tenantRepo = context.RequestServices.GetRequiredService<ITenantRepository>();
    var tenant = await tenantRepo.GetByApiKeyAsync(key!, context.RequestAborted);
    if (tenant is null)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Invalid API key");
        return;
    }

    // Stash tenant in HttpContext for TenantContext factory to pick it up
    // We add a temporary header for extraction
    context.Items["apiKeyTenantId"] = tenant.Id;
    await next();
});

// Auth endpoints
app.MapPost("/auth/login", async (LoginRequest req, AuthService auth, IUserRepository users) =>
{
    var res = await auth.LoginAsync(req);
    var user = await users.GetByEmailAsync(req.Email) ?? throw new UnauthorizedAccessException();

    // Create JWT with both standard and custom claims
    var claims = new List<System.Security.Claims.Claim>
    {
        new(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(System.Security.Claims.ClaimTypes.Role, user.Role.ToString()),
        new("sub", user.Id.ToString()),
        new("role", user.Role.ToString()),
    };
    if (user.TenantId.HasValue)
    {
        claims.Add(new("tenantId", user.TenantId.Value.ToString()));
    }

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
        issuer: null,
        audience: null,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: creds);
    var tokenString = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);

    res.Token = tokenString;
    return Results.Ok(res);
}).WithTags("Auth");

// Agent token (for server-to-server integrations)
app.MapPost("/auth/agent-token", (HttpRequest httpReq) =>
{
    var cfg = builder.Configuration;
    var provided = httpReq.Headers["X-Agent-Key"].FirstOrDefault();
    var expected = cfg["Agent:ApiKey"];
    if (string.IsNullOrWhiteSpace(expected) || provided != expected)
    {
        return Results.Unauthorized();
    }

    var tenantIdStr = cfg["Agent:TenantId"];
    var userIdStr = cfg["Agent:UserId"] ?? Guid.Empty.ToString();
    if (!Guid.TryParse(tenantIdStr, out var tenantId))
    {
        return Results.Problem("Agent:TenantId no configurado correctamente");
    }
    Guid.TryParse(userIdStr, out var userId);

    var claims = new List<System.Security.Claims.Claim>
    {
        new(System.Security.Claims.ClaimTypes.NameIdentifier, userId == Guid.Empty ? "agent" : userId.ToString()),
        new(System.Security.Claims.ClaimTypes.Role, "Agent"),
        new("sub", userId == Guid.Empty ? "agent" : userId.ToString()),
        new("role", "Agent"),
        new("tenantId", tenantId.ToString()),
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
        issuer: null,
        audience: null,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: creds);
    var tokenString = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new { token = tokenString });
}).WithTags("Auth");

app.MapPost("/auth/change-password", async (ChangePasswordRequest req, IUserRepository users, AuthService auth, TenantContext tenant) =>
{
    var user = await users.GetByIdAsync(tenant.UserId) ?? throw new UnauthorizedAccessException();
    await auth.ChangePasswordAsync(user, req);
    return Results.NoContent();
}).RequireAuthorization().WithTags("Auth");

// POST /tenants (no auth as requested temporarily)
app.MapPost("/tenants", async (CreateTenantCommand cmd, CreateTenantHandler handler) =>
{
    var result = await handler.HandleAsync(cmd);
    return Results.Created($"/tenants/{result.TenantId}", result);
}).WithTags("Tenants");

// Reservations and Availability endpoints

// POST /reservations
app.MapPost("/reservations", async (CreateReservationCommand command, CreateReservationHandler handler) =>
{
    var id = await handler.HandleAsync(command);
    return Results.Created($"/reservations/{id}", new { id });
}).WithTags("Reservations");

// GET /reservations
app.MapGet("/reservations", async (Guid? restaurantId, DateTime? date, IReservationRepository repo, TenantContext tenant) =>
{
    var tenantId = tenant.TenantId ?? Guid.Empty;
    var items = await repo.GetByFilterAsync(tenantId, restaurantId, date);
    return Results.Ok(items);
}).WithTags("Reservations");

// GET /availability
app.MapGet("/availability", async (Guid restaurantId, DateTime date, TimeSpan time, int partySize, AvailabilityService availability, TenantContext tenant, CancellationToken ct) =>
{
    var cmd = new CreateReservationCommand
    {
        RestaurantId = restaurantId,
        SalonId = null,
        CustomerName = "",
        Phone = "",
        Fecha = date.Date,
        Hora = time,
        PartySize = partySize,
        Source = ReserveOne.Domain.Enums.ReservationSource.WebForm,
    };

    try
    {
        await availability.CheckAvailabilityAsync(cmd, tenant, ct);
        return Results.Ok(new { available = true });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { available = false, reason = ex.Message });
    }
}).WithTags("Availability");

// PUT /reservations/{id}/cancel
app.MapPut("/reservations/{id}/cancel", async (Guid id, IReservationRepository repo, TenantContext tenant, CancellationToken ct) =>
{
    var tenantId = tenant.TenantId ?? Guid.Empty;
    var reservation = await repo.GetByIdAsync(id, tenantId, ct);
    if (reservation is null) return Results.NotFound();

    reservation.Status = ReserveOne.Domain.Enums.ReservationStatus.Cancelled;
    reservation.TableId = null; // libera mesa
    reservation.UltimaFechaEdicion = DateTime.UtcNow;
    reservation.IdUltimoEditor = tenant.UserId;

    await repo.SaveChangesAsync(ct);
    return Results.NoContent();
}).WithTags("Reservations");

// PUT /reservations/{id}
app.MapPut("/reservations/{id}", async (Guid id, CreateReservationCommand update, IReservationRepository repo, AvailabilityService availability, TenantContext tenant, CancellationToken ct) =>
{
    var tenantId = tenant.TenantId ?? Guid.Empty;
    var reservation = await repo.GetByIdAsync(id, tenantId, ct);
    if (reservation is null) return Results.NotFound();

    await availability.CheckAvailabilityAsync(update, tenant, ct);

    reservation.SalonId = update.SalonId;
    reservation.CustomerName = update.CustomerName;
    reservation.Phone = update.Phone;
    reservation.Fecha = update.Fecha;
    reservation.Hora = update.Hora;
    reservation.PartySize = update.PartySize;
    reservation.Source = update.Source;
    reservation.UltimaFechaEdicion = DateTime.UtcNow;
    reservation.IdUltimoEditor = tenant.UserId;

    await repo.SaveChangesAsync(ct);
    return Results.NoContent();
}).WithTags("Reservations");

// Users management
app.MapPost("/users", async (CreateUserRequest req, UserService service) =>
{
    var result = await service.CreateAsync(req);
    return Results.Created($"/users/{result.Id}", result);
}).WithTags("Users");

app.MapGet("/users", async (UserService service) =>
{
    var users = await service.GetAllAsync();
    return Results.Ok(users);
}).WithTags("Users");

app.MapPut("/users/{id}/deactivate", async (Guid id, UserService service) =>
{
    await service.DeactivateAsync(id);
    return Results.NoContent();
}).WithTags("Users");

// Restaurants management
app.MapPost("/restaurants", async (CreateRestaurantCommand cmd, CreateRestaurantHandler handler) =>
{
    var id = await handler.HandleAsync(cmd);
    return Results.Created($"/restaurants/{id}", new { id });
}).WithTags("Restaurants");

// Salon management
app.MapPost("/salons", async (CreateSalonCommand cmd, CreateSalonHandler handler) =>
{
    var id = await handler.HandleAsync(cmd);
    return Results.Created($"/salons/{id}", new { id });
}).WithTags("Salons").RequireAuthorization();

// RestaurantConfigs
app.MapGet("/restaurant-configs/{restaurantId}", async (Guid restaurantId, IRestaurantConfigRepository repo, TenantContext tenant, CancellationToken ct) =>
{
    if (tenant.TenantId is null) return Results.BadRequest(new { message = "TenantId requerido" });
    var config = await repo.GetByRestaurantAsync(restaurantId, tenant, ct);
    return config is null ? Results.NotFound() : Results.Ok(config);
}).RequireAuthorization().WithTags("RestaurantConfigs");

// POST /restaurant-configs (versioned upsert)
app.MapPost("/restaurant-configs", async (UpsertRestaurantConfigCommand cmd, RestaurantConfigVersioningService svc, TenantContext tenant, CancellationToken ct) =>
{
    if (tenant.TenantId is null) return Results.BadRequest(new { message = "TenantId requerido" });
    var created = await svc.UpsertAsync(cmd, tenant.UserId, tenant.TenantId.Value, ct);
    return Results.Created($"/restaurant-configs/{created.Id}", created);
}).RequireAuthorization().WithTags("RestaurantConfigs");

// ServicePeriods
app.MapPost("/service-periods", async (CreateServicePeriodRequest req, IServicePeriodRepository repo, ReserveOneDbContext db, TenantContext tenant, CancellationToken ct) =>
{
    if (tenant.TenantId is null) return Results.BadRequest(new { message = "TenantId requerido" });

    var exists = await db.RestaurantConfigs.AnyAsync(rc => rc.Id == req.RestaurantConfigId && rc.TenantId == tenant.TenantId, ct);
    if (!exists) return Results.BadRequest(new { message = "RestaurantConfigId no existe o no pertenece al tenant" });

    var sp = new ServicePeriod
    {
        Id = Guid.NewGuid(),
        TenantId = tenant.TenantId,
        RestaurantConfigId = req.RestaurantConfigId,
        Name = req.Name,
        StartTime = req.StartTime,
        EndTime = req.EndTime,
        IsActive = req.IsActive,
        Borrado = false,
        FechaCreacion = DateTime.UtcNow,
        UltimaFechaEdicion = DateTime.UtcNow,
        IdCreador = tenant.UserId,
        IdUltimoEditor = tenant.UserId,
    };
    await repo.AddAsync(sp, ct);
    return Results.Created($"/service-periods/{sp.Id}", new { id = sp.Id });
}).RequireAuthorization().WithTags("ServicePeriods");

app.MapGet("/service-periods", async (Guid restaurantConfigId, IServicePeriodRepository repo, TenantContext tenant, CancellationToken ct) =>
{
    if (tenant.TenantId is null) return Results.BadRequest(new { message = "TenantId requerido" });
    var list = await repo.GetByConfigAsync(restaurantConfigId, tenant.TenantId.Value, ct);
    return Results.Ok(list);
}).RequireAuthorization().WithTags("ServicePeriods");

app.Run();

static Guid? GetTenantIdFromToken(HttpContext? httpContext)
{
    var claim = httpContext?.User?.FindFirst("tenantId")?.Value;
    return Guid.TryParse(claim, out var id) ? id : null;
}

static Guid GetUserIdFromToken(HttpContext? httpContext)
{
    var claim = httpContext?.User?.FindFirst("sub")?.Value;
    return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
}
