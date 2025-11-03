using FluentAssertions;
using GiftOfTheGivers2.UITests.PageObjects;
using OpenQA.Selenium;
using Xunit;

namespace GiftOfTheGivers2.UITests.FunctionalTests
{
    public class ErrorHandlingTests : IClassFixture<WebDriverFixture>
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl;
        private readonly HomePage _homePage;
        private readonly LoginPage _loginPage;

        public ErrorHandlingTests(WebDriverFixture fixture)
        {
            _driver = fixture.Driver;
            _baseUrl = fixture.BaseUrl;
            _homePage = new HomePage(_driver, _baseUrl);
            _loginPage = new LoginPage(_driver, _baseUrl);
        }

        [Fact]
        public void Login_InvalidCredentials_ShouldShowErrorMessage()
        {
            // Act
            _loginPage.NavigateTo();
            _loginPage.FillLoginForm("nonexistent@example.com", "WrongPassword");
            _loginPage.SubmitLogin();

            // Assert
            _loginPage.IsLoginFailed().Should().BeTrue();
            _loginPage.GetErrorMessage().Should().Contain("Invalid login attempt");
        }

        [Fact]
        public void Forms_ValidationErrors_ShouldDisplayProperly()
        {
            // Act - Try to submit empty registration form
            var registerPage = new RegisterPage(_driver, _baseUrl);
            registerPage.NavigateTo();
            registerPage.SubmitRegistration(); // Submit without filling data

            // Assert
            registerPage.HasValidationErrors().Should().BeTrue();
            var errors = registerPage.GetErrorMessages();
            errors.Should().Contain(error => error.Contains("required", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Session_Timeout_ShouldRedirectToLogin()
        {
            // This test would require simulating session timeout
            // For now, we test that protected pages redirect to login when not authenticated

            // Act - Try to access protected page without login
            _driver.Navigate().GoToUrl($"{_baseUrl}/Donation/MyDonations");

            // Assert - Should redirect to login
            _driver.Url.Should().Contain("/Account/Login");
        }

        [Fact]
        public void ErrorMessages_ShouldBeUserFriendly()
        {
            // Test that error messages are clear and helpful
            _loginPage.NavigateTo();
            _loginPage.FillLoginForm("invalid-email-format", "password");
            _loginPage.SubmitLogin();

            var errorMessage = _loginPage.GetErrorMessage();
            errorMessage.Should().NotBeNullOrEmpty();
            errorMessage.Should().NotContain("exception", StringComparison.OrdinalIgnoreCase);
            errorMessage.Should().NotContain("stack trace", StringComparison.OrdinalIgnoreCase);
        }

    }
}
