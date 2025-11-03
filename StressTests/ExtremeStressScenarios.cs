using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using GiftOfTheGivers2.Data;
using GiftOfTheGivers2.Models;
using System.Net;
using System.Text;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers2.StressTests
{
    public class ExtremeStressScenarios
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ApplicationDbContext _dbContext;
        private readonly List<StressTestResult> _results;

        public ExtremeStressScenarios()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                MaxResponseContentBufferSize = 1024 * 1024 * 100, // 100MB
                HandleCookies = true
            });
            _dbContext = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _results = new List<StressTestResult>();
        }

        public async Task<StressTestResult> SuddenTrafficSpikeTest(int initialUsers, int spikeUsers, int durationSeconds)
        {
            var result = new StressTestResult
            {
                TestName = "Sudden Traffic Spike",
                StartTime = DateTime.UtcNow
            };

            try
            {
                Console.WriteLine($"Starting sudden traffic spike test: {initialUsers} -> {spikeUsers} users over {durationSeconds}s");

                // Phase 1: Normal load
                var normalTasks = new List<Task>();
                for (int i = 0; i < initialUsers; i++)
                {
                    normalTasks.Add(ExecuteContinuousUserWorkflow(i, durationSeconds / 2));
                }

                // Phase 2: Sudden spike after 30 seconds
                await Task.Delay(30000);

                var spikeTasks = new List<Task>();
                for (int i = initialUsers; i < spikeUsers; i++)
                {
                    spikeTasks.Add(ExecuteContinuousUserWorkflow(i, durationSeconds / 2));
                }

                var allTasks = normalTasks.Concat(spikeTasks);
                await Task.WhenAll(allTasks);

                result.SuccessfulRequests = allTasks.Count(t => t.IsCompletedSuccessfully);
                result.FailedRequests = allTasks.Count(t => t.IsFaulted);
                result.EndTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.EndTime = DateTime.UtcNow;
            }

            _results.Add(result);
            return result;
        }

        public async Task<StressTestResult> DatabaseConnectionPoolExhaustionTest(int concurrentConnections)
        {
            var result = new StressTestResult
            {
                TestName = "Database Connection Pool Exhaustion",
                StartTime = DateTime.UtcNow
            };

            try
            {
                Console.WriteLine($"Testing database connection pool with {concurrentConnections} concurrent connections");

                var tasks = new List<Task>();
                var semaphore = new SemaphoreSlim(50); // Limit to 50 concurrent database operations

                for (int i = 0; i < concurrentConnections; i++)
                {
                    await semaphore.WaitAsync();
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await ExecuteComplexDatabaseOperations(i);
                            Interlocked.Increment(ref result.SuccessfulRequests);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Database operation failed: {ex.Message}");
                            Interlocked.Increment(ref result.FailedRequests);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks);
                result.EndTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.EndTime = DateTime.UtcNow;
            }

            _results.Add(result);
            return result;
        }

        public async Task<StressTestResult> MemoryLeakStressTest(int durationMinutes)
        {
            var result = new StressTestResult
            {
                TestName = "Memory Leak Stress Test",
                StartTime = DateTime.UtcNow
            };

            var startMemory = Process.GetCurrentProcess().WorkingSet64;
            var memoryReadings = new List<long>();
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(durationMinutes));

            try
            {
                Console.WriteLine($"Starting memory leak stress test for {durationMinutes} minutes");
                Console.WriteLine($"Initial memory: {startMemory / 1024 / 1024} MB");

                while (!cts.Token.IsCancellationRequested)
                {
                    // Create memory-intensive operations
                    await ExecuteMemoryIntensiveOperations();

                    // Force garbage collection to see real memory usage
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    var currentMemory = Process.GetCurrentProcess().WorkingSet64;
                    memoryReadings.Add(currentMemory);

                    Console.WriteLine($"Current memory: {currentMemory / 1024 / 1024} MB");

                    if (currentMemory > startMemory * 2) // If memory doubles
                    {
                        result.ErrorMessage = $"Potential memory leak detected: Memory increased from {startMemory / 1024 / 1024}MB to {currentMemory / 1024 / 1024}MB";
                        break;
                    }

                    await Task.Delay(5000); // Check every 5 seconds
                }

                result.EndTime = DateTime.UtcNow;
                result.MemoryUsageReadings = memoryReadings;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.EndTime = DateTime.UtcNow;
            }

            _results.Add(result);
            return result;
        }

        public async Task<StressTestResult> CPUStressTest(int concurrentComplexOperations, int durationSeconds)
        {
            var result = new StressTestResult
            {
                TestName = "CPU Stress Test",
                StartTime = DateTime.UtcNow
            };

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(durationSeconds));
            var cpuReadings = new List<double>();

            try
            {
                Console.WriteLine($"Starting CPU stress test with {concurrentComplexOperations} complex operations");

                var tasks = new List<Task>();
                for (int i = 0; i < concurrentComplexOperations; i++)
                {
                    tasks.Add(ExecuteCPUIntensiveOperations(i, cts.Token));
                }

                // Monitor CPU usage
                var monitorTask = Task.Run(async () =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        var cpuUsage = await GetCurrentCpuUsage();
                        cpuReadings.Add(cpuUsage);
                        await Task.Delay(1000);
                    }
                });

                await Task.WhenAll(tasks);
                cts.Cancel();

                result.CpuUsageReadings = cpuReadings;
                result.EndTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.EndTime = DateTime.UtcNow;
            }

            _results.Add(result);
            return result;
        }

        public async Task<StressTestResult> NetworkLatencyStressTest(int concurrentRequests, int artificialDelayMs)
        {
            var result = new StressTestResult
            {
                TestName = "Network Latency Stress Test",
                StartTime = DateTime.UtcNow
            };

            try
            {
                Console.WriteLine($"Testing with {concurrentRequests} concurrent requests and {artificialDelayMs}ms delay");

                var tasks = new List<Task<HttpResponseMessage>>();

                // Simulate network latency
                _client.DefaultRequestHeaders.Add("X-Artificial-Delay", artificialDelayMs.ToString());

                for (int i = 0; i < concurrentRequests; i++)
                {
                    tasks.Add(ExecuteRequestWithTimeout($"/Home/Index?cacheBust={Guid.NewGuid()}", TimeSpan.FromSeconds(30)));
                }

                var responses = await Task.WhenAll(tasks);

                result.SuccessfulRequests = responses.Count(r => r.IsSuccessStatusCode);
                result.FailedRequests = responses.Count(r => !r.IsSuccessStatusCode);
                result.TimeoutRequests = responses.Count(r => r.StatusCode == HttpStatusCode.RequestTimeout);
                result.EndTime = DateTime.UtcNow;

                _client.DefaultRequestHeaders.Remove("X-Artificial-Delay");
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.EndTime = DateTime.UtcNow;
            }

            _results.Add(result);
            return result;
        }

        public async Task<StressTestResult> DatabaseDeadlockTest(int concurrentTransactions)
        {
            var result = new StressTestResult
            {
                TestName = "Database Deadlock Test",
                StartTime = DateTime.UtcNow
            };

            try
            {
                Console.WriteLine($"Testing database deadlocks with {concurrentTransactions} concurrent transactions");

                var tasks = new List<Task>();
                var deadlockCount = 0;

                for (int i = 0; i < concurrentTransactions; i++)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await ExecutePotentialDeadlockTransaction(i);
                            Interlocked.Increment(ref result.SuccessfulRequests);
                        }
                        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("deadlock") == true)
                        {
                            Interlocked.Increment(ref deadlockCount);
                            Interlocked.Increment(ref result.FailedRequests);
                        }
                        catch (Exception)
                        {
                            Interlocked.Increment(ref result.FailedRequests);
                        }
                    }));
                }

                await Task.WhenAll(tasks);
                result.EndTime = DateTime.UtcNow;

                if (deadlockCount > 0)
                {
                    result.ErrorMessage = $"Detected {deadlockCount} potential deadlock situations";
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.EndTime = DateTime.UtcNow;
            }

            _results.Add(result);
            return result;
        }

        public async Task<StressTestResult> FileSystemStressTest(int concurrentFileOperations)
        {
            var result = new StressTestResult
            {
                TestName = "File System Stress Test",
                StartTime = DateTime.UtcNow
            };

            try
            {
                Console.WriteLine($"Testing file system with {concurrentFileOperations} concurrent operations");

                var tempPath = Path.GetTempPath();
                var tasks = new List<Task>();

                for (int i = 0; i < concurrentFileOperations; i++)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await ExecuteFileSystemOperations(tempPath, i);
                            Interlocked.Increment(ref result.SuccessfulRequests);
                        }
                        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                        {
                            Console.WriteLine($"File operation failed: {ex.Message}");
                            Interlocked.Increment(ref result.FailedRequests);
                        }
                    }));
                }

                await Task.WhenAll(tasks);
                result.EndTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.EndTime = DateTime.UtcNow;
            }

            _results.Add(result);
            return result;
        }

        public async Task<StressTestResult> SessionStateStressTest(int concurrentSessions)
        {
            var result = new StressTestResult
            {
                TestName = "Session State Stress Test",
                StartTime = DateTime.UtcNow
            };

            try
            {
                Console.WriteLine($"Testing session state with {concurrentSessions} concurrent sessions");

                var tasks = new List<Task>();
                var cookies = new Dictionary<int, CookieContainer>();

                for (int i = 0; i < concurrentSessions; i++)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await MaintainUserSession(i);
                            Interlocked.Increment(ref result.SuccessfulRequests);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Session operation failed: {ex.Message}");
                            Interlocked.Increment(ref result.FailedRequests);
                        }
                    }));
                }

                await Task.WhenAll(tasks);
                result.EndTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.EndTime = DateTime.UtcNow;
            }

            _results.Add(result);
            return result;
        }

        private async Task ExecuteContinuousUserWorkflow(int userId, int durationSeconds)
        {
            var endTime = DateTime.UtcNow.AddSeconds(durationSeconds);
            var random = new Random(userId);

            while (DateTime.UtcNow < endTime)
            {
                try
                {
                    // Simulate different user actions
                    var action = random.Next(0, 5);
                    switch (action)
                    {
                        case 0:
                            await _client.GetAsync("/");
                            break;
                        case 1:
                            await _client.GetAsync("/Incident/List");
                            break;
                        case 2:
                            await _client.GetAsync("/Volunteer/Tasks");
                            break;
                        case 3:
                            await _client.GetAsync("/Donation/All");
                            break;
                        case 4:
                            await SimulateUserRegistration(userId);
                            break;
                    }

                    // Random think time between actions
                    await Task.Delay(random.Next(500, 3000));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"User workflow {userId} failed: {ex.Message}");
                    throw;
                }
            }
        }

        private async Task ExecuteComplexDatabaseOperations(int operationId)
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Execute multiple complex queries in transaction
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                // Complex join query
                var incidentStats = await dbContext.DisasterIncidents
                    .Join(dbContext.Users,
                        incident => incident.UserId,
                        user => user.UserId,
                        (incident, user) => new { incident, user })
                    .Where(x => x.incident.Status == "Reported")
                    .GroupBy(x => x.user.UserType)
                    .Select(g => new
                    {
                        UserType = g.Key,
                        IncidentCount = g.Count(),
                        AvgPeopleAffected = g.Average(x => x.incident.PeopleAffected)
                    })
                    .ToListAsync();

                // Bulk insert simulation
                for (int i = 0; i < 100; i++)
                {
                    var donation = new Donation
                    {
                        DonationId = Guid.NewGuid(),
                        UserId = Guid.NewGuid(),
                        CategoryId = Guid.NewGuid(),
                        ItemName = $"Stress Test Item {operationId}-{i}",
                        Quantity = i,
                        DonationType = "Food",
                        Status = "Pending",
                        DonatedAt = DateTime.Now
                    };
                    dbContext.Donations.Add(donation);
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task ExecuteMemoryIntensiveOperations()
        {
            // Create large objects to stress memory
            var largeLists = new List<byte[]>();

            for (int i = 0; i < 100; i++)
            {
                largeLists.Add(new byte[1024 * 1024]); // 1MB each
                await Task.Delay(10);
            }

            // Simulate memory leak by not clearing lists immediately
            if (DateTime.UtcNow.Second % 10 != 0) // Only clear 10% of the time
            {
                await Task.Delay(100);
                largeLists.Clear();
            }
        }

        private async Task ExecuteCPUIntensiveOperations(int operationId, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // CPU-intensive calculations
                var calculations = new List<double>();
                for (int i = 0; i < 100000; i++)
                {
                    calculations.Add(Math.Sqrt(i) * Math.Pow(i, 1.5) / Math.Log(i + 1));
                }

                // Complex LINQ operations
                var result = calculations
                    .Where(x => x > 100)
                    .Select(x => new { Value = x, Squared = x * x })
                    .OrderByDescending(x => x.Squared)
                    .Take(1000)
                    .Sum(x => x.Value);

                await Task.Delay(100); // Small delay to prevent complete CPU lock
            }
        }

        private async Task<HttpResponseMessage> ExecuteRequestWithTimeout(string url, TimeSpan timeout)
        {
            var cts = new CancellationTokenSource(timeout);
            try
            {
                return await _client.GetAsync(url, cts.Token);
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
            }
        }

        private async Task ExecutePotentialDeadlockTransaction(int transactionId)
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                // Operations that could cause deadlocks
                var user = await dbContext.Users.FirstAsync();
                var incident = await dbContext.DisasterIncidents.FirstAsync();

                // Simulate concurrent updates
                user.LastLogin = DateTime.Now;
                incident.UpdatedAt = DateTime.Now;

                await dbContext.SaveChangesAsync();

                // Additional operations that might conflict
                var donations = await dbContext.Donations
                    .Where(d => d.Status == "Pending")
                    .OrderBy(d => d.DonatedAt)
                    .Take(10)
                    .ToListAsync();

                foreach (var donation in donations)
                {
                    donation.Status = "Processing";
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task ExecuteFileSystemOperations(string tempPath, int operationId)
        {
            var fileName = Path.Combine(tempPath, $"stress_test_{operationId}_{Guid.NewGuid()}.txt");

            // Create and write to file
            for (int i = 0; i < 100; i++)
            {
                await File.WriteAllTextAsync(fileName, new string('X', 1024 * 1024)); // 1MB file
                await Task.Delay(10);
            }

            // Read from file
            for (int i = 0; i < 50; i++)
            {
                var content = await File.ReadAllTextAsync(fileName);
                await Task.Delay(10);
            }

            // Clean up
            File.Delete(fileName);
        }

        private async Task MaintainUserSession(int sessionId)
        {
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };

            using var client = new HttpClient(handler);

            // Simulate user session by making sequential requests
            for (int i = 0; i < 10; i++)
            {
                await client.GetAsync($"http://localhost/Home/Index?session={sessionId}&step={i}");
                await Task.Delay(1000);
            }
        }

        private async Task SimulateUserRegistration(int userId)
        {
            var formData = new Dictionary<string, string>
            {
                ["Email"] = $"stress_test_{userId}_{DateTime.Now.Ticks}@example.com",
                ["Password"] = "StressTestPassword123!",
                ["FirstName"] = "Stress",
                ["LastName"] = $"Test{userId}",
                ["UserType"] = "Volunteer"
            };

            var content = new FormUrlEncodedContent(formData);
            await _client.PostAsync("/Account/Register", content);
        }

        private async Task<double> GetCurrentCpuUsage()
        {
            // Simplified CPU usage calculation
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

            await Task.Delay(1000);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;

            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            return cpuUsageTotal * 100;
        }
    }

    public class StressTestResult
    {
        public string TestName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public int TimeoutRequests { get; set; }
        public string ErrorMessage { get; set; }
        public List<long> MemoryUsageReadings { get; set; } = new List<long>();
        public List<double> CpuUsageReadings { get; set; } = new List<double>();
        public TimeSpan Duration => EndTime - StartTime;

        public void PrintResult()
        {
            Console.WriteLine($"\n=== {TestName} RESULTS ===");
            Console.WriteLine($"Duration: {Duration}");
            Console.WriteLine($"Successful: {SuccessfulRequests}");
            Console.WriteLine($"Failed: {FailedRequests}");
            Console.WriteLine($"Timeouts: {TimeoutRequests}");

            if (MemoryUsageReadings.Any())
            {
                Console.WriteLine($"Memory Usage: {MemoryUsageReadings.First() / 1024 / 1024}MB -> {MemoryUsageReadings.Last() / 1024 / 1024}MB");
            }

            if (CpuUsageReadings.Any())
            {
                Console.WriteLine($"Avg CPU Usage: {CpuUsageReadings.Average():F1}%");
                Console.WriteLine($"Max CPU Usage: {CpuUsageReadings.Max():F1}%");
            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                Console.WriteLine($"ERROR: {ErrorMessage}");
            }
            Console.WriteLine("========================\n");
        }
    }
}
