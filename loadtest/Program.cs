namespace GiftOfTheGivers2.loadtest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var runner = new LoadTestRunner();

            if (args.Length == 0)
            {
                // Run comprehensive test suite
                await runner.RunComprehensiveLoadTest();
            }
            else
            {
                // Run specific scenario
                var scenario = args[0];
                var users = args.Length > 1 ? int.Parse(args[1]) : 50;
                var duration = args.Length > 2 ? int.Parse(args[2]) : 60;

                await runner.RunSpecificScenario(scenario, users, duration);
            }
        }
    }
}
