using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using FluentAssertions;

namespace GiftOfTheGivers2.UITests.PageObjects
{
    public abstract class BasePage
    {
        protected readonly IWebDriver Driver;
        protected readonly WebDriverWait Wait;
        protected readonly string BaseUrl;

        protected BasePage(IWebDriver driver, string baseUrl)
        {
            Driver = driver;
            BaseUrl = baseUrl;
            Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        public virtual void NavigateTo()
        {
            Driver.Navigate().GoToUrl(BaseUrl);
            WaitForPageLoad();
        }

        protected void WaitForPageLoad()
        {
            Wait.Until(driver =>
                ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }

        protected IWebElement WaitForElement(By locator)
        {
            return Wait.Until(driver =>
            {
                var element = driver.FindElement(locator);
                return element.Displayed && element.Enabled ? element : null;
            });
        }

        protected void ClickElement(By locator)
        {
            var element = WaitForElement(locator);
            element.Click();
        }

        protected void TypeText(By locator, string text)
        {
            var element = WaitForElement(locator);
            element.Clear();
            element.SendKeys(text);
        }

        protected string GetElementText(By locator)
        {
            return WaitForElement(locator).Text;
        }

        protected bool IsElementVisible(By locator)
        {
            try
            {
                return Driver.FindElement(locator).Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        protected void AssertElementIsVisible(By locator, string elementName)
        {
            IsElementVisible(locator).Should().BeTrue($"{elementName} should be visible");
        }

        protected void AssertElementText(By locator, string expectedText, string elementName)
        {
            var actualText = GetElementText(locator);
            actualText.Should().Be(expectedText, $"{elementName} should display '{expectedText}'");
        }

        protected void TakeScreenshot(string screenshotName)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                var fileName = $"{screenshotName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                screenshot.SaveAsFile(fileName);
                Console.WriteLine($"Screenshot saved: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to take screenshot: {ex.Message}");
            }
        }
    }
}
