using System.Text.Json;

namespace GiftOfTheGivers2.UsabilityTesting
{
    public class SessionManager
    {
        private readonly UsabilityTestFramework _framework;
        private UsabilityTestSession _currentSession;

        public SessionManager(UsabilityTestFramework framework)
        {
            _framework = framework;
        }

        public UsabilityTestSession StartSession(TestParticipant participant, string scenario)
        {
            _currentSession = _framework.CreateSession(participant, scenario);
            Console.WriteLine($"Session started: {_currentSession.SessionId}");
            Console.WriteLine($"Participant: {participant.Name}");
            Console.WriteLine($"Scenario: {scenario}");

            return _currentSession;
        }

        public void StartTask(string taskId, string taskName, string description)
        {
            if (_currentSession == null)
                throw new InvalidOperationException("No active session");

            var task = new UsabilityTask
            {
                TaskId = taskId,
                TaskName = taskName,
                Description = description,
                StartTime = DateTime.Now
            };

            _currentSession.Tasks.Add(task);
            Console.WriteLine($"Task started: {taskName}");
        }

        public void CompleteTask(string taskId, bool success, List<string> errors = null, int assistanceCount = 0, int satisfaction = 3, string comments = "")
        {
            var task = _currentSession?.Tasks.FirstOrDefault(t => t.TaskId == taskId);
            if (task != null)
            {
                task.EndTime = DateTime.Now;
                task.CompletedSuccessfully = success;
                task.ErrorCount = errors?.Count ?? 0;
                task.Errors = errors ?? new List<string>();
                task.AssistanceCount = assistanceCount;
                task.SatisfactionRating = satisfaction;
                task.UserComments = comments;

                Console.WriteLine($"Task completed: {task.TaskName}");
                Console.WriteLine($"Success: {success}, Time: {task.CompletionTimeSeconds:F1}s, Errors: {task.ErrorCount}");
            }
        }

        public void RecordObservation(string description, string context, bool isIssue = false, IssueSeverity severity = IssueSeverity.Minor, string category = "General")
        {
            if (_currentSession == null) return;

            var observation = new Observation
            {
                Timestamp = DateTime.Now,
                Description = description,
                Context = context,
                IsIssue = isIssue,
                Severity = severity,
                Category = category
            };

            _currentSession.Observations.Add(observation);
            Console.WriteLine($"Observation recorded: {description}");
        }

        public void RecordThinkAloud(string comment, string context = "Think Aloud")
        {
            RecordObservation(comment, context, false, IssueSeverity.Cosmetic, "User Feedback");
        }

        public void RecordIssue(string description, string context, IssueSeverity severity, string category = "Usability")
        {
            RecordObservation(description, context, true, severity, category);
        }

        public async Task<double> ConductSUSSurvey(Dictionary<int, int> responses)
        {
            var sus = DataCollectionTools.CreateSUSQuestionnaire();
            var score = sus.CalculateScore(responses);

            _currentSession.Metrics.SystemUsabilityScaleScore = score;
            Console.WriteLine($"SUS Score: {score:F1}");

            return score;
        }

        public async Task<double> ConductNPSSurvey(int score)
        {
            var nps = DataCollectionTools.CreateNPSQuestionnaire();
            _currentSession.Metrics.NetPromoterScore = score;

            // For individual session, we just store the score
            // Aggregate calculation happens in report generation
            Console.WriteLine($"NPS Score: {score}");

            return score;
        }

        public void CalculateSessionMetrics()
        {
            if (_currentSession?.Tasks == null || !_currentSession.Tasks.Any()) return;

            var completedTasks = _currentSession.Tasks.Where(t => t.EndTime.HasValue).ToList();

            _currentSession.Metrics.TaskSuccessRate = (double)completedTasks.Count(t => t.CompletedSuccessfully) / completedTasks.Count * 100;
            _currentSession.Metrics.AverageTaskTime = completedTasks.Average(t => t.CompletionTimeSeconds);
            _currentSession.Metrics.TaskSatisfactionRating = completedTasks.Average(t => t.SatisfactionRating);
            _currentSession.Metrics.TotalIssuesIdentified = _currentSession.Observations.Count(o => o.IsIssue);

            Console.WriteLine($"Session Metrics Calculated:");
            Console.WriteLine($"- Success Rate: {_currentSession.Metrics.TaskSuccessRate:F1}%");
            Console.WriteLine($"- Avg Task Time: {_currentSession.Metrics.AverageTaskTime:F1}s");
            Console.WriteLine($"- Avg Satisfaction: {_currentSession.Metrics.TaskSatisfactionRating:F1}/5");
            Console.WriteLine($"- Total Issues: {_currentSession.Metrics.TotalIssuesIdentified}");
        }

        public async Task EndSession(string sessionNotes = "")
        {
            if (_currentSession == null) return;

            _currentSession.EndTime = DateTime.Now;
            _currentSession.SessionNotes = sessionNotes;

            CalculateSessionMetrics();
            await _framework.SaveSessionResults(_currentSession);

            Console.WriteLine($"Session ended: {_currentSession.SessionId}");
            Console.WriteLine($"Duration: {(_currentSession.EndTime.Value - _currentSession.StartTime).TotalMinutes:F1} minutes");

            _currentSession = null;
        }

        public void GenerateRealTimeFeedback()
        {
            if (_currentSession == null) return;

            var recentTasks = _currentSession.Tasks.Where(t => t.EndTime.HasValue)
                                                 .OrderByDescending(t => t.EndTime)
                                                 .Take(3)
                                                 .ToList();

            if (recentTasks.Any(t => !t.CompletedSuccessfully))
            {
                Console.WriteLine("⚠️  Recent task failures detected. Consider providing additional guidance.");
            }

            if (recentTasks.Any(t => t.CompletionTimeSeconds > 120)) // More than 2 minutes
            {
                Console.WriteLine("⏱️  Some tasks are taking longer than expected. Check for usability barriers.");
            }

            var highSeverityIssues = _currentSession.Observations
                .Where(o => o.IsIssue && o.Severity >= IssueSeverity.Major)
                .ToList();

            if (highSeverityIssues.Any())
            {
                Console.WriteLine("🚨 High severity issues identified:");
                foreach (var issue in highSeverityIssues.Take(3))
                {
                    Console.WriteLine($"   - {issue.Description}");
                }
            }
        }
    }
}

