using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;

namespace SharedKernel.Config;

public static class LoggingConfig
{
    /// <summary>
    /// Gọi trong Program.cs: builder.Host.AddSharedLogging()
    /// Chỉ ghi log ở mức Error trở lên, và KHÔNG ghi khi chạy Development.
    /// </summary>
    public static IHostBuilder AddSharedLogging(this IHostBuilder host)
    {
        return host.UseSerilog((ctx, services, cfg) =>
        {
            // Nếu đang chạy Development → không cấu hình sink nào cả (im lặng hoàn toàn)
            if (ctx.HostingEnvironment.IsDevelopment())
            {
                cfg.MinimumLevel.Fatal(); // đặt mức cực cao để không log gì (an toàn hơn WriteTo rỗng)
                return;
            }

            cfg.ReadFrom.Configuration(ctx.Configuration)
               .ReadFrom.Services(services)
               .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
               .MinimumLevel.Override("System", LogEventLevel.Error)
               .MinimumLevel.Error() // chỉ giữ Error trở lên (Error, Fatal)
               .Enrich.FromLogContext()
               .Enrich.WithProperty("App", ctx.HostingEnvironment.ApplicationName)
               .Enrich.WithProperty("Env", ctx.HostingEnvironment.EnvironmentName);

            // Console — chỉ hiện lỗi
            cfg.WriteTo.Console(
                restrictedToMinimumLevel: LogEventLevel.Error,
                outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

            // File — ghi lỗi theo ngày, giữ 30 ngày
            cfg.WriteTo.File(
                path: "logs/error-.log",
                restrictedToMinimumLevel: LogEventLevel.Error,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

            // Seq — chỉ bật nếu có config, chỉ gửi lỗi
            var seqUrl = ctx.Configuration["Seq:ServerUrl"];
            if (!string.IsNullOrWhiteSpace(seqUrl))
                cfg.WriteTo.Seq(seqUrl, restrictedToMinimumLevel: LogEventLevel.Error);
        });
    }

    /// <summary>
    /// Gọi sau app.Build(): app.UseSharedRequestLogging()
    /// Chỉ log các request bị lỗi (exception hoặc status code >= 400).
    /// KHÔNG log khi chạy Development.
    /// </summary>
    public static IApplicationBuilder UseSharedRequestLogging(
        this IApplicationBuilder app)
    {
        var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
        if (env.IsDevelopment())
            return app; // bỏ qua hoàn toàn, không đăng ký middleware log request

        app.UseSerilogRequestLogging(opts =>
        {
            opts.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} → {StatusCode} ({Elapsed:0.000}ms)";

            opts.GetLevel = (ctx, _, ex) =>
                ex != null || ctx.Response.StatusCode >= 400
                    ? LogEventLevel.Error
                    : LogEventLevel.Verbose;
        });
        return app;
    }
}