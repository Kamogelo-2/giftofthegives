using OpenQA.Selenium;

namespace GiftOfTheGivers2.UITests.PageObjects
{
    public class HomePage : BasePage
    {
        public HomePage(IWebDriver driver, string baseUrl) : base(driver, baseUrl) { }

        // Locators
        private By NavigationBar => By.CssSelector("nav.navbar");
        private By HeroSection => By.CssSelector(".hero-section");
        private By IncidentStats => By.CssSelector(".stat-card");
        private By RecentIncidents => By.CssSelector(".incident-card");
        private By RegisterLink => By.CssSelector("a[href*='/Account/Register']");
        private By LoginLink => By.CssSelector("a[href*='/Account/Login']");
        private By IncidentListLink => By.CssSelector("a[href*='/Incident/List']");
        private By VolunteerTasksLink => By.CssSelector("a[href*='/Volunteer/Tasks']");
        private By DonateLink => By.CssSelector("a[href*='/Donation/Donate']");

        public override void NavigateTo()
        {
            Driver.Navigate().GoToUrl(BaseUrl);
            WaitForPageLoad();
            AssertPageIsLoaded();
        }

        public void AssertPageIsLoaded()
        {
            AssertElementIsVisible(NavigationBar, "Navigation bar");
            AssertElementIsVisible(HeroSection, "Hero section");
            TakeScreenshot("HomePage_Loaded");
        }

        public void ClickRegisterLink()
        {
            ClickElement(RegisterLink);
        }

        public void ClickLoginLink()
        {
            ClickElement(LoginLink);
        }

        public void ClickIncidentListLink()
        {
            ClickElement(IncidentListLink);
        }

        public void ClickVolunteerTasksLink()
        {
            ClickElement(VolunteerTasksLink);
        }

        public void ClickDonateLink()
        {
            ClickElement(DonateLink);
        }

        public int GetStatCardsCount()
        {
            return Driver.FindElements(IncidentStats).Count;
        }

        public int GetRecentIncidentsCount()
        {
            return Driver.FindElements(RecentIncidents).Count;
        }

        public bool IsUserLoggedIn(string userName)
        {
            return IsElementVisible(By.XPath($"//a[contains(text(), '{userName}')]"));
        }
    }
}
