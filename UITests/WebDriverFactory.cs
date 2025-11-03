using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;

namespace GiftOfTheGivers2.UITests
{
    public class WebDriverFactory
    {
        public enum BrowserType
        {
            Chrome,
            Firefox,
            Edge,
            ChromeHeadless,
            FirefoxHeadless
        }

        public class WebDriverFactory
        {
            public static IWebDriver CreateDriver(BrowserType browserType)
            {
                return browserType switch
                {
                    BrowserType.Chrome => CreateChromeDriver(false),
                    BrowserType.ChromeHeadless => CreateChromeDriver(true),
                    BrowserType.Firefox => CreateFirefoxDriver(false),
                    BrowserType.FirefoxHeadless => CreateFirefoxDriver(true),
                    BrowserType.Edge => CreateEdgeDriver(),
                    _ => throw new ArgumentException($"Unsupported browser type: {browserType}")
                };
            }

            private static ChromeDriver CreateChromeDriver(bool headless)
            {
                var options = new ChromeOptions();

                if (headless)
                {
                    options.AddArgument("--headless");
                }

                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("--ignore-certificate-errors");
                options.AddArgument("--disable-extensions");
                options.AddArgument("--disable-notifications");

                // Additional options for better stability
                options.AddArgument("--disable-web-security");
                options.AddArgument("--allow-running-insecure-content");
                options.AddArgument("--disable-blink-features=AutomationControlled");
                options.AddExcludedArgument("enable-automation");
                options.AddAdditionalOption("useAutomationExtension", false);

                var driver = new ChromeDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
                driver.Manage().Window.Maximize();

                return driver;
            }

            private static FirefoxDriver CreateFirefoxDriver(bool headless)
            {
                var options = new FirefoxOptions();

                if (headless)
                {
                    options.AddArgument("--headless");
                }

                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--window-size=1920,1080");

                var driver = new FirefoxDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
                driver.Manage().Window.Maximize();

                return driver;
            }

            private static EdgeDriver CreateEdgeDriver()
            {
                var options = new EdgeOptions();
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--window-size=1920,1080");

                var driver = new EdgeDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
                driver.Manage().Window.Maximize();

                return driver;
            }

            public static void TakeScreenshot(IWebDriver driver, string testName)
            {
                try
                {
                    var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                    var fileName = $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    var filePath = Path.Combine("screenshots", fileName);

                    Directory.CreateDirectory("screenshots");
                    screenshot.SaveAsFile(filePath, ScreenshotImageFormat.Png);

                    Console.WriteLine($"Screenshot saved: {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to take screenshot: {ex.Message}");
                }
            }
        }
    }
}
