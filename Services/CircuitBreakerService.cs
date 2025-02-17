using Polly;
using Polly.CircuitBreaker;

namespace FansVoice.UserService.Services
{
    public interface ICircuitBreakerService
    {
        Task<T> ExecuteAsync<T>(Func<Task<T>> action, string operationKey);
    }

    public class CircuitBreakerService : ICircuitBreakerService
    {
        private readonly ILogger<CircuitBreakerService> _logger;
        private readonly Dictionary<string, AsyncCircuitBreakerPolicy> _policies;

        public CircuitBreakerService(ILogger<CircuitBreakerService> logger)
        {
            _logger = logger;
            _policies = new Dictionary<string, AsyncCircuitBreakerPolicy>();
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, string operationKey)
        {
            var policy = GetOrCreatePolicy(operationKey);

            try
            {
                return await policy.ExecuteAsync(action);
            }
            catch (BrokenCircuitException)
            {
                _logger.LogError("Circuit breaker is open for operation {OperationKey}", operationKey);
                throw;
            }
        }

        private AsyncCircuitBreakerPolicy GetOrCreatePolicy(string operationKey)
        {
            if (!_policies.ContainsKey(operationKey))
            {
                var policy = Policy
                    .Handle<Exception>()
                    .CircuitBreakerAsync(
                        exceptionsAllowedBeforeBreaking: 3,
                        durationOfBreak: TimeSpan.FromSeconds(30),
                        onBreak: (exception, duration) =>
                        {
                            _logger.LogWarning("Circuit breaker opened for {Duration}s due to: {Exception}",
                                duration.TotalSeconds, exception.Message);
                        },
                        onReset: () =>
                        {
                            _logger.LogInformation("Circuit breaker reset for {OperationKey}", operationKey);
                        });

                _policies[operationKey] = policy;
            }

            return _policies[operationKey];
        }
    }
}