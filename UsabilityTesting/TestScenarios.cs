namespace GiftOfTheGivers2.UsabilityTesting
{
    public class TestScenarios
    {
        public static List<UsabilityTask> GetRegistrationScenarioTasks()
        {
            return new List<UsabilityTask>
            {
                new UsabilityTask
                {
                    TaskId = "REG_01",
                    TaskName = "Navigate to Registration",
                    Description = "Find and click the registration link from the home page"
                },
                new UsabilityTask
                {
                    TaskId = "REG_02",
                    TaskName = "Complete Registration Form",
                    Description = "Fill out all required fields in the registration form"
                },
                new UsabilityTask
                {
                    TaskId = "REG_03",
                    TaskName = "Submit and Verify",
                    Description = "Submit the form and verify successful registration"
                },
                new UsabilityTask
                {
                    TaskId = "REG_04",
                    TaskName = "Initial Login",
                    Description = "Log in with the newly created account"
                }
            };
        }

        public static List<UsabilityTask> GetIncidentReportingScenarioTasks()
        {
            return new List<UsabilityTask>
            {
                new UsabilityTask
                {
                    TaskId = "INC_01",
                    TaskName = "Find Reporting Feature",
                    Description = "Locate where to report a new disaster incident"
                },
                new UsabilityTask
                {
                    TaskId = "INC_02",
                    TaskName = "Complete Incident Form",
                    Description = "Fill out all required information about the disaster incident"
                },
                new UsabilityTask
                {
                    TaskId = "INC_03",
                    TaskName = "Add Location Details",
                    Description = "Provide location information and map details if available"
                },
                new UsabilityTask
                {
                    TaskId = "INC_04",
                    TaskName = "Submit Report",
                    Description = "Submit the incident report and verify confirmation"
                }
            };
        }

        public static List<UsabilityTask> GetDonationScenarioTasks()
        {
            return new List<UsabilityTask>
            {
                new UsabilityTask
                {
                    TaskId = "DON_01",
                    TaskName = "Navigate to Donation",
                    Description = "Find the donation section from the main navigation"
                },
                new UsabilityTask
                {
                    TaskId = "DON_02",
                    TaskName = "Select Donation Type",
                    Description = "Choose what type of donation to make (food, clothing, medical, etc.)"
                },
                new UsabilityTask
                {
                    TaskId = "DON_03",
                    TaskName = "Provide Donation Details",
                    Description = "Enter item details, quantity, and description"
                },
                new UsabilityTask
                {
                    TaskId = "DON_04",
                    TaskName = "Complete Donation",
                    Description = "Submit the donation and receive confirmation"
                }
            };
        }

        public static List<UsabilityTask> GetVolunteerScenarioTasks()
        {
            return new List<UsabilityTask>
            {
                new UsabilityTask
                {
                    TaskId = "VOL_01",
                    TaskName = "Find Volunteer Opportunities",
                    Description = "Locate available volunteer tasks and opportunities"
                },
                new UsabilityTask
                {
                    TaskId = "VOL_02",
                    TaskName = "View Task Details",
                    Description = "Examine details of a specific volunteer task"
                },
                new UsabilityTask
                {
                    TaskId = "VOL_03",
                    TaskName = "Join Volunteer Task",
                    Description = "Sign up for a volunteer task"
                },
                new UsabilityTask
                {
                    TaskId = "VOL_04",
                    TaskName = "View My Assignments",
                    Description = "Check currently assigned volunteer tasks"
                }
            };
        }

        public static List<UsabilityTask> GetEmergencyAccessScenarioTasks()
        {
            return new List<UsabilityTask>
            {
                new UsabilityTask
                {
                    TaskId = "EMG_01",
                    TaskName = "Find Emergency Information",
                    Description = "Locate critical emergency information quickly"
                },
                new UsabilityTask
                {
                    TaskId = "EMG_02",
                    TaskName = "Access Disaster Updates",
                    Description = "Find the latest updates on current disasters"
                },
                new UsabilityTask
                {
                    TaskId = "EMG_03",
                    TaskName = "Locate Help Resources",
                    Description = "Find available help resources and contact information"
                },
                new UsabilityTask
                {
                    TaskId = "EMG_04",
                    TaskName = "Quick Navigation",
                    Description = "Navigate to different sections quickly during simulated emergency"
                }
            };
        }
    }
}

