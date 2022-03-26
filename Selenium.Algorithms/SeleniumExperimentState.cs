namespace Selenium.Algorithms
{
    using Selenium.Algorithms.ReinforcementLearning;
    using System.Collections.Generic;

    public sealed class SeleniumExperimentState : IExperimentState<IReadOnlyCollection<ElementData>>
    {
        public IDictionary<StateAndActionPair<IReadOnlyCollection<ElementData>>, double> QualityMatrix { get; } = new Dictionary<StateAndActionPair<IReadOnlyCollection<ElementData>>, double>();
    }
}
