using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace GiftOfTheGivers2.UITests.PageObjects
{
    public class DonationPage : BasePage
    {
        public DonationPage(IWebDriver driver, string baseUrl) : base(driver, baseUrl) { }

        // Locators
        private By CategorySelect => By.Id("CategoryId");
        private By ItemNameInput => By.Id("ItemName");
        private By QuantityInput => By.Id("Quantity");
        private By DescriptionInput => By.Id("Description");
        private By DonationTypeSelect => By.Id("DonationType");
        private By SubmitButton => By.CssSelector("button[type='submit']");
        private By SuccessMessage => By.CssSelector(".alert-success");
        private By DonationTable => By.CssSelector("table.table");

        public override void NavigateTo()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Donation/Donate");
            WaitForPageLoad();
            AssertPageIsLoaded();
        }

        public void AssertPageIsLoaded()
        {
            AssertElementIsVisible(CategorySelect, "Category select");
            AssertElementIsVisible(ItemNameInput, "Item name input");
            AssertElementIsVisible(SubmitButton, "Submit button");
            TakeScreenshot("DonationPage_Loaded");
        }

        public void FillDonationForm(string itemName, int quantity, string donationType,
                                   string categoryName = "Food", string description = null)
        {
            // Select category
            if (!string.IsNullOrEmpty(categoryName))
            {
                var categorySelect = new SelectElement(WaitForElement(CategorySelect));
                categorySelect.SelectByText(categoryName);
            }

            TypeText(ItemNameInput, itemName);
            TypeText(QuantityInput, quantity.ToString());

            // Select donation type
            var donationTypeSelect = new SelectElement(WaitForElement(DonationTypeSelect));
            donationTypeSelect.SelectByText(donationType);

            if (!string.IsNullOrEmpty(description))
                TypeText(DescriptionInput, description);
        }

        public void SubmitDonation()
        {
            ClickElement(SubmitButton);
            WaitForPageLoad();
        }

        public bool IsDonationSuccessful()
        {
            return IsElementVisible(SuccessMessage);
        }

        public string GetSuccessMessage()
        {
            return IsDonationSuccessful() ? GetElementText(SuccessMessage) : string.Empty;
        }

        public List<string> GetAvailableCategories()
        {
            var categorySelect = new SelectElement(WaitForElement(CategorySelect));
            return categorySelect.Options.Select(option => option.Text).ToList();
        }
    }
}

