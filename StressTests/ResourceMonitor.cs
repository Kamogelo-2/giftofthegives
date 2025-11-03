using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace GiftOfTheGivers2.StressTests
{
    public class ResourceMonitor : IDisposable
    {
        private readonly Meter _meter;
        private readonly Counter<long> _requestCounter;
        private readonly ObservableGauge<long> _memoryGauge;
        private readonly ObservableGauge<double> _cpuGauge;
        private readonly Process _process;
        private readonly Timer _timer;
        private bool _disposed = false;

        public ResourceMonitor(string testName)
        {
            _meter = new Meter($"GiftOfTheGivers.StressTests.{testName}");
            _requestCounter = _meter.CreateCounter<long>("stress_test_requests");
            _memoryGauge = _meter.CreateObservableGauge<long>("memory_usage", GetMemoryUsage);
            _cpuGauge = _meter.CreateObservableGauge<double>("cpu_usage", GetCpuUsage);

            _process = Process.GetCurrentProcess();
            _timer = new Timer(CollectMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        public void RecordRequest(bool success)
        {
            _requestCounter.Add(1, new KeyValuePair<string, object>("success", success));
        }

        private Measurement<long> GetMemoryUsage()
        {
            return new Measurement<long>(_process.WorkingSet64);
        }

        private Measurement<double> GetCpuUsage()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = _process.TotalProcessorTime;

            Thread.Sleep(100);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = _process.TotalProcessorTime;

            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            return new Measurement<double>(cpuUsageTotal * 100);
        }

        private void CollectMetrics(object state)
        {
            // Force garbage collection to get accurate memory readings
            if (DateTime.UtcNow.Second % 30 == 0) // Every 30 seconds
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        public async Task<SystemResourceSnapshot> TakeSnapshot()
        {
            return new SystemResourceSnapshot
            {
                Timestamp = DateTime.UtcNow,
                MemoryUsage = _process.WorkingSet64,
                PrivateMemory = _process.PrivateMemorySize64,
                HandleCount = _process.HandleCount,
                ThreadCount = _process.Threads.Count,
                CpuUsage = await GetCurrentCpuUsageAsync()
            };
        }

        private async Task<double> GetCurrentCpuUsageAsync()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = _process.TotalProcessorTime;

            await Task.Delay(1000);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = _process.TotalProcessorTime;

            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;

            return (cpuUsedMs / (Environment.ProcessorCount * totalMsPassed)) * 100;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _timer?.Dispose();
                _meter?.Dispose();
                _disposed = true;
            }
        }
    }

    public class SystemResourceSnapshot
    {
        public DateTime Timestamp { get; set; }
        public long MemoryUsage { get; set; }
        public long PrivateMemory { get; set; }
        public int HandleCount { get; set; }
        public int ThreadCount { get; set; }
        public double CpuUsage { get; set; }

        public void PrintSnapshot()
        {
            Console.WriteLine($"[{Timestamp:HH:mm:ss}] Memory: {MemoryUsage / 1024 / 1024}MB, " +
                            $"CPU: {CpuUsage:F1}%, Threads: {ThreadCount}, Handles: {HandleCount}");
        }
    }
}

