using OpenQA.Selenium;
using GiftOfTheGivers2.UITests.PageObjects;
    
namespace GiftOfTheGivers2.UsabilityTesting
{
    public class AutomatedUsabilityChecks
    {
        private readonly IWebDriver _driver;
        private readonly SessionManager _sessionManager;

        public AutomatedUsabilityChecks(IWebDriver driver, SessionManager sessionManager)
        {
            _driver = driver;
            _sessionManager = sessionManager;
        }

        public async Task ConductAutomatedAccessibilityCheck()
        {
            Console.WriteLine("Running automated accessibility checks...");

            // Check page title
            var pageTitle = _driver.Title;
            if (string.IsNullOrEmpty(pageTitle) || pageTitle.Length < 10)
            {
                _sessionManager.RecordIssue("Page title is missing or too short", "Global", IssueSeverity.Minor, "Accessibility");
            }

            // Check for proper heading structure
            var headings = _driver.FindElements(By.TagName("h1"));
            if (headings.Count == 0)
            {
                _sessionManager.RecordIssue("No H1 heading found on page", "Global", IssueSeverity.Major, "Accessibility");
            }

            // Check image alt texts
            var images = _driver.FindElements(By.TagName("img"));
            foreach (var img in images.Take(10)) // Check first 10 images
            {
                var alt = img.GetAttribute("alt");
                if (string.IsNullOrEmpty(alt))
                {
                    _sessionManager.RecordIssue("Image missing alt text", "Visual Content", IssueSeverity.Minor, "Accessibility");
                }
            }

            // Check form labels
            var inputs = _driver.FindElements(By.TagName("input"));
            foreach (var input in inputs.Take(10))
            {
                var id = input.GetAttribute("id");
                if (!string.IsNullOrEmpty(id))
                {
                    var label = _driver.FindElements(By.CssSelector($"label[for='{id}']"));
                    if (!label.Any())
                    {
                        _sessionManager.RecordIssue("Input field missing associated label", "Forms", IssueSeverity.Major, "Accessibility");
                    }
                }
            }

            // Check color contrast (simplified check)
            var body = _driver.FindElement(By.TagName("body"));
            var backgroundColor = body.GetCssValue("background-color");
            var color = body.GetCssValue("color");

            // Basic contrast check - in real implementation, use proper contrast ratio calculation
            if (backgroundColor.Contains("255,255,255") && color.Contains("255,255,255"))
            {
                _sessionManager.RecordIssue("Potential color contrast issues", "Visual Design", IssueSeverity.Minor, "Accessibility");
            }
        }

        public async Task CheckNavigationConsistency()
        {
            Console.WriteLine("Checking navigation consistency...");

            var navLinks = _driver.FindElements(By.CssSelector("nav a"));
            var uniqueLinks = navLinks.Select(l => l.Text).Where(t => !string.IsNullOrEmpty(t)).Distinct().ToList();

            if (uniqueLinks.Count < 3)
            {
                _sessionManager.RecordIssue("Navigation seems limited", "Navigation", IssueSeverity.Minor, "Navigation");
            }

            // Check if main navigation items are present
            var expectedLinks = new[] { "Home", "Incidents", "Volunteer", "Donate" };
            foreach (var expected in expectedLinks)
            {
                if (!uniqueLinks.Any(l => l.Contains(expected, StringComparison.OrdinalIgnoreCase)))
                {
                    _sessionManager.RecordIssue($"Expected navigation item '{expected}' not found", "Navigation", IssueSeverity.Major, "Navigation");
                }
            }
        }

