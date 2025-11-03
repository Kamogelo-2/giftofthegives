using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace GiftOfTheGivers2.UITests.PageObjects
{
    public class RegisterPage : BasePage
    {
        public RegisterPage(IWebDriver driver, string baseUrl) : base(driver, baseUrl) { }

        // Locators
        private By FirstNameInput => By.Id("FirstName");
        private By LastNameInput => By.Id("LastName");
        private By EmailInput => By.Id("Email");
        private By PasswordInput => By.Name("password");
        private By PhoneNumberInput => By.Id("PhoneNumber");
        private By AddressInput => By.Id("Address");
        private By UserTypeSelect => By.Id("UserType");
        private By RegisterButton => By.CssSelector("button[type='submit']");
        private By SuccessMessage => By.CssSelector(".alert-success");
        private By ErrorMessages => By.CssSelector(".text-danger");
        private By ValidationSummary => By.CssSelector(".validation-summary-errors");

        public override void NavigateTo()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Account/Register");
            WaitForPageLoad();
            AssertPageIsLoaded();
        }

        public void AssertPageIsLoaded()
        {
            AssertElementIsVisible(FirstNameInput, "First name input");
            AssertElementIsVisible(EmailInput, "Email input");
            AssertElementIsVisible(RegisterButton, "Register button");
            TakeScreenshot("RegisterPage_Loaded");
        }

        public void FillRegistrationForm(string firstName, string lastName, string email, string password,
                                      string phoneNumber = null, string address = null, string userType = "Volunteer")
        {
            TypeText(FirstNameInput, firstName);
            TypeText(LastNameInput, lastName);
            TypeText(EmailInput, email);
            TypeText(PasswordInput, password);

            if (!string.IsNullOrEmpty(phoneNumber))
                TypeText(PhoneNumberInput, phoneNumber);

            if (!string.IsNullOrEmpty(address))
                TypeText(AddressInput, address);

            if (!string.IsNullOrEmpty(userType))
            {
                var selectElement = new SelectElement(WaitForElement(UserTypeSelect));
                selectElement.SelectByText(userType);
            }
        }

        public void SubmitRegistration()
        {
            ClickElement(RegisterButton);
            WaitForPageLoad();
        }

        public bool IsRegistrationSuccessful()
        {
            return IsElementVisible(SuccessMessage);
        }

        public string GetSuccessMessage()
        {
            return IsRegistrationSuccessful() ? GetElementText(SuccessMessage) : string.Empty;
        }

        public bool HasValidationErrors()
        {
            return IsElementVisible(ValidationSummary) || Driver.FindElements(ErrorMessages).Count > 0;
        }

        public List<string> GetErrorMessages()
        {
            var errors = new List<string>();

            if (IsElementVisible(ValidationSummary))
            {
                errors.Add(GetElementText(ValidationSummary));
            }

            var fieldErrors = Driver.FindElements(ErrorMessages);
            foreach (var error in fieldErrors)
            {
                if (!string.IsNullOrEmpty(error.Text))
                    errors.Add(error.Text);
            }

            return errors;
        }
    }
}
