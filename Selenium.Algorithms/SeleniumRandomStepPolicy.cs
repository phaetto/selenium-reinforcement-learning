namespace Selenium.Algorithms
{
    using OpenQA.Selenium.Remote;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class SeleniumRandomStepPolicy : Policy<IReadOnlyCollection<ElementData>>
    {
        private readonly Random random;

        public SeleniumRandomStepPolicy(Random random)
        {
            this.random = random;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<AgentAction<IReadOnlyCollection<ElementData>>> GetNextAction(Environment<IReadOnlyCollection<ElementData>> environment, State<IReadOnlyCollection<ElementData>> state)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // TODO: typing: when element is an input, type instead of click (remember it has to be able to be traced from the goal/reward)

            Debug.Assert(state.Data.Count > 0, $"A state reached {nameof(SeleniumRandomStepPolicy)} that has no data");

            return new ElementClickAction(state.Data.ElementAt(random.Next(0, state.Data.Count)));
        }
    }
}
