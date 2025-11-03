using OpenQA.Selenium;

namespace GiftOfTheGivers2.UITests.PageObjects
{
    public class LoginPage : BasePage
    {
        public LoginPage(IWebDriver driver, string baseUrl) : base(driver, baseUrl) { }

        // Locators
        private By EmailInput => By.Id("Email");
        private By PasswordInput => By.Name("Password");
        private By LoginButton => By.CssSelector("button[type='submit']");
        private By ErrorMessage => By.CssSelector(".alert-danger");
        private By RememberMeCheckbox => By.Id("RememberMe");

        public override void NavigateTo()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Account/Login");
            WaitForPageLoad();
            AssertPageIsLoaded();
        }

        public void AssertPageIsLoaded()
        {
            AssertElementIsVisible(EmailInput, "Email input");
            AssertElementIsVisible(PasswordInput, "Password input");
            AssertElementIsVisible(LoginButton, "Login button");
            TakeScreenshot("LoginPage_Loaded");
        }

        public void FillLoginForm(string email, string password, bool rememberMe = false)
        {
            TypeText(EmailInput, email);
            TypeText(PasswordInput, password);

            if (rememberMe && IsElementVisible(RememberMeCheckbox))
            {
                var checkbox = WaitForElement(RememberMeCheckbox);
                if (!checkbox.Selected)
                    checkbox.Click();
            }
        }

        public void SubmitLogin()
        {
            ClickElement(LoginButton);
            WaitForPageLoad();
        }

        public bool IsLoginSuccessful()
        {
            // Check if redirected to home page and user menu is visible
            return Driver.Url == BaseUrl + "/" || Driver.Url == BaseUrl + "/Home/Index";
        }

        public bool IsLoginFailed()
        {
            return IsElementVisible(ErrorMessage);
        }

        public string GetErrorMessage()
        {
            return IsLoginFailed() ? GetElementText(ErrorMessage) : string.Empty;
        }
    }
}

