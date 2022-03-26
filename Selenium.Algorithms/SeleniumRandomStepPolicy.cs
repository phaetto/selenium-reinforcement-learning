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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IAgentAction<IReadOnlyCollection<ElementData>>> GetNextAction(
            IEnvironment<IReadOnlyCollection<ElementData>> environment,
            IState<IReadOnlyCollection<ElementData>> state,
            IExperimentState<IReadOnlyCollection<ElementData>> experimentState)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // TODO: typing: when element is an input, type instead of click (remember it has to be able to be traced from the goal/reward)

            Debug.Assert(state.Data.Count > 0, $"A state reached {nameof(SeleniumRandomStepPolicy)} that has no data");

            return new ElementClickAction(state.Data.ElementAt(random.Next(0, state.Data.Count)));
        }
    }
}
