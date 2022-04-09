namespace Selenium.Algorithms.ReinforcementLearning
{
    using Selenium.Algorithms.ReinforcementLearning.Serialization;
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(ExperimentConverterFactory))]
    public sealed class Experiment<TData>
    {
        public Experiment(IEnvironment<TData> environment, ITrainGoal<TData> trainGoal, IExperimentState<TData> experimentState)
        {
            Environment = environment;
            TrainGoal = trainGoal;
            ExperimentState = experimentState;
        }

        public IEnvironment<TData> Environment { get; }
        public ITrainGoal<TData> TrainGoal { get; }
        public IExperimentState<TData> ExperimentState { get; }

        public ExperimentDependency<TData> ToDependency(int maxSteps)
        {
            return new ExperimentDependency<TData>(ExperimentState, Environment, TrainGoal, maxSteps);
        }
    }
}
