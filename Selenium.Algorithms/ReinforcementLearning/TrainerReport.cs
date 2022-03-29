namespace Selenium.Algorithms.ReinforcementLearning
{
    public readonly struct TrainerReport
    {
        public int TimesReachedGoal { get; }
        public float AverageActionsRun { get; }

        public TrainerReport(int timesReachedGoal, float averageActionsRun)
        {
            TimesReachedGoal = timesReachedGoal;
            AverageActionsRun = averageActionsRun;
        }
    }
}
