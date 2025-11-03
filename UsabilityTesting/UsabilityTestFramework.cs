using System.Text.Json;

namespace GiftOfTheGivers2.UsabilityTesting
{
    public class UsabilityTestFramework
    {
        private readonly List<UsabilityTestSession> _sessions;
        private readonly string _resultsPath;

        public UsabilityTestFramework()
        {
            _sessions = new List<UsabilityTestSession>();
            _resultsPath = Path.Combine(Directory.GetCurrentDirectory(), "UsabilityTestResults");
            Directory.CreateDirectory(_resultsPath);
        }

        public UsabilityTestSession CreateSession(TestParticipant participant, string testScenario)
        {
            var session = new UsabilityTestSession
            {
                SessionId = Guid.NewGuid(),
                Participant = participant,
                TestScenario = testScenario,
                StartTime = DateTime.Now,
                Tasks = new List<UsabilityTask>(),
                Observations = new List<Observation>(),
                Metrics = new UsabilityMetrics()
            };

            _sessions.Add(session);
            return session;
        }

        public async Task SaveSessionResults(UsabilityTestSession session)
        {
            var fileName = $"Session_{session.SessionId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(_resultsPath, fileName);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(session, options);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<UsabilityTestReport> GenerateComprehensiveReport()
        {
            var report = new UsabilityTestReport
            {
                GeneratedAt = DateTime.Now,
                TotalSessions = _sessions.Count,
                ParticipantDemographics = AnalyzeDemographics(),
                TaskAnalysis = AnalyzeTaskPerformance(),
                SatisfactionScores = CalculateSatisfactionScores(),
                IssuesBySeverity = CategorizeIssues(),
                Recommendations = GenerateRecommendations()
            };

            var reportPath = Path.Combine(_resultsPath, $"ComprehensiveReport_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(reportPath, json);

            return report;
        }

        private ParticipantDemographics AnalyzeDemographics()
        {
            return new ParticipantDemographics
            {
                TotalParticipants = _sessions.Count,
                AgeGroups = _sessions.GroupBy(s => s.Participant.AgeGroup)
                                    .ToDictionary(g => g.Key, g => g.Count()),
                TechProficiencyLevels = _sessions.GroupBy(s => s.Participant.TechProficiency)
                                               .ToDictionary(g => g.Key, g => g.Count()),
                UserTypes = _sessions.GroupBy(s => s.Participant.UserType)
                                   .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        private TaskPerformanceAnalysis AnalyzeTaskPerformance()
        {
            var allTasks = _sessions.SelectMany(s => s.Tasks).ToList();

            return new TaskPerformanceAnalysis
            {
                AverageCompletionTime = allTasks.Average(t => t.CompletionTimeSeconds),
                SuccessRate = (double)allTasks.Count(t => t.CompletedSuccessfully) / allTasks.Count * 100,
                AverageErrorCount = allTasks.Average(t => t.ErrorCount),
                CommonPainPoints = allTasks.Where(t => t.ErrorCount > 0)
                                         .GroupBy(t => t.TaskName)
                                         .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        private SatisfactionScores CalculateSatisfactionScores()
        {
            var susScores = _sessions.Where(s => s.Metrics.SystemUsabilityScaleScore > 0)
                                   .Select(s => s.Metrics.SystemUsabilityScaleScore);

            var npsScores = _sessions.Where(s => s.Metrics.NetPromoterScore.HasValue)
                                   .Select(s => s.Metrics.NetPromoterScore.Value);

            return new SatisfactionScores
            {
                AverageSUS = susScores.Any() ? susScores.Average() : 0,
                AverageNPS = npsScores.Any() ? npsScores.Average() : 0,
                TaskSatisfaction = _sessions.Average(s => s.Metrics.TaskSatisfactionRating)
            };
        }

        private Dictionary<IssueSeverity, List<UsabilityIssue>> CategorizeIssues()
        {
            var allIssues = _sessions.SelectMany(s => s.Observations)
                                   .Where(o => o.IsIssue)
                                   .Select(o => new UsabilityIssue
                                   {
                                       Description = o.Description,
                                       Severity = o.Severity,
                                       Context = o.Context,
                                       Frequency = _sessions.Count(s => s.Observations.Any(obs =>
                                           obs.Description == o.Description && obs.IsIssue))
                                   })
                                   .Distinct()
                                   .ToList();

            return allIssues.GroupBy(i => i.Severity)
                           .ToDictionary(g => g.Key, g => g.ToList());
        }

        private List<Recommendation> GenerateRecommendations()
        {
            var recommendations = new List<Recommendation>();
            var issuesBySeverity = CategorizeIssues();

            // Generate recommendations based on critical and major issues
            foreach (var issue in issuesBySeverity.GetValueOrDefault(IssueSeverity.Critical, new List<UsabilityIssue>()))
            {
                recommendations.Add(new Recommendation
                {
                    Priority = Priority.Critical,
                    Description = $"Address critical issue: {issue.Description}",
                    Impact = "High impact on user success and satisfaction",
                    Effort = "High",
                    Timeline = "Immediate"
                });
            }

            foreach (var issue in issuesBySeverity.GetValueOrDefault(IssueSeverity.Major, new List<UsabilityIssue>()))
            {
                recommendations.Add(new Recommendation
                {
                    Priority = Priority.High,
                    Description = $"Resolve major usability issue: {issue.Description}",
                    Impact = "Significant impact on user experience",
                    Effort = "Medium",
                    Timeline = "Short-term"
                });
            }

            return recommendations.OrderByDescending(r => r.Priority).ToList();
        }
    }

    public class UsabilityTestSession
    {
        public Guid SessionId { get; set; }
        public TestParticipant Participant { get; set; }
        public string TestScenario { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<UsabilityTask> Tasks { get; set; }
        public List<Observation> Observations { get; set; }
        public UsabilityMetrics Metrics { get; set; }
        public string SessionNotes { get; set; }
    }

    public class TestParticipant
    {
        public string ParticipantId { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string AgeGroup => Age switch
        {
            < 25 => "18-24",
            < 35 => "25-34",
            < 45 => "35-44",
            < 60 => "45-59",
            _ => "60+"
        };
        public string UserType { get; set; } // Volunteer, Donor, Admin, Victim
        public string TechProficiency { get; set; } // Beginner, Intermediate, Advanced
        public string Email { get; set; }
        public bool HasDisasterExperience { get; set; }
    }

    public class UsabilityTask
    {
        public string TaskId { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double CompletionTimeSeconds => EndTime.HasValue ? (EndTime.Value - StartTime).TotalSeconds : 0;
        public bool CompletedSuccessfully { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public int AssistanceCount { get; set; }
        public int SatisfactionRating { get; set; } // 1-5 scale
        public string UserComments { get; set; }
    }

    public class Observation
    {
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
        public string Context { get; set; }
        public bool IsIssue { get; set; }
        public IssueSeverity Severity { get; set; }
        public string Category { get; set; } // Navigation, Form, Content, Accessibility, Performance
    }

    public class UsabilityMetrics
    {
        public double SystemUsabilityScaleScore { get; set; }
        public int? NetPromoterScore { get; set; }
        public double TaskSuccessRate { get; set; }
        public double AverageTaskTime { get; set; }
        public double TaskSatisfactionRating { get; set; }
        public int TotalIssuesIdentified { get; set; }
    }

    public enum IssueSeverity
    {
        Critical,   // Prevents task completion
        Major,      // Significant difficulty but task can be completed
        Minor,      // Minor inconvenience
        Cosmetic    // Visual issue, no functional impact
    }

    public class UsabilityTestReport
    {
        public DateTime GeneratedAt { get; set; }
        public int TotalSessions { get; set; }
        public ParticipantDemographics ParticipantDemographics { get; set; }
        public TaskPerformanceAnalysis TaskAnalysis { get; set; }
        public SatisfactionScores SatisfactionScores { get; set; }
        public Dictionary<IssueSeverity, List<UsabilityIssue>> IssuesBySeverity { get; set; }
        public List<Recommendation> Recommendations { get; set; }
    }

    public class ParticipantDemographics
    {
        public int TotalParticipants { get; set; }
        public Dictionary<string, int> AgeGroups { get; set; }
        public Dictionary<string, int> TechProficiencyLevels { get; set; }
        public Dictionary<string, int> UserTypes { get; set; }
    }

    public class TaskPerformanceAnalysis
    {
        public double AverageCompletionTime { get; set; }
        public double SuccessRate { get; set; }
        public double AverageErrorCount { get; set; }
        public Dictionary<string, int> CommonPainPoints { get; set; }
    }

    public class SatisfactionScores
    {
        public double AverageSUS { get; set; }
        public double AverageNPS { get; set; }
        public double TaskSatisfaction { get; set; }
    }

    public class UsabilityIssue
    {
        public string Description { get; set; }
        public IssueSeverity Severity { get; set; }
        public string Context { get; set; }
        public int Frequency { get; set; }
    }

    public class Recommendation
    {
        public Priority Priority { get; set; }
        public string Description { get; set; }
        public string Impact { get; set; }
        public string Effort { get; set; }
        public string Timeline { get; set; }
    }

    public enum Priority
    {
        Critical,
        High,
        Medium,
        Low
    }
}
