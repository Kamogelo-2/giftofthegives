namespace GiftOfTheGivers2.loadtest
{
    public class LoadTestRunner
    {
        private readonly LoadTestScenarios _scenarios;
        private readonly PerformanceMetrics _metrics;

        public LoadTestRunner()
        {
            _scenarios = new LoadTestScenarios();
            _metrics = new PerformanceMetrics(new HttpClient());
        }

        public async Task RunComprehensiveLoadTest()
        {
            Console.WriteLine("Starting Comprehensive Load Test for Gift of the Givers Application");
            Console.WriteLine("==================================================================");

            var testResults = new List<bool>();

            // Test 1: Smoke Test (Light Load)
            Console.WriteLine("\n1. Running Smoke Test (10 concurrent users, 30 seconds)...");
            testResults.Add(await _scenarios.HomePageLoadTest(10, 30));

            // Test 2: Normal Load Test
            Console.WriteLine("\n2. Running Normal Load Test (50 concurrent users, 60 seconds)...");
            testResults.Add(await _scenarios.UserRegistrationLoadTest(50, 60));

            // Test 3: Stress Test
            Console.WriteLine("\n3. Running Stress Test (100 concurrent users, 120 seconds)...");
            testResults.Add(await _scenarios.DisasterIncidentWorkflowTest(100, 120));

            // Test 4: Database Stress Test
            Console.WriteLine("\n4. Running Database Stress Test (1000 operations)...");
            testResults.Add(await _scenarios.DatabaseStressTest(1000));

            // Test 5: Spike Test
            Console.WriteLine("\n5. Running Spike Test (200 concurrent users, 30 seconds)...");
            testResults.Add(await _scenarios.HomePageLoadTest(200, 30));

            // Generate final report
            Console.WriteLine("\n=== LOAD TEST SUMMARY ===");
            for (int i = 0; i < testResults.Count; i++)
            {
                Console.WriteLine($"Test {i + 1}: {(testResults[i] ? "PASSED" : "FAILED")}");
            }

            var overallResult = testResults.All(r => r);
            Console.WriteLine($"\nOverall Result: {(overallResult ? "ALL TESTS PASSED" : "SOME TESTS FAILED")}");

            if (overallResult)
            {
                Console.WriteLine("Application meets performance requirements!");
            }
            else
            {
                Console.WriteLine("Application requires performance optimization!");
            }
        }

        public async Task RunSpecificScenario(string scenarioName, int users, int duration)
        {
            Console.WriteLine($"Running {scenarioName} with {users} users for {duration} seconds...");

            bool result = scenarioName.ToLower() switch
            {
                "homepage" => await _scenarios.HomePageLoadTest(users, duration),
                "registration" => await _scenarios.UserRegistrationLoadTest(users, duration),
                "workflow" => await _scenarios.DisasterIncidentWorkflowTest(users, duration),
                "database" => await _scenarios.DatabaseStressTest(users),
                _ => throw new ArgumentException($"Unknown scenario: {scenarioName}")
            };

            Console.WriteLine($"{scenarioName} Test: {(result ? "PASSED" : "FAILED")}");
        }
    }
}
