namespace Selenium.Algorithms
{
    using Selenium.Algorithms.ReinforcementLearning;
    using Selenium.Algorithms.ReinforcementLearning.Serialization;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(ExperimentStateConverterFactory))]
    public sealed class SeleniumExperimentState : IExperimentState<IReadOnlyCollection<ElementData>>
    {
        public IDictionary<StateAndActionPair<IReadOnlyCollection<ElementData>>, double> QualityMatrix { get; } = new Dictionary<StateAndActionPair<IReadOnlyCollection<ElementData>>, double>();
    }
}
