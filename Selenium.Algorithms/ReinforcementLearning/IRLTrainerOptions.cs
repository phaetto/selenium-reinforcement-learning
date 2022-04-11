using System.Collections.Generic;

namespace Selenium.Algorithms.ReinforcementLearning
{
    public interface IRLTrainerOptions<TData>
    {
        double DiscountRate { get; }
        IEnvironment<TData> Environment { get; }
        double LearningRate { get; }
        IPolicy<TData> Policy { get; }
        ITrainGoal<TData> TrainGoal { get; }
        IExperimentState<TData> ExperimentState { get; }
        IEnumerable<ExperimentDependency<TData>> Dependencies { get; }
    }
}