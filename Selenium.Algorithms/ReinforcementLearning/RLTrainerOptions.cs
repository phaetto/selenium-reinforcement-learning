using System.Collections.Generic;

namespace Selenium.Algorithms.ReinforcementLearning
{
    public readonly struct RLTrainerOptions<TData> : IRLTrainerOptions<TData>
    {
        public RLTrainerOptions(
            in IEnvironment<TData> environment,
            in IPolicy<TData> policy,
            in IExperimentState<TData> experimentState,
            in ITrainGoal<TData> trainGoal,
            in IEnumerable<ExperimentDependency<TData>>? dependencies = null,
            in double learningRate = 0.5D,
            in double discountRate = 0.5D
        )
        {
            Environment = environment;
            Policy = policy;
            ExperimentState = experimentState;
            TrainGoal = trainGoal;
            Dependencies = dependencies ?? new ExperimentDependency<TData>[0];
            LearningRate = learningRate;
            DiscountRate = discountRate;
        }

        public IEnvironment<TData> Environment { get; }
        public IPolicy<TData> Policy { get; }
        public IExperimentState<TData> ExperimentState { get; }
        public ITrainGoal<TData> TrainGoal { get; }
        public IEnumerable<ExperimentDependency<TData>> Dependencies { get; }
        public double LearningRate { get; }
        public double DiscountRate { get; }
    }
}
