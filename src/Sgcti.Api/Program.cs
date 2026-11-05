using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using QuestPDF.Infrastructure;
using Sgcti.Api.Hubs;
using Sgcti.Api.Services;
using Sgcti.Core.Abstractions;
using Sgcti.Core.Entities;
using Sgcti.Core.Services;
using Sgcti.Infrastructure.Persistence;
using Sgcti.Infrastructure.Repositories;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("No se configuró 'Jwt:Key'.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("No se configuró 'Jwt:Issuer'.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtIssuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Global", policy => policy
        .WithOrigins(
            "http://localhost:5173",
            "http://localhost:5174",
            "http://localhost:4173",
            "http://localhost:3000",
            "http://127.0.0.1:5173",
            "http://127.0.0.1:5174")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddSignalR();

builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Escribe el token JWT obtenido en POST /api/Auth/login."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

builder.Services.AddDbContext<SgctiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SgctiDB")));

builder.Services.AddScoped<IImpresoraRepositorio, ImpresoraRepositorio>();
builder.Services.AddScoped<ITicketRepositorio, TicketRepositorio>();
builder.Services.AddScoped<ServicioAnalisisPredictivo>();

builder.Services.AddTransient<IServicioBuscadorManuales, ServicioBuscadorManuales>();

builder.Services.AddHttpClient<IServicioAsistenteIA, ServicioAsistenteIA>();

builder.Services.AddHostedService<ServicioMonitoreoRed>();

builder.Services.AddHostedService<ServicioAlertasAutomaticas>();

builder.Services.AddHostedService<ServicioTelegramBot>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseCors("Global");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificacionesHub>("/hubs/notificaciones");

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<SgctiDbContext>();

        db.Database.ExecuteSqlRaw("""
            ALTER TABLE "Tickets" ADD COLUMN IF NOT EXISTS "Departamento" character varying(100);
            ALTER TABLE "Tickets" ADD COLUMN IF NOT EXISTS "Categoria" character varying(100);
            ALTER TABLE "Tickets" ADD COLUMN IF NOT EXISTS "EquipoAfectado" character varying(150);
            ALTER TABLE "Tickets" ADD COLUMN IF NOT EXISTS "SolucionAplicada" character varying(2000);
            ALTER TABLE "Tickets" ADD COLUMN IF NOT EXISTS "SuministrosUtilizados" character varying(1000);
            ALTER TABLE "Tickets" ADD COLUMN IF NOT EXISTS "FechaResolucion" timestamp with time zone;
            ALTER TABLE "Tickets" ADD COLUMN IF NOT EXISTS "Solicitante" character varying(150);
            ALTER TABLE "Tickets" ADD COLUMN IF NOT EXISTS "TelegramChatId" character varying(100);
            ALTER TABLE "Tickets" ADD COLUMN IF NOT EXISTS "CodigoNomenclatura" character varying(30);
            CREATE INDEX IF NOT EXISTS "IX_Tickets_Departamento" ON "Tickets" ("Departamento");
            CREATE INDEX IF NOT EXISTS "IX_Tickets_Categoria" ON "Tickets" ("Categoria");
            CREATE INDEX IF NOT EXISTS "IX_Tickets_Solicitante" ON "Tickets" ("Solicitante");
            CREATE INDEX IF NOT EXISTS "IX_Tickets_TelegramChatId" ON "Tickets" ("TelegramChatId");
            DROP INDEX IF EXISTS "IX_Tickets_CodigoNomenclatura";
            CREATE INDEX IF NOT EXISTS "IX_Tickets_CodigoNomenclatura" ON "Tickets" ("CodigoNomenclatura");

            ALTER TABLE "Usuarios" ADD COLUMN IF NOT EXISTS "Cargo" character varying(100);
            ALTER TABLE "Usuarios" ADD COLUMN IF NOT EXISTS "Departamento" character varying(100);
            ALTER TABLE "Usuarios" ADD COLUMN IF NOT EXISTS "PrioridadPersonalizada" character varying(20);
            ALTER TABLE "Usuarios" ADD COLUMN IF NOT EXISTS "TelegramUsuarioOId" character varying(100);
            CREATE INDEX IF NOT EXISTS "IX_Usuarios_Departamento" ON "Usuarios" ("Departamento");
            CREATE INDEX IF NOT EXISTS "IX_Usuarios_TelegramUsuarioOId" ON "Usuarios" ("TelegramUsuarioOId");

            CREATE TABLE IF NOT EXISTS "Departamentos" (
                "Id" integer GENERATED BY DEFAULT AS IDENTITY,
                "Nombre" character varying(100) NOT NULL,
                "Sigla" character varying(10) NOT NULL,
                "PrioridadPorDefecto" character varying(20) NOT NULL,
                CONSTRAINT "PK_Departamentos" PRIMARY KEY ("Id")
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Departamentos_Nombre" ON "Departamentos" ("Nombre");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Departamentos_Sigla" ON "Departamentos" ("Sigla");
            INSERT INTO "Departamentos" ("Nombre", "Sigla", "PrioridadPorDefecto") VALUES
                ('Gerencia', 'GER', 'Alta'),
                ('Facturación', 'FAC', 'Media'),
                ('Almacén', 'ALM', 'Media'),
                ('Administración', 'ADM', 'Media'),
                ('Operaciones', 'OPE', 'Media'),
                ('TI', 'TI', 'Media')
            ON CONFLICT ("Nombre") DO NOTHING;

            CREATE TABLE IF NOT EXISTS "CategoriasTicket" (
                "Id" integer GENERATED BY DEFAULT AS IDENTITY,
                "Nombre" character varying(100) NOT NULL,
                CONSTRAINT "PK_CategoriasTicket" PRIMARY KEY ("Id")
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_CategoriasTicket_Nombre" ON "CategoriasTicket" ("Nombre");
            INSERT INTO "CategoriasTicket" ("Nombre") VALUES
                ('Hardware'),
                ('Software'),
                ('Impresoras'),
                ('Periféricos'),
                ('Red')
            ON CONFLICT ("Nombre") DO NOTHING;
            """);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(
            ex,
            "No se pudo verificar el esquema de Tickets al iniciar la aplicación.");
    }
}

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<SgctiDbContext>();

        var empleadosSemilla = new[]
        {
            new EmpleadoSemilla("Laura Gómez", "Gerente General", "Gerencia", "laura.gomez", "0424-4910238", PrioridadTicket.Alta),
            new EmpleadoSemilla("Carlos Pérez", "Analista de Facturación", "Facturación", "carlos.perez", "0414-3289104", null),
            new EmpleadoSemilla("María Rodríguez", "Encargada de Almacén", "Almacén", "maria.rodriguez", "0416-6421908", null),
            new EmpleadoSemilla("José Martínez", "Asistente Administrativo", "Administración", "jose.martinez", "0412-8841209", null),
            new EmpleadoSemilla("Ana Torres", "Supervisora de Operaciones", "Operaciones", "ana.torres", "0414-4092817", null),
            new EmpleadoSemilla("Luis Ramírez", "Técnico de Soporte TI", "TI", "luis.ramirez", "0424-3117845", null)
        };

        var usuarios = await db.Usuarios.ToListAsync();

        foreach (var empleadoExistente in usuarios.Where(u => u.Rol == Usuario.RolEmpleado))
        {
            empleadoExistente.PasswordHash = string.Empty;
        }

        foreach (var semilla in empleadosSemilla)
        {
            if (usuarios.Any(u => string.Equals(u.NombreCompleto, semilla.NombreCompleto, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var username = semilla.Username;
            var sufijo = 1;

            while (usuarios.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)))
            {
                sufijo++;
                username = $"{semilla.Username}{sufijo}";
            }

            var empleado = new Usuario
            {
                NombreCompleto = semilla.NombreCompleto,
                Username = username,
                PasswordHash = string.Empty,
                Rol = Usuario.RolEmpleado,
                Activo = true,
                Cargo = semilla.Cargo,
                Departamento = semilla.Departamento,
                PrioridadPersonalizada = semilla.Prioridad,
                TelegramUsuarioOId = semilla.Telefono
            };

            db.Usuarios.Add(empleado);
            usuarios.Add(empleado);
        }

        await db.SaveChangesAsync();

        var departamentos = await db.Departamentos.AsNoTracking().ToListAsync();
        var categorias = await db.CategoriasTicket
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => c.Nombre)
            .ToListAsync();

        categorias = categorias
            .Where(nombre => !string.IsNullOrWhiteSpace(nombre) && !RequiereDatos(nombre))
            .ToList();

        if (categorias.Count == 0)
        {
            categorias = ["Hardware", "Software", "Impresoras", "Periféricos", "Red"];
        }

        var equipos = new[]
        {
            "PC-005",
            "Monitor Dell",
            "Antivirus Corporativo",
            "Mouse USB",
            "Impresora MF455"
        };

        var tickets = await db.Tickets.OrderBy(t => t.Id).ToListAsync();

        for (var indice = 0; indice < tickets.Count; indice++)
        {
            var ticket = tickets[indice];
            var empleado = empleadosSemilla[indice % empleadosSemilla.Length];

            if (RequiereDatos(ticket.Departamento))
            {
                ticket.Departamento = empleado.Departamento;
            }

            if (RequiereDatos(ticket.Solicitante))
            {
                ticket.Solicitante = TicketsDelDepartamento(
                    empleadosSemilla,
                    ticket.Departamento,
                    indice).NombreCompleto;
            }

            if (RequiereDatos(ticket.Categoria))
            {
                ticket.Categoria = categorias[indice % categorias.Count];
            }

            if (RequiereDatos(ticket.EquipoAfectado))
            {
                ticket.EquipoAfectado = equipos[indice % equipos.Length];
            }
        }

        GeneradorNomenclaturaTicket.RecalcularConsecutivosPorDepartamento(tickets, departamentos);
        await db.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(
            ex,
            "No se pudieron completar los datos históricos de tickets y empleados.");
    }
}

app.Run();

static bool RequiereDatos(string? valor)
{
    if (string.IsNullOrWhiteSpace(valor))
    {
        return true;
    }

    return valor.Trim().ToLowerInvariant() is
        "sin departamento" or
        "sin categoria" or
        "sin categoría" or
        "sin solicitante" or
        "sin asignar" or
        "sin area" or
        "sin área" or
        "n/a" or
        "-";
}

static EmpleadoSemilla TicketsDelDepartamento(
    EmpleadoSemilla[] empleados,
    string? departamento,
    int indice)
{
    if (!string.IsNullOrWhiteSpace(departamento))
    {
        var coincidencia = empleados.FirstOrDefault(e =>
            string.Equals(e.Departamento, departamento, StringComparison.OrdinalIgnoreCase));

        if (coincidencia is not null)
        {
            return coincidencia;
        }
    }

    return empleados[indice % empleados.Length];
}

internal sealed record EmpleadoSemilla(
    string NombreCompleto,
    string Cargo,
    string Departamento,
    string Username,
    string Telefono,
    PrioridadTicket? Prioridad);