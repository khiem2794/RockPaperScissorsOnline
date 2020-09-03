using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Infrastructure
{
    public class JwtRefreshTokenCache : IHostedService, IDisposable
    {
        private readonly IJwtAuthManager _jwtAuthManager;
        private readonly ILogger<JwtRefreshTokenCache> _logger;
        private Timer _timer;
        public JwtRefreshTokenCache(IJwtAuthManager _jwtAuthManager, ILogger<JwtRefreshTokenCache> logger)
        {
            this._logger = logger;
            this._jwtAuthManager = _jwtAuthManager;

        }

        private void DoWork(object state)
        {
            _jwtAuthManager.RemoveExpiredRefreshTokens(DateTime.Now);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
