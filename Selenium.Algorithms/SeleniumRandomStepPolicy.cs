using OpenQA.Selenium;
using Selenium.Algorithms.ReinforcementLearning;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Selenium.Algorithms
{
    public class SeleniumRandomStepPolicy : Policy<IReadOnlyCollection<IWebElement>>
    {
        private readonly Random random;

        public SeleniumRandomStepPolicy(Random random)
        {
            this.random = random;
        }

        public override AgentAction<IReadOnlyCollection<IWebElement>> GetNextAction(in Environment<IReadOnlyCollection<IWebElement>> environment, in State<IReadOnlyCollection<IWebElement>> state)
        {
            var randomElement = state.Data.ElementAt(random.Next(0, state.Data.Count));
            return new ElementClickAction(randomElement, this);
        }
    }
}
