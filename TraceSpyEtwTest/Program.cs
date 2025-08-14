using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using EventProviderLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TraceSpyEtwTest;

public class Program
{
    private static readonly Random _rnd = new Random(Environment.TickCount);

    [RequiresUnreferencedCode("")]
    static async Task Main(string[] args)
    {
        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services
                    .AddSingleton<Logger>()
                    .AddLogging(loggingBuilder =>
                    {
                        // for ETW events, we must add the EventProvider like this:
                        loggingBuilder.AddEventProvider();

                        // MaxQueueLength = 1 otherwise we loose messages
                        loggingBuilder.AddConsole(config => config.MaxQueueLength = 1).AddSimpleConsole(config => config.SingleLine = true);
                    });

            }).Build();

        await TestMain(host);
    }

    static async Task TestMain(IHost host)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            cts.Cancel();
            e.Cancel = true;
        };

        var logger = host.Services.GetRequiredService<Logger>();
        var start = DateTime.Now;
        logger.LogInformation($"Start time: {start}");
        logger.LogWarning($"Press CTRL+C to stop...");

        var count = 1;
        do
        {
            cts.Token.ThrowIfCancellationRequested();

            await Task.Delay(_rnd.Next(100, 500), cts.Token).ConfigureAwait(false);

            var isError = _rnd.Next(0, 10) < 2; // 20% chance of error
            if (isError)
            {
                var phrase = $"ETWTrace ERROR #{count} from TraceSpyTest. Date:{DateTime.Now}";
                logger.LogError(phrase);
            }
            else
            {
                var phrase = $"ETWTrace #{count} from TraceSpyTest. Date:{DateTime.Now}";
                logger.LogInformation(phrase);
            }
            count++;
        }
        while (true);
    }

    private sealed class Logger(ILogger<Program> logger) : ILoggable<Program>
    {
        ILogger<Program>? ILoggable<Program>.Logger => logger;
    }
}