        public async Task CheckResponsiveDesign()
        {
            Console.WriteLine("Checking responsive design...");

            // Test different viewport sizes
            var viewports = new[] { (375, 667), (768, 1024), (1920, 1080) }; // Mobile, Tablet, Desktop

            foreach (var (width, height) in viewports)
            {
                _driver.Manage().Window.Size = new System.Drawing.Size(width, height);
                await Task.Delay(500); // Allow for layout adjustment

                // Check for horizontal scrolling (indicative of responsive issues)
                var body = _driver.FindElement(By.TagName("body"));
                var bodySize = body.Size;
                var windowSize = _driver.Manage().Window.Size;

                if (bodySize.Width > windowSize.Width)
                {
                    _sessionManager.RecordIssue($"Horizontal scrolling detected at {width}x{height}", "Responsive Design", IssueSeverity.Minor, "Layout");
                }

                // Check if critical elements are visible
                var criticalSelectors = new[] { "nav", ".navbar", "main", "footer" };
                foreach (var selector in criticalSelectors)
                {
                    try
                    {
                        var element = _driver.FindElement(By.CssSelector(selector));
                        if (!element.Displayed)
                        {
                            _sessionManager.RecordIssue($"Critical element '{selector}' not visible at {width}x{height}", "Responsive Design", IssueSeverity.Major, "Layout");
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        _sessionManager.RecordIssue($"Critical element '{selector}' missing at {width}x{height}", "Responsive Design", IssueSeverity.Major, "Layout");
                    }
                }
            }

            // Reset to default size
            _driver.Manage().Window.Maximize();
        }

        public async Task CheckFormUsability()
        {
            Console.WriteLine("Checking form usability...");

            var forms = _driver.FindElements(By.TagName("form"));
            foreach (var form in forms)
            {
                // Check for required field indicators
                var requiredFields = form.FindElements(By.CssSelector("[required]"));
                var requiredIndicators = form.FindElements(By.CssSelector(".required, [aria-required='true']"));

                if (requiredFields.Count > 0 && requiredIndicators.Count < requiredFields.Count)
                {
                    _sessionManager.RecordIssue("Not all required fields have visual indicators", "Forms", IssueSeverity.Minor, "Forms");
                }

                // Check for clear error messaging
                var errorMessages = form.FindElements(By.CssSelector(".text-danger, .field-validation-error"));
                if (errorMessages.Any(e => string.IsNullOrEmpty(e.Text)))
                {
                    _sessionManager.RecordIssue("Empty error message containers found", "Forms", IssueSeverity.Minor, "Forms");
                }

                // Check submit button accessibility
                var submitButtons = form.FindElements(By.CssSelector("button[type='submit'], input[type='submit']"));
                if (!submitButtons.Any(b => b.Displayed && b.Enabled))
                {
                    _sessionManager.RecordIssue("No visible/enabled submit button in form", "Forms", IssueSeverity.Major, "Forms");
                }
            }
        }

        public async Task CheckContentReadability()
        {
            Console.WriteLine("Checking content readability...");

            // Check font sizes
            var body = _driver.FindElement(By.TagName("body"));
            var fontSize = body.GetCssValue("font-size");

            if (fontSize.Contains("px"))
            {
                var size = int.Parse(fontSize.Replace("px", ""));
                if (size < 14)
                {
                    _sessionManager.RecordIssue("Base font size may be too small for comfortable reading", "Content", IssueSeverity.Minor, "Content");
                }
            }

            // Check line height
            var lineHeight = body.GetCssValue("line-height");
            if (lineHeight.Contains("px"))
            {
                var height = float.Parse(lineHeight.Replace("px", ""));
                var size = float.Parse(fontSize.Replace("px", ""));
                var ratio = height / size;

                if (ratio < 1.4)
                {
                    _sessionManager.RecordIssue("Line height may be too tight for comfortable reading", "Content", IssueSeverity.Minor, "Content");
                }
            }

            // Check text contrast
            var color = body.GetCssValue("color");
            var backgroundColor = body.GetCssValue("background-color");

            // Simple contrast check (in real implementation, use proper contrast ratio calculation)
            if ((color.Contains("255,255,255") && backgroundColor.Contains("255,255,255")) ||
                (color.Contains("0,0,0") && backgroundColor.Contains("0,0,0")))
            {
                _sessionManager.RecordIssue("Potential text contrast issues", "Content", IssueSeverity.Minor, "Content");
            }
        }

        public async Task RunComprehensiveAutomatedCheck()
        {
            Console.WriteLine("Starting comprehensive automated usability check...");

            await ConductAutomatedAccessibilityCheck();
            await CheckNavigationConsistency();
            await CheckResponsiveDesign();
            await CheckFormUsability();
            await CheckContentReadability();

            Console.WriteLine("Automated usability check completed.");
        }
    }
}

