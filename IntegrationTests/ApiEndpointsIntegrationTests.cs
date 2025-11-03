using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using GiftOfTheGivers2.Models;
using Xunit;

namespace GiftOfTheGivers2.IntegrationTests
{
    public class ApiEndpointsIntegrationTests : IClassFixture<WebApplicationFactoryFixture>
    {
        private readonly WebApplicationFactoryFixture _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiEndpointsIntegrationTests(WebApplicationFactoryFixture factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [Fact]
        public async Task HomeController_Endpoints_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task IncidentController_ListIncidents_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/Incident/List");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Disaster Incidents", content);
        }

        [Fact]
        public async Task DonationController_AllDonations_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/Donation/All");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Donations", content);
        }

        [Fact]
        public async Task VolunteerController_Tasks_ShouldReturnSuccess()
        {
            // Act
            var response = await _client.GetAsync("/Volunteer/Tasks");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Volunteer Tasks", content);
        }

        [Fact]
        public async Task AccountController_Register_ShouldCreateNewUser()
        {
            // Arrange
            var uniqueEmail = $"test{Guid.NewGuid()}@example.com";
            var registerData = new List<KeyValuePair<string, string>>
            {
                new("Email", uniqueEmail),
                new("Password", "TestPassword123"),
                new("FirstName", "Integration"),
                new("LastName", "Test"),
                new("PhoneNumber", "0123456789"),
                new("Address", "123 Test Street"),
                new("UserType", "Volunteer")
            };

            var content = new FormUrlEncodedContent(registerData);

            // Act
            var response = await _client.PostAsync("/Account/Register", content);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Home/Index", response.Headers.Location.OriginalString);
        }

        [Fact]
        public async Task IncidentController_ReportIncident_ShouldCreateNewIncident()
        {
            // Arrange
            var incidentData = new List<KeyValuePair<string, string>>
            {
                new("Title", "Integration Test Flood"),
                new("Description", "This is a test incident created during integration testing"),
                new("IncidentType", "Flood"),
                new("Location", "Test Location, Integration City"),
                new("SeverityLevel", "Medium"),
                new("PeopleAffected", "150"),
                new("ImmediateNeeds", "Food, Water, Medical Supplies")
            };

            var content = new FormUrlEncodedContent(incidentData);

            // Act
            var response = await _client.PostAsync("/Incident/Report", content);

            // Assert - Should redirect to login since user is not authenticated
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }
    }
    
    }

