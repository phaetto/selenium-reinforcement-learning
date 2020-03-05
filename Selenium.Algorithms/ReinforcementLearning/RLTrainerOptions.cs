namespace Selenium.Algorithms.ReinforcementLearning
{
    public class RLTrainerOptions<TData> : IRLTrainerOptions<TData>
    {
        public RLTrainerOptions(
            in IEnvironment<TData> environment,
            in IPolicy<TData> policy,
            in ITrainGoal<TData> trainGoal,
            in double learningRate = 0.5D,
            in double discountRate = 0.5D
        )
        {
            Environment = environment;
            Policy = policy;
            TrainGoal = trainGoal;
            LearningRate = learningRate;
            DiscountRate = discountRate;
        }

        public IEnvironment<TData> Environment { get; }
        public IPolicy<TData> Policy { get; }
        public ITrainGoal<TData> TrainGoal { get; }
        public double LearningRate { get; }
        public double DiscountRate { get; }
    }
}
