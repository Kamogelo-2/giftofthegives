using FluentAssertions;
using GiftOfTheGivers2.UITests.PageObjects;
using OpenQA.Selenium;
using Xunit;

namespace GiftOfTheGivers2.UITests.FunctionalTests
{
    public class UserRegistrationTests : IClassFixture<WebDriverFixture>
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl;
        private readonly HomePage _homePage;
        private readonly RegisterPage _registerPage;
        private readonly LoginPage _loginPage;

        public UserRegistrationTests(WebDriverFixture fixture)
        {
            _driver = fixture.Driver;
            _baseUrl = fixture.BaseUrl;
            _homePage = new HomePage(_driver, _baseUrl);
            _registerPage = new RegisterPage(_driver, _baseUrl);
            _loginPage = new LoginPage(_driver, _baseUrl);
        }

        [Fact]
        public void Register_ValidUser_ShouldRegisterSuccessfully()
        {
            // Arrange
            var timestamp = DateTime.Now.Ticks;
            var testEmail = $"testuser{timestamp}@example.com";

            // Act
            _homePage.NavigateTo();
            _homePage.ClickRegisterLink();

            _registerPage.FillRegistrationForm(
                firstName: "Test",
                lastName: "User",
                email: testEmail,
                password: "TestPassword123!",
                phoneNumber: "0123456789",
                address: "123 Test Street",
                userType: "Volunteer"
            );

            _registerPage.SubmitRegistration();

            // Assert
            _registerPage.IsRegistrationSuccessful().Should().BeTrue();
            _registerPage.GetSuccessMessage().Should().Contain("successfully");

            // Verify user can login
            _homePage.ClickLoginLink();
            _loginPage.FillLoginForm(testEmail, "TestPassword123!");
            _loginPage.SubmitLogin();

            _loginPage.IsLoginSuccessful().Should().BeTrue();
            _homePage.IsUserLoggedIn("Test User").Should().BeTrue();
        }

        [Theory]
        [InlineData("", "User", "test@example.com", "Password123", "First name is required")]
        [InlineData("Test", "", "test@example.com", "Password123", "Last name is required")]
        [InlineData("Test", "User", "invalid-email", "Password123", "Invalid email format")]
        [InlineData("Test", "User", "test@example.com", "weak", "Password must be strong")]
        public void Register_InvalidData_ShouldShowValidationErrors(string firstName, string lastName,
                                                                  string email, string password, string expectedError)
        {
            // Act
            _registerPage.NavigateTo();
            _registerPage.FillRegistrationForm(firstName, lastName, email, password);
            _registerPage.SubmitRegistration();

            // Assert
            _registerPage.HasValidationErrors().Should().BeTrue();
            var errors = _registerPage.GetErrorMessages();
            errors.Should().Contain(error => error.Contains(expectedError));
        }

        [Fact]
        public void Register_DuplicateEmail_ShouldShowError()
        {
            // Arrange
            var duplicateEmail = "duplicate@example.com";

            // Register first user
            _registerPage.NavigateTo();
            _registerPage.FillRegistrationForm("First", "User", duplicateEmail, "Password123!");
            _registerPage.SubmitRegistration();

            // Act - Try to register with same email
            _registerPage.NavigateTo();
            _registerPage.FillRegistrationForm("Second", "User", duplicateEmail, "Password123!");
            _registerPage.SubmitRegistration();

            // Assert
            _registerPage.HasValidationErrors().Should().BeTrue();
            _registerPage.GetErrorMessages().Should().Contain(error =>
                error.Contains("already exists", StringComparison.OrdinalIgnoreCase));
        }
    }
}

