namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class SeleniumRandomStepPolicy : Policy<IReadOnlyCollection<IWebElement>>
    {
        private readonly Random random;

        public SeleniumRandomStepPolicy(Random random)
        {
            this.random = random;
        }

        public override async Task<AgentAction<IReadOnlyCollection<IWebElement>>> GetNextAction(Environment<IReadOnlyCollection<IWebElement>> environment, State<IReadOnlyCollection<IWebElement>> state)
        {
            // TODO: typing: when element is an input, type instead of click (remember it has to be able to be traced from the goal/reward)

            // Intermediate states (e.g. loading): when you have a state with no elements, create a wait-for-time-action
            if (state.Data.Count == 0)
            {
                return new WaitAction(random.Next(100, 1000));
            }

            return new ElementClickAction(state.Data.ElementAt(random.Next(0, state.Data.Count)));
        }
    }
}
