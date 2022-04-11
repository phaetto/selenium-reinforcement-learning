namespace Selenium.Algorithms
{
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed class SeleniumRandomStepPolicy : IPolicy<IReadOnlyCollection<ElementData>>
    {
        private readonly Random random;

        public SeleniumRandomStepPolicy(Random random)
        {
            this.random = random;
        }

        public async Task<IAgentAction<IReadOnlyCollection<ElementData>>> GetNextAction(
            IEnvironment<IReadOnlyCollection<ElementData>> environment,
            IState<IReadOnlyCollection<ElementData>> state,
            IExperimentState<IReadOnlyCollection<ElementData>> experimentState)
        {
            Debug.Assert(state.Data.Count > 0, $"A state reached {nameof(SeleniumRandomStepPolicy)} that has no data");

            var possibleActions = (await environment.GetPossibleActions(state)).ToList();

            if (possibleActions.Count == 0)
            {
                return new NoAction<IReadOnlyCollection<ElementData>>();
            }

            return possibleActions.Count > 1
                ? possibleActions.ElementAt(random.Next(0, possibleActions.Count))
                : possibleActions.First();
        }
    }
}
