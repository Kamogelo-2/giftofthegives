using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace GiftOfTheGivers2.UITests.PageObjects
{
    public class IncidentReportPage : BasePage
    {
        public IncidentReportPage(IWebDriver driver, string baseUrl) : base(driver, baseUrl) { }

        // Locators
        private By TitleInput => By.Id("Title");
        private By DescriptionInput => By.Id("Description");
        private By IncidentTypeSelect => By.Id("IncidentType");
        private By LocationInput => By.Id("Location");
        private By SeverityLevelSelect => By.Id("SeverityLevel");
        private By PeopleAffectedInput => By.Id("PeopleAffected");
        private By ImmediateNeedsInput => By.Id("ImmediateNeeds");
        private By SubmitButton => By.CssSelector("button[type='submit']");
        private By SuccessMessage => By.CssSelector(".alert-success");
        private By LatitudeInput => By.Id("Latitude");
        private By LongitudeInput => By.Id("Longitude");

        public override void NavigateTo()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Incident/Report");
            WaitForPageLoad();
            AssertPageIsLoaded();
        }

        public void AssertPageIsLoaded()
        {
            AssertElementIsVisible(TitleInput, "Title input");
            AssertElementIsVisible(DescriptionInput, "Description input");
            AssertElementIsVisible(SubmitButton, "Submit button");
            TakeScreenshot("IncidentReportPage_Loaded");
        }

        public void FillIncidentForm(string title, string description, string incidentType, string location,
                                   string severityLevel = "Medium", int? peopleAffected = null,
                                   string immediateNeeds = null, decimal? latitude = null, decimal? longitude = null)
        {
            TypeText(TitleInput, title);
            TypeText(DescriptionInput, description);
            TypeText(LocationInput, location);

            // Select incident type
            var incidentTypeSelect = new SelectElement(WaitForElement(IncidentTypeSelect));
            incidentTypeSelect.SelectByText(incidentType);

            // Select severity level
            var severitySelect = new SelectElement(WaitForElement(SeverityLevelSelect));
            severitySelect.SelectByText(severityLevel);

            if (peopleAffected.HasValue)
                TypeText(PeopleAffectedInput, peopleAffected.Value.ToString());

            if (!string.IsNullOrEmpty(immediateNeeds))
                TypeText(ImmediateNeedsInput, immediateNeeds);

            if (latitude.HasValue)
                TypeText(LatitudeInput, latitude.Value.ToString());

            if (longitude.HasValue)
                TypeText(LongitudeInput, longitude.Value.ToString());
        }

        public void SubmitIncidentReport()
        {
            ClickElement(SubmitButton);
            WaitForPageLoad();
        }

        public bool IsReportSuccessful()
        {
            return IsElementVisible(SuccessMessage);
        }

        public string GetSuccessMessage()
        {
            return IsReportSuccessful() ? GetElementText(SuccessMessage) : string.Empty;
        }

        public bool HasValidationErrors()
        {
            return Driver.FindElements(By.CssSelector(".text-danger")).Count > 0;
        }
    }
}

