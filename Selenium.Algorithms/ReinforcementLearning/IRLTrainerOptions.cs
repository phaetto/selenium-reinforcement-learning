namespace Selenium.Algorithms.ReinforcementLearning
{
    public interface IRLTrainerOptions<TData>
    {
        double DiscountRate { get; }
        Environment<TData> Environment { get; }
        double LearningRate { get; }
        Policy<TData> Policy { get; }
        ITrainGoal<TData> TrainGoal { get; }
    }
}