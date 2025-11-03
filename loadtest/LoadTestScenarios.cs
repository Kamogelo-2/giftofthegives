using GiftOfTheGivers2.Data;
using GiftOfTheGivers2.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;

namespace GiftOfTheGivers2.loadtest
{
    public class LoadTestScenarios
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ApplicationDbContext _dbContext;

        public LoadTestScenarios()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
            _dbContext = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        public async Task<bool> HomePageLoadTest(int concurrentUsers, int durationSeconds)
        {
            var tasks = new List<Task<HttpResponseMessage>>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(durationSeconds));

            for (int i = 0; i < concurrentUsers; i++)
            {
                tasks.Add(ExecuteHomePageRequest(cts.Token));
            }

            try
            {
                var results = await Task.WhenAll(tasks);
                var successCount = results.Count(r => r.IsSuccessStatusCode);

                Console.WriteLine($"Home Page Load Test: {successCount}/{concurrentUsers} successful requests");
                return successCount >= concurrentUsers * 0.95; // 95% success rate
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Home Page Load Test failed: {ex.Message}");
                return false;
            }
        }

        private async Task<HttpResponseMessage> ExecuteHomePageRequest(CancellationToken cancellationToken)
        {
            try
            {
                return await _client.GetAsync("/", cancellationToken);
            }
            catch (TaskCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
            }
        }

        public async Task<bool> UserRegistrationLoadTest(int concurrentUsers, int durationSeconds)
        {
            var tasks = new List<Task<bool>>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(durationSeconds));

            for (int i = 0; i < concurrentUsers; i++)
            {
                tasks.Add(ExecuteUserRegistrationScenario(i, cts.Token));
            }

            try
            {
                var results = await Task.WhenAll(tasks);
                var successCount = results.Count(r => r);

                Console.WriteLine($"User Registration Load Test: {successCount}/{concurrentUsers} successful registrations");
                return successCount >= concurrentUsers * 0.90; // 90% success rate
            }
            catch (Exception ex)
            {
                Console.WriteLine($"User Registration Load Test failed: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> ExecuteUserRegistrationScenario(int userId, CancellationToken cancellationToken)
        {
            try
            {
                // Step 1: Get registration page
                var getResponse = await _client.GetAsync("/Account/Register", cancellationToken);
                if (!getResponse.IsSuccessStatusCode)
                    return false;

                // Step 2: Submit registration form
                var formData = new Dictionary<string, string>
                {
                    ["Email"] = $"loadtestuser{userId}_{DateTime.Now.Ticks}@example.com",
                    ["Password"] = "LoadTestPassword123!",
                    ["FirstName"] = "Load",
                    ["LastName"] = $"Test{userId}",
                    ["PhoneNumber"] = "0123456789",
                    ["Address"] = "123 Load Test Street",
                    ["UserType"] = "Volunteer"
                };

                var content = new FormUrlEncodedContent(formData);
                var postResponse = await _client.PostAsync("/Account/Register", content, cancellationToken);

                return postResponse.IsSuccessStatusCode || postResponse.StatusCode == HttpStatusCode.Redirect;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }

        public async Task<bool> DisasterIncidentWorkflowTest(int concurrentUsers, int durationSeconds)
        {
            var tasks = new List<Task<bool>>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(durationSeconds));

            for (int i = 0; i < concurrentUsers; i++)
            {
                tasks.Add(ExecuteDisasterIncidentWorkflow(i, cts.Token));
            }

            try
            {
                var results = await Task.WhenAll(tasks);
                var successCount = results.Count(r => r);

                Console.WriteLine($"Disaster Incident Workflow Test: {successCount}/{concurrentUsers} successful workflows");
                return successCount >= concurrentUsers * 0.85; // 85% success rate
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Disaster Incident Workflow Test failed: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> ExecuteDisasterIncidentWorkflow(int workflowId, CancellationToken cancellationToken)
        {
            try
            {
                // Simulate browsing incidents
                var incidentsResponse = await _client.GetAsync("/Incident/List", cancellationToken);
                if (!incidentsResponse.IsSuccessStatusCode)
                    return false;

                // Simulate viewing volunteer tasks
                var tasksResponse = await _client.GetAsync("/Volunteer/Tasks", cancellationToken);
                if (!tasksResponse.IsSuccessStatusCode)
                    return false;

                // Simulate viewing donations
                var donationsResponse = await _client.GetAsync("/Donation/All", cancellationToken);

                return donationsResponse.IsSuccessStatusCode;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }

        public async Task<bool> DatabaseStressTest(int operationsCount)
        {
            var tasks = new List<Task>();
            var random = new Random();

            for (int i = 0; i < operationsCount; i++)
            {
                tasks.Add(ExecuteDatabaseOperation(i, random));
            }

            try
            {
                await Task.WhenAll(tasks);
                Console.WriteLine($"Database Stress Test: {operationsCount} operations completed");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Stress Test failed: {ex.Message}");
                return false;
            }
        }

        private async Task ExecuteDatabaseOperation(int operationId, Random random)
        {
            try
            {
                // Simulate different database operations
                var operationType = random.Next(0, 4);

                switch (operationType)
                {
                    case 0: // Read incidents
                        var incidents = await _dbContext.DisasterIncidents.Take(10).ToListAsync();
                        break;
                    case 1: // Read donations
                        var donations = await _dbContext.Donations.Take(10).ToListAsync();
                        break;
                    case 2: // Read volunteer tasks
                        var tasks = await _dbContext.VolunteerTasks.Take(10).ToListAsync();
                        break;
                    case 3: // Complex query
                        var stats = await _dbContext.DisasterIncidents
                            .GroupBy(i => i.Status)
                            .Select(g => new { Status = g.Key, Count = g.Count() })
                            .ToListAsync();
                        break;
                }

                // Small delay to simulate processing
                await Task.Delay(random.Next(10, 100));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database operation {operationId} failed: {ex.Message}");
            }
        }
    }
}

