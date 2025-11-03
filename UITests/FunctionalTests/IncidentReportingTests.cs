using FluentAssertions;
using GiftOfTheGivers2.UITests.PageObjects;
using OpenQA.Selenium;
using Xunit;

namespace GiftOfTheGivers2.UITests.FunctionalTests
{
    public class IncidentReportingTests : IClassFixture<WebDriverFixture>
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl;
        private readonly HomePage _homePage;
        private readonly IncidentReportPage _incidentReportPage;
        private readonly LoginPage _loginPage;

        public IncidentReportingTests(WebDriverFixture fixture)
        {
            _driver = fixture.Driver;
            _baseUrl = fixture.BaseUrl;
            _homePage = new HomePage(_driver, _baseUrl);
            _incidentReportPage = new IncidentReportPage(_driver, _baseUrl);
            _loginPage = new LoginPage(_driver, _baseUrl);
        }

        [Fact]
        public void ReportIncident_ValidData_ShouldReportSuccessfully()
        {
            // Arrange - Login first
            LoginAsTestUser();

            // Act
            _incidentReportPage.NavigateTo();
            _incidentReportPage.FillIncidentForm(
                title: "Test Flood Incident - UI Test",
                description: "This is a test incident reported through UI automation",
                incidentType: "Flood",
                location: "Durban, South Africa",
                severityLevel: "High",
                peopleAffected: 150,
                immediateNeeds: "Food, Water, Shelter"
            );

            _incidentReportPage.SubmitIncidentReport();

            // Assert
            _incidentReportPage.IsReportSuccessful().Should().BeTrue();
            _incidentReportPage.GetSuccessMessage().Should().Contain("successfully");
        }

        [Fact]
        public void ReportIncident_WithoutLogin_ShouldRedirectToLogin()
        {
            // Act - Try to report incident without logging in
            _incidentReportPage.NavigateTo();

            // Assert - Should be redirected to login page
            _loginPage.AssertPageIsLoaded();
            _driver.Url.Should().Contain("/Account/Login");
        }

        [Theory]
        [InlineData("", "Description", "Flood", "Location", "Title is required")]
        [InlineData("Title", "", "Flood", "Location", "Description is required")]
        [InlineData("Title", "Description", "", "Location", "Incident type is required")]
        [InlineData("Title", "Description", "Flood", "", "Location is required")]
        public void ReportIncident_InvalidData_ShouldShowValidationErrors(string title, string description,
                                                                        string incidentType, string location,
                                                                        string expectedError)
        {
            // Arrange - Login first
            LoginAsTestUser();

            // Act
            _incidentReportPage.NavigateTo();
            _incidentReportPage.FillIncidentForm(title, description, incidentType, location);
            _incidentReportPage.SubmitIncidentReport();

            // Assert
            _incidentReportPage.HasValidationErrors().Should().BeTrue();
        }

        private void LoginAsTestUser()
        {
            _loginPage.NavigateTo();
            _loginPage.FillLoginForm("test@example.com", "TestPassword123!");
            _loginPage.SubmitLogin();
            _loginPage.IsLoginSuccessful().Should().BeTrue();
        }
    }
}
