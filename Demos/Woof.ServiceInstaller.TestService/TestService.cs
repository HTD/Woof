using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Woof.ServiceInstaller.TestService;

class TestService : IHostedService {

    ILogger<TestService> Logger { get; }

    public TestService(ILogger<TestService> logger) => Logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken) {
        await Task.Delay(1000, cancellationToken);
        Logger.LogDebug("Test service started.");
    }

    public async Task StopAsync(CancellationToken cancellationToken) {
        await Task.Delay(1000, cancellationToken);
        Logger.LogDebug("Test service stopped.");
    }

}