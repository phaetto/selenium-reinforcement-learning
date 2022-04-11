namespace Selenium.Algorithms.ReinforcementLearning
{
    public readonly struct TrainerReport
    {
        public long TimesReachedGoal { get; }
        public long TotalActionsRun { get; }
        public long StabilizationWaitCount { get; }
        public long Epochs { get; }

        public double AverageActionsPerRun => TotalActionsRun / (double)Epochs;

        public TrainerReport(long timesReachedGoal, long totalActionsRun, long stabilizationWaitCount, long epochs)
        {
            TimesReachedGoal = timesReachedGoal;
            TotalActionsRun = totalActionsRun;
            StabilizationWaitCount = stabilizationWaitCount;
            Epochs = epochs;
        }

        public static TrainerReport operator +(TrainerReport lhs, TrainerReport rhs)
        {
            return new TrainerReport(
                lhs.TimesReachedGoal + rhs.TimesReachedGoal,
                lhs.TotalActionsRun + rhs.TotalActionsRun,
                lhs.StabilizationWaitCount + rhs.StabilizationWaitCount,
                lhs.Epochs + rhs.Epochs
            );
        }
    }
}
