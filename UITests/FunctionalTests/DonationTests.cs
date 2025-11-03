using FluentAssertions;
using GiftOfTheGivers2.UITests.PageObjects;
using OpenQA.Selenium;
using Xunit;

namespace GiftOfTheGivers2.UITests.FunctionalTests
{
    public class DonationTests : IClassFixture<WebDriverFixture>
    {
        private readonly IWebDriver _driver;
        private readonly string _baseUrl;
        private readonly DonationPage _donationPage;
        private readonly HomePage _homePage;
        private readonly LoginPage _loginPage;

        public DonationTests(WebDriverFixture fixture)
        {
            _driver = fixture.Driver;
            _baseUrl = fixture.BaseUrl;
            _donationPage = new DonationPage(_driver, _baseUrl);
            _homePage = new HomePage(_driver, _baseUrl);
            _loginPage = new LoginPage(_driver, _baseUrl);
        }

        [Fact]
        public void MakeDonation_ValidData_ShouldDonateSuccessfully()
        {
            // Arrange - Login first
            LoginAsTestUser();

            // Act
            _donationPage.NavigateTo();
            _donationPage.FillDonationForm(
                itemName: "Canned Food",
                quantity: 50,
                donationType: "Food",
                categoryName: "Food",
                description: "Various canned food items for disaster relief"
            );

            _donationPage.SubmitDonation();

            // Assert
            _donationPage.IsDonationSuccessful().Should().BeTrue();
            _donationPage.GetSuccessMessage().Should().Contain("Thank you");
        }

        [Fact]
        public void DonationPage_ShouldDisplayAllCategories()
        {
            // Arrange - Login first
            LoginAsTestUser();

            // Act
            _donationPage.NavigateTo();
            var categories = _donationPage.GetAvailableCategories();

            // Assert
            categories.Should().Contain(new[] { "Food", "Clothing", "Medical", "Shelter", "Hygiene" });
        }

        [Theory]
        [InlineData("", 10, "Food", "Item name is required")]
        [InlineData("Test Item", 0, "Food", "Quantity must be at least 1")]
        [InlineData("Test Item", -5, "Food", "Quantity must be positive")]
        public void MakeDonation_InvalidData_ShouldShowValidationErrors(string itemName, int quantity,
                                                                      string donationType, string expectedError)
        {
            // Arrange - Login first
            LoginAsTestUser();

            // Act
            _donationPage.NavigateTo();
            _donationPage.FillDonationForm(itemName, quantity, donationType);
            _donationPage.SubmitDonation();

            // Assert - Should have validation errors
            _driver.FindElements(By.CssSelector(".text-danger"))
                  .Count.Should().BeGreaterThan(0);
        }

        private void LoginAsTestUser()
        {
            _loginPage.NavigateTo();
            _loginPage.FillLoginForm("test@example.com", "TestPassword123!");
            _loginPage.SubmitLogin();
        }
    }
}
