using System;
using System.Collections.Concurrent;

namespace CircuitBreakerExample
{
    internal class Program
    {
        private const string PolicyKey = "sertunc";

        static void Main(string[] args)
        {
            for (int i = 0; i < 10; i++)
            {
                CircuitBreakerPolicy.Retry(
                    policyKey: PolicyKey,
                    tryCount: 3,
                    action: () =>
                    {
                        Console.WriteLine("Merhaba Dünya");
                    },
                    onRetry: (exception, retryCount) =>
                    {
                        Console.WriteLine("Hata: " + exception?.Message);
                        Console.WriteLine("RetryCount: " + retryCount);
                        Console.WriteLine();
                    });

                if (i == 7)
                    CircuitBreakerPolicy.RemovePolicyKey(PolicyKey);
            }

            Console.ReadLine();
        }

        public class CircuitBreakerPolicy
        {
            private static readonly ConcurrentDictionary<string, int> _retryList = new ConcurrentDictionary<string, int>();

            public static void Retry(string policyKey, int tryCount, Action action, Action<Exception, int> onRetry = null)
            {
                try
                {
                    if (!_retryList.ContainsKey(policyKey))
                    {
                        _retryList[policyKey] = 0;
                    }

                    if (_retryList[policyKey] < tryCount)
                    {
                        _retryList[policyKey]++;
                        action?.Invoke();
                        onRetry?.Invoke(null, _retryList[policyKey]);
                    }
                }
                catch (Exception ex)
                {
                    onRetry?.Invoke(ex, _retryList[policyKey]);
                }
            }

            public static bool RemovePolicyKey(string policyKey)
            {
                return _retryList.TryRemove(policyKey, out _);
            }
        }
    }
}
