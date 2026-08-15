using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services.BL.Interface.Chat;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OFMS_API.Services
{
    /// <summary>
    /// Automatic background service that archives and truncates chat messages older than 5 days every 24 hours.
    /// </summary>
    public class ChatCleanupHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ChatCleanupHostedService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

        public ChatCleanupHostedService(IServiceProvider serviceProvider, ILogger<ChatCleanupHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Chat Cleanup Automated Service started. Running every 24 hours to archive & truncate messages older than 5 days.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var chatBL = scope.ServiceProvider.GetRequiredService<IChatBL>();
                        var (archived, deleted) = await chatBL.ArchiveAndCleanupChatMessagesAsync(retentionDays: 5);
                        _logger.LogInformation("Chat Cleanup Automation complete: Archived {Archived} messages and deleted {Deleted} old messages.", archived, deleted);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during automated chat message cleanup & archive task.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
