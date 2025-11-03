namespace GiftOfTheGivers2.StressTests
{
    public class StressTestRunner
    {
        private readonly ExtremeStressScenarios _scenarios;
        private readonly FailurePointDetector _detector;
        private readonly ResourceMonitor _monitor;

        public StressTestRunner()
        {
            _scenarios = new ExtremeStressScenarios();
            _detector = new FailurePointDetector();
            _monitor = new ResourceMonitor("ComprehensiveStressTest");
        }

        public async Task RunComprehensiveStressTest()
        {
            Console.WriteLine("🚀 STARTING COMPREHENSIVE STRESS TEST SUITE");
            Console.WriteLine("===========================================\n");

            var testResults = new List<StressTestResult>();
            var failurePoints = new List<FailurePoint>();

            try
            {
                // Phase 1: Failure Point Detection
                Console.WriteLine("PHASE 1: FAILURE POINT DETECTION");
                Console.WriteLine("---------------------------------");
                failurePoints = await _detector.DetectAllFailurePoints();

                // Phase 2: Extreme Stress Scenarios
                Console.WriteLine("\nPHASE 2: EXTREME STRESS SCENARIOS");
                Console.WriteLine("----------------------------------");

                // Test 1: Sudden Traffic Spike
                Console.WriteLine("\n1. Sudden Traffic Spike Test...");
                testResults.Add(await _scenarios.SuddenTrafficSpikeTest(100, 1000, 120));

                // Test 2: Database Connection Stress
                Console.WriteLine("\n2. Database Connection Stress Test...");
                testResults.Add(await _scenarios.DatabaseConnectionPoolExhaustionTest(500));

                // Test 3: Memory Leak Detection
                Console.WriteLine("\n3. Memory Leak Stress Test...");
                testResults.Add(await _scenarios.MemoryLeakStressTest(5)); // 5 minutes

                // Test 4: CPU Stress
                Console.WriteLine("\n4. CPU Stress Test...");
                testResults.Add(await _scenarios.CPUStressTest(50, 60));

                // Test 5: Network Latency
                Console.WriteLine("\n5. Network Latency Stress Test...");
                testResults.Add(await _scenarios.NetworkLatencyStressTest(200, 5000));

                // Test 6: Database Deadlocks
                Console.WriteLine("\n6. Database Deadlock Test...");
                testResults.Add(await _scenarios.DatabaseDeadlockTest(100));

                // Test 7: File System Stress
                Console.WriteLine("\n7. File System Stress Test...");
                testResults.Add(await _scenarios.FileSystemStressTest(200));

                // Test 8: Session State Stress
                Console.WriteLine("\n8. Session State Stress Test...");
                testResults.Add(await _scenarios.SessionStateStressTest(500));

                // Generate Final Report
                Console.WriteLine("\n📊 STRESS TEST SUMMARY");
                Console.WriteLine("======================");
                PrintTestSummary(testResults);
                PrintFailureSummary(failurePoints);
                PrintRecommendations(testResults, failurePoints);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Stress test suite failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                _monitor.Dispose();
            }
        }

        public async Task RunSpecificStressTest(string testName, params object[] parameters)
        {
            Console.WriteLine($"Running specific stress test: {testName}");

            try
            {
                var result = testName.ToLower() switch
                {
                    "traffic-spike" => await _scenarios.SuddenTrafficSpikeTest(
                        (int)parameters[0], (int)parameters[1], (int)parameters[2]),
                    "database-connections" => await _scenarios.DatabaseConnectionPoolExhaustionTest(
                        (int)parameters[0]),
                    "memory-leak" => await _scenarios.MemoryLeakStressTest(
                        (int)parameters[0]),
                    "cpu-stress" => await _scenarios.CPUStressTest(
                        (int)parameters[0], (int)parameters[1]),
                    "network-latency" => await _scenarios.NetworkLatencyStressTest(
                        (int)parameters[0], (int)parameters[1]),
                    "deadlocks" => await _scenarios.DatabaseDeadlockTest(
                        (int)parameters[0]),
                    "file-system" => await _scenarios.FileSystemStressTest(
                        (int)parameters[0]),
                    "session-state" => await _scenarios.SessionStateStressTest(
                        (int)parameters[0]),
                    _ => throw new ArgumentException($"Unknown test: {testName}")
                };

                result.PrintResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Stress test {testName} failed: {ex.Message}");
            }
        }

        private void PrintTestSummary(List<StressTestResult> results)
        {
            Console.WriteLine("\n📈 TEST RESULTS SUMMARY");
            Console.WriteLine("-----------------------");

            foreach (var result in results)
            {
                var status = string.IsNullOrEmpty(result.ErrorMessage) ? "✅ PASSED" : "❌ FAILED";
                Console.WriteLine($"{result.TestName}: {status}");
                Console.WriteLine($"   Duration: {result.Duration}");
                Console.WriteLine($"   Success: {result.SuccessfulRequests}, Failed: {result.FailedRequests}");

                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    Console.WriteLine($"   Error: {result.ErrorMessage}");
                }
            }

            var totalTests = results.Count;
            var passedTests = results.Count(r => string.IsNullOrEmpty(r.ErrorMessage));
            var failureRate = (double)(totalTests - passedTests) / totalTests * 100;

            Console.WriteLine($"\nOverall: {passedTests}/{totalTests} tests passed ({failureRate:F1}% failure rate)");
        }

        private void PrintFailureSummary(List<FailurePoint> failurePoints)
        {
            if (!failurePoints.Any())
            {
                Console.WriteLine("\n🎉 No critical failure points detected!");
                return;
            }

            Console.WriteLine($"\n⚠️ DETECTED FAILURE POINTS: {failurePoints.Count}");
            Console.WriteLine("------------------------------");

            var groupedFailures = failurePoints.GroupBy(f => f.Type);

            foreach (var group in groupedFailures)
            {
                Console.WriteLine($"\n{group.Key}: {group.Count()} occurrences");
                foreach (var failure in group.Take(3)) // Show first 3 of each type
                {
                    Console.WriteLine($"  - {failure.Description}");
                }
            }
        }

        private void PrintRecommendations(List<StressTestResult> testResults, List<FailurePoint> failurePoints)
        {
            Console.WriteLine("\n💡 RECOMMENDATIONS FOR IMPROVEMENT");
            Console.WriteLine("----------------------------------");

            var recommendations = new HashSet<string>();

            // Add recommendations from failure points
            foreach (var failure in failurePoints)
            {
                recommendations.Add(failure.Recommendation);
            }

            // Add recommendations from test results
            foreach (var result in testResults.Where(r => !string.IsNullOrEmpty(r.ErrorMessage)))
            {
                if (result.TestName.Contains("Memory"))
                {
                    recommendations.Add("Implement memory profiling and monitoring");
                    recommendations.Add("Review object disposal patterns");
                }
                else if (result.TestName.Contains("Database"))
                {
                    recommendations.Add("Optimize database queries and indexes");
                    recommendations.Add("Implement connection pooling best practices");
                }
                else if (result.TestName.Contains("CPU"))
                {
                    recommendations.Add("Optimize CPU-intensive operations");
                    recommendations.Add("Consider implementing background processing");
                }
            }

            // Print recommendations
            int counter = 1;
            foreach (var recommendation in recommendations)
            {
                Console.WriteLine($"{counter++}. {recommendation}");
            }

            if (!recommendations.Any())
            {
                Console.WriteLine("No specific recommendations - application handled stress well!");
            }
        }
    }
}

