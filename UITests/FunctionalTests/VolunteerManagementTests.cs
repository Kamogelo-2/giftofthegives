using FluentAssertions;
using GiftOfTheGivers2.UITests.PageObjects;
using OpenQA.Selenium;
using Xunit;

namespace GiftOfTheGivers2.UITests.FunctionalTests
{
    public class VolunteerManagementTests : IClassFixture<WebDriverFixture>
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl;
        private readonly VolunteerTasksPage _volunteerTasksPage;
        private readonly HomePage _homePage;
        private readonly LoginPage _loginPage;

        public VolunteerManagementTests(WebDriverFixture fixture)
        {
            _driver = fixture.Driver;
            _baseUrl = fixture.BaseUrl;
            _volunteerTasksPage = new VolunteerTasksPage(_driver, _baseUrl);
            _homePage = new HomePage(_driver, _baseUrl);
            _loginPage = new LoginPage(_driver, _baseUrl);
        }

        [Fact]
        public void BrowseVolunteerTasks_ShouldDisplayAvailableTasks()
        {
            // Act
            _volunteerTasksPage.NavigateTo();
            var taskCount = _volunteerTasksPage.GetAvailableTasksCount();
            var taskTitles = _volunteerTasksPage.GetTaskTitles();

            // Assert
            taskCount.Should().BeGreaterThan(0);
            taskTitles.Should().NotBeEmpty();
            taskTitles.All(title => !string.IsNullOrWhiteSpace(title)).Should().BeTrue();
        }

        [Fact]
        public void JoinVolunteerTask_ShouldAssignSuccessfully()
        {
            // Arrange - Login first
            LoginAsTestUser();

            // Act
            _volunteerTasksPage.NavigateTo();
            _volunteerTasksPage.JoinTask(0); // Join first available task

            // Assert
            _volunteerTasksPage.IsJoinSuccessful().Should().BeTrue();
            _volunteerTasksPage.GetSuccessMessage().Should().Contain("joined");
        }

        [Fact]
        public void ViewTaskDetails_ShouldDisplayTaskInformation()
        {
            // Act
            _volunteerTasksPage.NavigateTo();
            _volunteerTasksPage.ClickTaskDetails(0); // Click first task details

            // Assert - Should be on task details page
            _driver.Url.Should().Contain("/Volunteer/TaskDetails");
            _driver.FindElements(By.CssSelector(".task-details")).Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Navigation_VolunteerSection_ShouldWorkCorrectly()
        {
            // Arrange - Login first
            LoginAsTestUser();

            // Act & Assert - Navigate through volunteer section
            _homePage.NavigateTo();
            _homePage.ClickVolunteerTasksLink();
            _volunteerTasksPage.AssertPageIsLoaded();

            _volunteerTasksPage.NavigateToMyAssignments();
            _driver.Url.Should().Contain("/Volunteer/MyAssignments");
        }

        private void LoginAsTestUser()
        {
            _loginPage.NavigateTo();
            _loginPage.FillLoginForm("test@example.com", "TestPassword123!");
            _loginPage.SubmitLogin();
        }
    }
}
