using System.Diagnostics;
using System.Net.Http.Headers;

namespace GiftOfTheGivers2.loadtest
{
    public class PerformanceMetrics
    {
        private readonly HttpClient _client;
        private readonly List<RequestMetric> _metrics;

        public PerformanceMetrics(HttpClient client)
        {
            _client = client;
            _metrics = new List<RequestMetric>();
        }

        public async Task<RequestMetric> MeasureRequestAsync(Func<Task<HttpResponseMessage>> request)
        {
            var stopwatch = Stopwatch.StartNew();
            var startTime = DateTime.UtcNow;

            try
            {
                var response = await request();
                stopwatch.Stop();

                var metric = new RequestMetric
                {
                    Timestamp = startTime,
                    Duration = stopwatch.ElapsedMilliseconds,
                    StatusCode = (int)response.StatusCode,
                    Success = response.IsSuccessStatusCode,
                    ContentSize = response.Content.Headers.ContentLength ?? 0
                };

                _metrics.Add(metric);
                return metric;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var metric = new RequestMetric
                {
                    Timestamp = startTime,
                    Duration = stopwatch.ElapsedMilliseconds,
                    StatusCode = 0,
                    Success = false,
                    ErrorMessage = ex.Message
                };

                _metrics.Add(metric);
                return metric;
            }
        }

        public PerformanceReport GenerateReport()
        {
            if (_metrics.Count == 0)
                return new PerformanceReport();

            var successfulRequests = _metrics.Where(m => m.Success).ToList();

            return new PerformanceReport
            {
                TotalRequests = _metrics.Count,
                SuccessfulRequests = successfulRequests.Count,
                FailedRequests = _metrics.Count - successfulRequests.Count,
                AverageResponseTime = successfulRequests.Any() ? successfulRequests.Average(m => m.Duration) : 0,
                MinResponseTime = successfulRequests.Any() ? successfulRequests.Min(m => m.Duration) : 0,
                MaxResponseTime = successfulRequests.Any() ? successfulRequests.Max(m => m.Duration) : 0,
                P95ResponseTime = CalculatePercentile(successfulRequests.Select(m => m.Duration).ToList(), 95),
                Throughput = _metrics.Count / (_metrics.Max(m => m.Timestamp) - _metrics.Min(m => m.Timestamp)).TotalSeconds,
                ErrorRate = (double)(_metrics.Count - successfulRequests.Count) / _metrics.Count * 100
            };
        }

        private double CalculatePercentile(List<long> values, double percentile)
        {
            if (!values.Any()) return 0;

            values.Sort();
            var index = (int)Math.Ceiling(percentile / 100.0 * values.Count) - 1;
            return values[Math.Max(0, Math.Min(index, values.Count - 1))];
        }
    }

    public class RequestMetric
    {
        public DateTime Timestamp { get; set; }
        public long Duration { get; set; } // milliseconds
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public long ContentSize { get; set; } // bytes
        public string ErrorMessage { get; set; }
    }

    public class PerformanceReport
    {
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double AverageResponseTime { get; set; } // milliseconds
        public double MinResponseTime { get; set; } // milliseconds
        public double MaxResponseTime { get; set; } // milliseconds
        public double P95ResponseTime { get; set; } // milliseconds
        public double Throughput { get; set; } // requests per second
        public double ErrorRate { get; set; } // percentage

        public void PrintReport()
        {
            Console.WriteLine("=== PERFORMANCE REPORT ===");
            Console.WriteLine($"Total Requests: {TotalRequests}");
            Console.WriteLine($"Successful: {SuccessfulRequests}");
            Console.WriteLine($"Failed: {FailedRequests}");
            Console.WriteLine($"Error Rate: {ErrorRate:F2}%");
            Console.WriteLine($"Average Response Time: {AverageResponseTime:F2} ms");
            Console.WriteLine($"Min Response Time: {MinResponseTime:F2} ms");
            Console.WriteLine($"Max Response Time: {MaxResponseTime:F2} ms");
            Console.WriteLine($"95th Percentile: {P95ResponseTime:F2} ms");
            Console.WriteLine($"Throughput: {Throughput:F2} req/sec");
            Console.WriteLine("===========================");
        }
    }
}

