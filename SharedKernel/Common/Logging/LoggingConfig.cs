using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Logging;

public static class LoggingConfig
{
    /// <summary>
    /// Gọi trong Program.cs: builder.Host.AddSharedLogging()
    /// </summary>
    public static IHostBuilder AddSharedLogging(this IHostBuilder host)
    {
        return host.UseSerilog((ctx, services, cfg) =>
        {
            cfg.ReadFrom.Configuration(ctx.Configuration)
               .ReadFrom.Services(services)
               .Enrich.FromLogContext()
               .Enrich.WithProperty("App", ctx.HostingEnvironment.ApplicationName)
               .Enrich.WithProperty("Env", ctx.HostingEnvironment.EnvironmentName);

            // Console — luôn bật
            cfg.WriteTo.Console(
                outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

            // File — ghi theo ngày, giữ 30 ngày
            cfg.WriteTo.File(
                path: "logs/app-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

            // Seq — chỉ bật nếu có config
            var seqUrl = ctx.Configuration["Seq:ServerUrl"];
            if (!string.IsNullOrWhiteSpace(seqUrl))
                cfg.WriteTo.Seq(seqUrl);
        });
    }

    /// <summary>
    /// Gọi sau app.Build(): app.UseSharedRequestLogging()
    /// </summary>
    public static IApplicationBuilder UseSharedRequestLogging(
        this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(opts =>
        {
            opts.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} → {StatusCode} ({Elapsed:0.000}ms)";

            // Không log các request health check
            opts.GetLevel = (ctx, _, ex) =>
                ex != null || ctx.Response.StatusCode >= 500
                    ? LogEventLevel.Error
                    : ctx.Request.Path.StartsWithSegments("/health")
                        ? LogEventLevel.Verbose
                        : LogEventLevel.Information;
        });

        return app;
    }
}