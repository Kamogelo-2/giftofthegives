using OpenQA.Selenium;

namespace GiftOfTheGivers2.UITests.PageObjects
{
    public class VolunteerTasksPage : BasePage
    {
        public VolunteerTasksPage(IWebDriver driver, string baseUrl) : base(driver, baseUrl) { }

        // Locators
        private By TaskCards => By.CssSelector(".task-card");
        private By JoinButtons => By.CssSelector(".btn-join-task");
        private By TaskDetailsLinks => By.CssSelector(".task-details-link");
        private By SuccessMessage => By.CssSelector(".alert-success");
        private By TaskFilter => By.Id("taskFilter");
        private By MyAssignmentsLink => By.CssSelector("a[href*='/Volunteer/MyAssignments']");

        public override void NavigateTo()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Volunteer/Tasks");
            WaitForPageLoad();
            AssertPageIsLoaded();
        }

        public void AssertPageIsLoaded()
        {
            AssertElementIsVisible(TaskCards, "Task cards container");
            TakeScreenshot("VolunteerTasksPage_Loaded");
        }

        public int GetAvailableTasksCount()
        {
            return Driver.FindElements(TaskCards).Count;
        }

        public void ClickTaskDetails(int taskIndex = 0)
        {
            var detailsLinks = Driver.FindElements(TaskDetailsLinks);
            if (detailsLinks.Count > taskIndex)
            {
                detailsLinks[taskIndex].Click();
                WaitForPageLoad();
            }
        }

        public void JoinTask(int taskIndex = 0)
        {
            var joinButtons = Driver.FindElements(JoinButtons);
            if (joinButtons.Count > taskIndex)
            {
                joinButtons[taskIndex].Click();
                WaitForPageLoad();
            }
        }

        public bool IsJoinSuccessful()
        {
            return IsElementVisible(SuccessMessage);
        }

        public string GetSuccessMessage()
        {
            return IsJoinSuccessful() ? GetElementText(SuccessMessage) : string.Empty;
        }

        public void NavigateToMyAssignments()
        {
            ClickElement(MyAssignmentsLink);
            WaitForPageLoad();
        }

        public List<string> GetTaskTitles()
        {
            return Driver.FindElements(By.CssSelector(".task-card .card-title"))
                        .Select(element => element.Text)
                        .ToList();
        }
    }
}

