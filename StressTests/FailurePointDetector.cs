using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using GiftOfTheGivers2.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers2.StressTests
{
    public class FailurePointDetector
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly List<FailurePoint> _detectedFailures;

        public FailurePointDetector()
        {
            _factory = new WebApplicationFactory<Program>();
            _detectedFailures = new List<FailurePoint>();
        }

        public async Task<List<FailurePoint>> DetectAllFailurePoints()
        {
            Console.WriteLine("Starting comprehensive failure point detection...");

            var detectionTasks = new List<Task>
            {
                DetectDatabaseConnectionLimits(),
                DetectMemoryLeaks(),
                DetectThreadPoolStarvation(),
                DetectConnectionPoolExhaustion(),
                DetectDeadlocks(),
                DetectFileHandleExhaustion(),
                DetectSessionStateIssues(),
                DetectConfigurationLimits()
            };

            await Task.WhenAll(detectionTasks);
            return _detectedFailures;
        }

        public async Task DetectDatabaseConnectionLimits()
        {
            try
            {
                Console.WriteLine("Testing database connection limits...");

                var connections = new List<ApplicationDbContext>();
                var maxConnections = 100; // Start with reasonable limit

                for (int i = 0; i < maxConnections; i++)
                {
                    try
                    {
                        var scope = _factory.Services.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        await dbContext.Users.FirstOrDefaultAsync(); // Test connection
                        connections.Add(dbContext);
                    }
                    catch (Exception ex) when (IsConnectionLimitException(ex))
                    {
                        _detectedFailures.Add(new FailurePoint
                        {
                            Type = FailureType.DatabaseConnectionLimit,
                            Description = $"Database connection limit reached at {i} connections",
                            Impact = "Application cannot serve additional database requests",
                            Recommendation = "Increase connection pool size or implement connection pooling optimization"
                        });
                        break;
                    }
                }

                // Cleanup
                foreach (var connection in connections)
                {
                    await connection.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection limit detection failed: {ex.Message}");
            }
        }

        public async Task DetectMemoryLeaks()
        {
            try
            {
                Console.WriteLine("Detecting potential memory leaks...");

                var startMemory = GC.GetTotalMemory(true);
                var memoryReadings = new List<long>();

                // Simulate prolonged operation
                for (int i = 0; i < 1000; i++)
                {
                    await ExecuteMemoryIntensiveOperation(i);

                    if (i % 100 == 0)
                    {
                        GC.Collect();
                        var currentMemory = GC.GetTotalMemory(true);
                        memoryReadings.Add(currentMemory);

                        // Check for consistent memory growth
                        if (memoryReadings.Count > 5)
                        {
                            var recentAverage = memoryReadings.TakeLast(5).Average();
                            var initialAverage = memoryReadings.Take(5).Average();

                            if (recentAverage > initialAverage * 1.5) // 50% growth
                            {
                                _detectedFailures.Add(new FailurePoint
                                {
                                    Type = FailureType.MemoryLeak,
                                    Description = $"Potential memory leak detected: Memory grew from {initialAverage / 1024 / 1024:F1}MB to {recentAverage / 1024 / 1024:F1}MB",
                                    Impact = "Application memory consumption grows over time, leading to out-of-memory exceptions",
                                    Recommendation = "Profile memory usage, identify objects not being garbage collected, implement using statements and proper disposal"
                                });
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Memory leak detection failed: {ex.Message}");
            }
        }

        public async Task DetectThreadPoolStarvation()
        {
            try
            {
                Console.WriteLine("Testing for thread pool starvation...");

                var tasks = new List<Task>();
                var completionTimes = new List<TimeSpan>();

                for (int i = 0; i < 1000; i++)
                {
                    var startTime = DateTime.UtcNow;
                    var task = Task.Run(async () =>
                    {
                        // Simulate blocking operation
                        Thread.Sleep(100);
                        await Task.Delay(100);
                    });

                    task = task.ContinueWith(t =>
                    {
                        completionTimes.Add(DateTime.UtcNow - startTime);
                    });

                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);

                var avgCompletionTime = completionTimes.Average(t => t.TotalMilliseconds);
                if (avgCompletionTime > 500) // If average completion > 500ms
                {
                    _detectedFailures.Add(new FailurePoint
                    {
                        Type = FailureType.ThreadPoolStarvation,
                        Description = $"Thread pool starvation detected: Average task completion time {avgCompletionTime:F0}ms",
                        Impact = "Slow request processing, timeouts, and reduced throughput",
                        Recommendation = "Use async/await properly, avoid blocking calls, consider increasing thread pool limits"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Thread pool starvation detection failed: {ex.Message}");
            }
        }

        public async Task DetectConnectionPoolExhaustion()
        {
            try
            {
                Console.WriteLine("Testing database connection pool exhaustion...");

                var tasks = new List<Task>();
                var exceptions = new List<Exception>();

                for (int i = 0; i < 200; i++) // More than typical connection pool size
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            using var scope = _factory.Services.CreateScope();
                            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                            // Complex query that holds connection
                            var result = await dbContext.DisasterIncidents
                                .FromSqlRaw("SELECT * FROM DisasterIncidents WITH (NOLOCK)")
                                .ToListAsync();

                            await Task.Delay(1000); // Hold connection
                        }
                        catch (Exception ex)
                        {
                            lock (exceptions)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }));
                }

                await Task.WhenAll(tasks);

                if (exceptions.Any(e => IsConnectionPoolException(e)))
                {
                    _detectedFailures.Add(new FailurePoint
                    {
                        Type = FailureType.ConnectionPoolExhaustion,
                        Description = "Database connection pool exhaustion detected",
                        Impact = "Database operations fail with timeout or connection limit errors",
                        Recommendation = "Optimize connection usage, implement connection pooling, review query performance"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection pool exhaustion detection failed: {ex.Message}");
            }
        }

        public async Task DetectDeadlocks()
        {
            try
            {
                Console.WriteLine("Testing for potential deadlocks...");

                var deadlockCount = 0;
                var tasks = new List<Task>();

                for (int i = 0; i < 50; i++)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await ExecutePotentialDeadlockScenario(i);
                        }
                        catch (Exception ex) when (IsDeadlockException(ex))
                        {
                            Interlocked.Increment(ref deadlockCount);
                        }
                    }));
                }

                await Task.WhenAll(tasks);

                if (deadlockCount > 0)
                {
                    _detectedFailures.Add(new FailurePoint
                    {
                        Type = FailureType.Deadlock,
                        Description = $"Potential deadlock scenarios detected: {deadlockCount} occurrences",
                        Impact = "Database transactions fail, data consistency issues",
                        Recommendation = "Review transaction isolation levels, implement retry logic, optimize query order"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deadlock detection failed: {ex.Message}");
            }
        }

        public async Task DetectFileHandleExhaustion()
        {
            try
            {
                Console.WriteLine("Testing file handle limits...");

                var files = new List<FileStream>();
                var maxHandles = 1000;

                for (int i = 0; i < maxHandles; i++)
                {
                    try
                    {
                        var tempFile = Path.GetTempFileName();
                        var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
                        files.Add(fileStream);
                    }
                    catch (IOException ex) when (ex.Message.Contains("too many open files"))
                    {
                        _detectedFailures.Add(new FailurePoint
                        {
                            Type = FailureType.FileHandleExhaustion,
                            Description = $"File handle limit reached at {i} open files",
                            Impact = "Cannot open additional files, file operations fail",
                            Recommendation = "Implement proper file disposal, use using statements, consider increasing system file handle limits"
                        });
                        break;
                    }
                }

                // Cleanup
                foreach (var file in files)
                {
                    file.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File handle exhaustion detection failed: {ex.Message}");
            }
        }

        public async Task DetectSessionStateIssues()
        {
            try
            {
                Console.WriteLine("Testing session state limitations...");

                var sessions = new List<Dictionary<string, object>>();
                var largeObject = new byte[1024 * 1024]; // 1MB object

                for (int i = 0; i < 1000; i++)
                {
                    try
                    {
                        var session = new Dictionary<string, object>
                        {
                            ["UserData"] = largeObject,
                            ["Preferences"] = largeObject,
                            ["TempData"] = largeObject
                        };
                        sessions.Add(session);

                        // Simulate session access
                        if (session.Count > 100)
                        {
                            session.Clear();
                        }
                    }
                    catch (Exception ex) when (IsMemoryOrSessionException(ex))
                    {
                        _detectedFailures.Add(new FailurePoint
                        {
                            Type = FailureType.SessionStateLimit,
                            Description = $"Session state limit reached with large session objects",
                            Impact = "Session data loss, authentication issues, poor performance",
                            Recommendation = "Optimize session size, use distributed caching, implement session cleanup"
                        });
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Session state issues detection failed: {ex.Message}");
            }
        }

        public async Task DetectConfigurationLimits()
        {
            try
            {
                Console.WriteLine("Testing configuration and limits...");

                // Test various configuration limits
                await TestMaxRequestSize();
                await TestTimeoutConfigurations();
                await TestConcurrentRequestLimits();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Configuration limits detection failed: {ex.Message}");
            }
        }

        private async Task TestMaxRequestSize()
        {
            try
            {
                var largeContent = new string('X', 50 * 1024 * 1024); // 50MB content
                var content = new StringContent(largeContent);

                using var client = _factory.CreateClient();
                var response = await client.PostAsync("/api/test", content);

                if (response.StatusCode == System.Net.HttpStatusCode.RequestEntityTooLarge)
                {
                    _detectedFailures.Add(new FailurePoint
                    {
                        Type = FailureType.RequestSizeLimit,
                        Description = "Request size limit exceeded",
                        Impact = "Large file uploads or data submissions fail",
                        Recommendation = "Increase MaxRequestBodySize, implement chunked uploads"
                    });
                }
            }
            catch (Exception ex)
            {
                // Expected to fail for large requests
            }
        }

        private async Task ExecuteMemoryIntensiveOperation(int iteration)
        {
            // Create objects that might not be properly disposed
            var lists = new List<byte[]>();
            for (int i = 0; i < 100; i++)
            {
                lists.Add(new byte[1024 * 10]); // 10KB each
            }

            // Simulate some objects not being disposed
            if (iteration % 3 != 0)
            {
                lists.Clear();
            }

            await Task.Delay(10);
        }

        private async Task ExecutePotentialDeadlockScenario(int scenarioId)
        {
            using var scope1 = _factory.Services.CreateScope();
            using var scope2 = _factory.Services.CreateScope();

            var dbContext1 = scope1.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dbContext2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var task1 = Task.Run(async () =>
            {
                using var transaction1 = await dbContext1.Database.BeginTransactionAsync();
                var user = await dbContext1.Users.FirstAsync();
                user.LastLogin = DateTime.Now;
                await dbContext1.SaveChangesAsync();

                // Delay to increase deadlock probability
                await Task.Delay(100);

                var incident = await dbContext2.DisasterIncidents.FirstAsync();
                incident.UpdatedAt = DateTime.Now;
                await dbContext2.SaveChangesAsync();

                await transaction1.CommitAsync();
            });

            var task2 = Task.Run(async () =>
            {
                using var transaction2 = await dbContext2.Database.BeginTransactionAsync();
                var incident = await dbContext2.DisasterIncidents.FirstAsync();
                incident.UpdatedAt = DateTime.Now;
                await dbContext2.SaveChangesAsync();

                // Delay to increase deadlock probability
                await Task.Delay(100);

                var user = await dbContext1.Users.FirstAsync();
                user.LastLogin = DateTime.Now;
                await dbContext1.SaveChangesAsync();

                await transaction2.CommitAsync();
            });

            await Task.WhenAll(task1, task2);
        }

        private bool IsConnectionLimitException(Exception ex)
        {
            return ex.Message.Contains("connection limit", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("pool", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsConnectionPoolException(Exception ex)
        {
            return ex.Message.Contains("pool", StringComparison.OrdinalIgnoreCase) &&
                   ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsDeadlockException(Exception ex)
        {
            return ex.Message.Contains("deadlock", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("lock", StringComparison.OrdinalIgnoreCase) ||
                   (ex.InnerException?.Message.Contains("deadlock", StringComparison.OrdinalIgnoreCase) == true);
        }

        private bool IsMemoryOrSessionException(Exception ex)
        {
            return ex is OutOfMemoryException ||
                   ex.Message.Contains("memory", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("session", StringComparison.OrdinalIgnoreCase);
        }

        private async Task TestTimeoutConfigurations()
        {
            try
            {
                using var client = _factory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(1); // Very short timeout

                var response = await client.GetAsync("/Home/Index");
            }
            catch (TaskCanceledException)
            {
                _detectedFailures.Add(new FailurePoint
                {
                    Type = FailureType.Timeout,
                    Description = "Request timeout under high load",
                    Impact = "Users experience timeouts, poor user experience",
                    Recommendation = "Optimize slow endpoints, implement proper timeout handling, use async operations"
                });
            }
        }

        private async Task TestConcurrentRequestLimits()
        {
            var tasks = new List<Task>();
            var errorCount = 0;

            for (int i = 0; i < 1000; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using var client = _factory.CreateClient();
                        await client.GetAsync("/Home/Index");
                    }
                    catch (Exception)
                    {
                        Interlocked.Increment(ref errorCount);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            if (errorCount > 100) // More than 10% errors
            {
                _detectedFailures.Add(new FailurePoint
                {
                    Type = FailureType.ConcurrentRequestLimit,
                    Description = $"High error rate ({errorCount}/1000) under concurrent load",
                    Impact = "Service becomes unresponsive under high traffic",
                    Recommendation = "Scale horizontally, optimize resource usage, implement rate limiting"
                });
            }
        }
    }

    public class FailurePoint
    {
        public FailureType Type { get; set; }
        public string Description { get; set; }
        public string Impact { get; set; }
        public string Recommendation { get; set; }
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

        public void PrintFailurePoint()
        {
            Console.WriteLine($"\n⚠️ FAILURE POINT DETECTED:");
            Console.WriteLine($"Type: {Type}");
            Console.WriteLine($"Description: {Description}");
            Console.WriteLine($"Impact: {Impact}");
            Console.WriteLine($"Recommendation: {Recommendation}");
            Console.WriteLine($"Detected: {DetectedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();
        }
    }

    public enum FailureType
    {
        DatabaseConnectionLimit,
        MemoryLeak,
        ThreadPoolStarvation,
        ConnectionPoolExhaustion,
        Deadlock,
        FileHandleExhaustion,
        SessionStateLimit,
        RequestSizeLimit,
        Timeout,
        ConcurrentRequestLimit,
        ConfigurationLimit
    }
}

