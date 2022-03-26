namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Collections.Generic;

    public interface IExperimentState<TData>
    {
        public IDictionary<StateAndActionPair<TData>, double> QualityMatrix { get; }

        public void Import() { // TODO: Implement deafult methods <3
            return;
        }
    }
}
