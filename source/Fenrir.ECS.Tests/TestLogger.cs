using Microsoft.Extensions.Logging;

namespace Fenrir.ECS.Tests
{
    internal class TestLogger : Fenrir.Multiplayer.ILogger
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public TestLogger(ILogger<TestLogger> logger)
        {
            _logger = logger;
        }

        public void Critical(string format, params object[] arguments) => _logger.LogCritical(format, arguments);
        public void Debug(string format, params object[] arguments) => _logger.LogDebug(format, arguments);
        public void Error(string format, params object[] arguments) => _logger.LogError(format, arguments);
        public void Info(string format, params object[] arguments) => _logger.LogInformation(format, arguments);
        public void Trace(string format, params object[] arguments) => _logger.LogTrace(format, arguments);
        public void Warning(string format, params object[] arguments) => _logger.LogWarning(format, arguments);
    }
}
