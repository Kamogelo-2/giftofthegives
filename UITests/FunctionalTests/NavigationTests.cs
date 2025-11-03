using FluentAssertions;
using GiftOfTheGivers2.UITests.PageObjects;
using OpenQA.Selenium;
using Xunit;

namespace GiftOfTheGivers2.UITests.FunctionalTests
{
    public class NavigationTests : IClassFixture<WebDriverFixture>
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl;
        private readonly HomePage _homePage;

        public NavigationTests(WebDriverFixture fixture)
        {
            _driver = fixture.Driver;
            _baseUrl = fixture.BaseUrl;
            _homePage = new HomePage(_driver, _baseUrl);
        }

        [Fact]
        public void Navigation_MainMenu_ShouldWorkCorrectly()
        {
            // Act & Assert - Test all main navigation links
            _homePage.NavigateTo();

            // Test Home link
            _homePage.ClickIncidentListLink();
            _driver.Url.Should().Contain("/Incident/List");
            _homePage.NavigateTo();

            // Test Volunteer link
            _homePage.ClickVolunteerTasksLink();
            _driver.Url.Should().Contain("/Volunteer/Tasks");
            _homePage.NavigateTo();

            // Test Donate link
            _homePage.ClickDonateLink();
            _driver.Url.Should().Contain("/Donation/Donate");
            _homePage.NavigateTo();
        }

        [Fact]
        public void Navigation_Breadcrumbs_ShouldWorkCorrectly()
        {
            // Arrange
            _homePage.NavigateTo();

            // Act - Navigate to incident list
            _homePage.ClickIncidentListLink();

            // Assert - Should be able to navigate back to home
            _driver.FindElement(By.CssSelector("a[href='/']")).Click();
            _driver.Url.Should().Be(_baseUrl + "/");
        }

        [Fact]
        public void Navigation_FooterLinks_ShouldWorkCorrectly()
        {
            // Arrange
            _homePage.NavigateTo();

            // Act - Scroll to footer and click links
            var footerLinks = _driver.FindElements(By.CssSelector("footer a"));

            // Assert
            footerLinks.Should().NotBeEmpty();

            // Test each footer link
            foreach (var link in footerLinks.Take(3)) // Test first 3 links
            {
                var href = link.GetAttribute("href");
                if (!string.IsNullOrEmpty(href) && href.StartsWith(_baseUrl))
                {
                    link.Click();
                    _driver.Url.Should().Be(href);
                    _homePage.NavigateTo(); // Return to home
                }
            }
        }

        [Fact]
        public void Navigation_InvalidUrl_ShouldShowErrorPage()
        {
            // Act - Navigate to non-existent page
            _driver.Navigate().GoToUrl($"{_baseUrl}/NonExistentPage");

            // Assert - Should show error page or redirect
            var pageContent = _driver.PageSource;
            pageContent.Should().ContainAny("Error", "NotFound", "404", "Oops");
        }
    }
}
