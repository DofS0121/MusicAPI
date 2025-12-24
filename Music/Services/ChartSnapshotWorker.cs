using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Music.Services;

namespace Music.Services
{
    public class ChartSnapshotWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ChartSnapshotWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var snapshotService =
                    scope.ServiceProvider.GetRequiredService<ChartSnapshotService>();

                // 🔥 REALTIME – mỗi 15 phút
                await snapshotService.CreateSnapshotAsync("realtime");

                // 🔥 DAILY – 00:00 UTC
                if (DateTime.UtcNow.Hour == 0)
                    await snapshotService.CreateSnapshotAsync("daily");

                // 🔥 WEEKLY – Chủ nhật 00:00 UTC
                if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday &&
                    DateTime.UtcNow.Hour == 0)
                    await snapshotService.CreateSnapshotAsync("weekly");

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }
}
