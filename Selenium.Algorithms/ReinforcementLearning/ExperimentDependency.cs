namespace Selenium.Algorithms.ReinforcementLearning
{
    public readonly struct ExperimentDependency<TData>
    {
        public ExperimentDependency(
            IExperimentState<TData> experimentState,
            IEnvironment<TData> environment,
            ITrainGoal<TData> trainGoal,
            int maxSteps = 10)
        {
            ExperimentState = experimentState;
            Environment = environment;
            TrainGoal = trainGoal;
            MaxSteps = maxSteps;
        }

        public IExperimentState<TData> ExperimentState { get; }
        public IEnvironment<TData> Environment { get; }
        public ITrainGoal<TData> TrainGoal { get; }
        public int MaxSteps { get;}
    }
}