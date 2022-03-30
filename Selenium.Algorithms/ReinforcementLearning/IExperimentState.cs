namespace Selenium.Algorithms.ReinforcementLearning
{
    using Selenium.Algorithms.ReinforcementLearning.Serialization;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(ExperimentStateConverterFactory))]
    public interface IExperimentState<TData>
    {
        public IDictionary<StateAndActionPair<TData>, double> QualityMatrix { get; }
    }
}
