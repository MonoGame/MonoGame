using System;
using Cake.Frosting;
using Cake.Core.Composition;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

return new CakeHost()
    .UseWorkingDirectory("../")
    .UseContext<BuildScripts.BuildContext>()
    .ConfigureServices(services =>
    {
        services.AddSingleton<Func<string, HttpClient>>(serviceProvider =>
        {
            return key =>
            {
                // Default to 2 minutes.
                // This will hopefully avoid the timeouts caused by the default 100 seconds.
                var timeout = TimeSpan.FromMinutes(2);

                // Extract override from an environment variable.
                var timeoutVariable = Environment.GetEnvironmentVariable("HTTPCLIENT_TIMEOUT");
                if (int.TryParse(timeoutVariable, out var timeoutInSeconds)
                    && timeoutInSeconds > 0)
                {
                    timeout = TimeSpan.FromSeconds(timeoutInSeconds);
                }

                var client = new HttpClient()
                {
                    Timeout = timeout,
                };

                return client;
            };
        });
    })
    .Run(args);
