using System.Text;

namespace GiftOfTheGivers2.UsabilityTesting
{
    public class DataCollectionTools
    {
        public static SystemUsabilityScale CreateSUSQuestionnaire()
        {
            return new SystemUsabilityScale
            {
                Questions = new List<SUSQuestion>
                {
                    new SUSQuestion { Number = 1, Text = "I think that I would like to use this system frequently", IsPositive = true },
                    new SUSQuestion { Number = 2, Text = "I found the system unnecessarily complex", IsPositive = false },
                    new SUSQuestion { Number = 3, Text = "I thought the system was easy to use", IsPositive = true },
                    new SUSQuestion { Number = 4, Text = "I think that I would need the support of a technical person to be able to use this system", IsPositive = false },
                    new SUSQuestion { Number = 5, Text = "I found the various functions in this system were well integrated", IsPositive = true },
                    new SUSQuestion { Number = 6, Text = "I thought there was too much inconsistency in this system", IsPositive = false },
                    new SUSQuestion { Number = 7, Text = "I would imagine that most people would learn to use this system very quickly", IsPositive = true },
                    new SUSQuestion { Number = 8, Text = "I found the system very cumbersome to use", IsPositive = false },
                    new SUSQuestion { Number = 9, Text = "I felt very confident using the system", IsPositive = true },
                    new SUSQuestion { Number = 10, Text = "I needed to learn a lot of things before I could get going with this system", IsPositive = false }
                }
            };
        }

        public static NetPromoterScore CreateNPSQuestionnaire()
        {
            return new NetPromoterScore
            {
                Question = "How likely are you to recommend Gift of the Givers application to a friend or colleague?",
                ScaleDescription = "0 = Not at all likely, 10 = Extremely likely"
            };
        }

        public static PostTestQuestionnaire CreatePostTestQuestionnaire()
        {
            return new PostTestQuestionnaire
            {
                Questions = new List<PostTestQuestion>
                {
                    new PostTestQuestion
                    {
                        Question = "What did you like most about the application?",
                        Type = QuestionType.OpenEnded
                    },
                    new PostTestQuestion
                    {
                        Question = "What was the most frustrating part of using the application?",
                        Type = QuestionType.OpenEnded
                    },
                    new PostTestQuestion
                    {
                        Question = "How easy was it to navigate through the application?",
                        Type = QuestionType.Rating,
                        RatingScale = "1 (Very Difficult) to 5 (Very Easy)"
                    },
                    new PostTestQuestion
                    {
                        Question = "How would you rate the visual design of the application?",
                        Type = QuestionType.Rating,
                        RatingScale = "1 (Poor) to 5 (Excellent)"
                    },
                    new PostTestQuestion
                    {
                        Question = "Did you encounter any errors or bugs? If yes, please describe:",
                        Type = QuestionType.OpenEnded
                    },
                    new PostTestQuestion
                    {
                        Question = "What features would you like to see added or improved?",
                        Type = QuestionType.OpenEnded
                    },
                    new PostTestQuestion
                    {
                        Question = "How confident do you feel using this application in an emergency situation?",
                        Type = QuestionType.Rating,
                        RatingScale = "1 (Not Confident) to 5 (Very Confident)"
                    }
                }
            };
        }

        public static AccessibilityChecklist CreateAccessibilityChecklist()
        {
            return new AccessibilityChecklist
            {
                Items = new List<AccessibilityCheckItem>
                {
                    new AccessibilityCheckItem { Category = "Visual", Description = "Adequate color contrast ratio (4.5:1 for normal text)" },
                    new AccessibilityCheckItem { Category = "Visual", Description = "Text resizes properly without breaking layout" },
                    new AccessibilityCheckItem { Category = "Visual", Description = "Clear visual focus indicators for keyboard navigation" },
                    new AccessibilityCheckItem { Category = "Navigation", Description = "Logical tab order through interactive elements" },
                    new AccessibilityCheckItem { Category = "Navigation", Description = "Keyboard access to all functionality" },
                    new AccessibilityCheckItem { Category = "Navigation", Description = "Skip navigation links available" },
                    new AccessibilityCheckItem { Category = "Content", Description = "Meaningful link text (not 'click here')" },
                    new AccessibilityCheckItem { Category = "Content", Description = "Proper heading structure (h1, h2, h3)" },
                    new AccessibilityCheckItem { Category = "Content", Description = "Alt text for meaningful images" },
                    new AccessibilityCheckItem { Category = "Forms", Description = "Clear form labels associated with inputs" },
                    new AccessibilityCheckItem { Category = "Forms", Description = "Error messages that are clearly associated with fields" },
                    new AccessibilityCheckItem { Category = "Forms", Description = "Form validation that doesn't rely solely on color" }
                }
            };
        }
    }

    public class SystemUsabilityScale
    {
        public List<SUSQuestion> Questions { get; set; }

        public double CalculateScore(Dictionary<int, int> responses)
        {
            if (responses.Count != 10) return 0;

            double total = 0;
            foreach (var response in responses)
            {
                var question = Questions.First(q => q.Number == response.Key);
                int score = question.IsPositive ? response.Value - 1 : 5 - response.Value;
                total += score;
            }

            return total * 2.5; // Convert to 0-100 scale
        }
    }

    public class SUSQuestion
    {
        public int Number { get; set; }
        public string Text { get; set; }
        public bool IsPositive { get; set; }
    }

    public class NetPromoterScore
    {
        public string Question { get; set; }
        public string ScaleDescription { get; set; }

        public (int promoters, int passives, int detractors) CalculateNPSSegments(List<int> scores)
        {
            int promoters = scores.Count(s => s >= 9);
            int passives = scores.Count(s => s >= 7 && s <= 8);
            int detractors = scores.Count(s => s <= 6);

            return (promoters, passives, detractors);
        }

        public double CalculateNPS(List<int> scores)
        {
            var segments = CalculateNPSSegments(scores);
            double total = scores.Count;

            if (total == 0) return 0;

            return ((double)segments.promoters / total - (double)segments.detractors / total) * 100;
        }
    }

    public class PostTestQuestionnaire
    {
        public List<PostTestQuestion> Questions { get; set; }
    }

    public class PostTestQuestion
    {
        public string Question { get; set; }
        public QuestionType Type { get; set; }
        public string RatingScale { get; set; }
        public string Response { get; set; }
    }

    public enum QuestionType
    {
        OpenEnded,
        Rating,
        MultipleChoice
    }

    public class AccessibilityChecklist
    {
        public List<AccessibilityCheckItem> Items { get; set; }

        public AccessibilityReport GenerateReport(Dictionary<string, bool> results)
        {
            return new AccessibilityReport
            {
                TotalItems = Items.Count,
                PassedItems = results.Count(r => r.Value),
                FailedItems = results.Count(r => !r.Value),
                ComplianceScore = (double)results.Count(r => r.Value) / Items.Count * 100,
                FailedChecks = results.Where(r => !r.Value)
                                    .ToDictionary(r => r.Key, r => r.Value)
            };
        }
    }

    public class AccessibilityCheckItem
    {
        public string Category { get; set; }
        public string Description { get; set; }
    }

    public class AccessibilityReport
    {
        public int TotalItems { get; set; }
        public int PassedItems { get; set; }
        public int FailedItems { get; set; }
        public double ComplianceScore { get; set; }
        public Dictionary<string, bool> FailedChecks { get; set; }
    }
}

