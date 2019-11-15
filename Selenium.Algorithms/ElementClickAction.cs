using OpenQA.Selenium;
using Selenium.Algorithms.ReinforcementLearning;
using System;
using System.Collections.Generic;

namespace Selenium.Algorithms
{
    public class ElementClickAction : AgentAction<IReadOnlyCollection<IWebElement>>
    {
        private readonly IWebElement webElement;
        private readonly Policy<IReadOnlyCollection<IWebElement>> policy;

        public readonly string CachedName;
        public readonly int CachedHash;

        public ElementClickAction(IWebElement webElement, Policy<IReadOnlyCollection<IWebElement>> policy)
        {
            this.webElement = webElement;
            this.policy = policy;

            // We have to cache those values because the element will get out of the DOM eventually
            CachedHash = webElement.ExtendedGetHashCode();
            CachedName = $"Click on {webElement.ExtendedToString()}";
        }

        public override bool Equals(object obj)
        {
            var otherAction = obj as ElementClickAction;

            return otherAction != null
                && CachedHash == otherAction.CachedHash;
        }

        public override State<IReadOnlyCollection<IWebElement>> ExecuteAction(in Environment<IReadOnlyCollection<IWebElement>> environment, in State<IReadOnlyCollection<IWebElement>> state)
        {
            try
            {
                webElement.Click();
                return (environment as SeleniumEnvironment).GetCurrentState();
            }
            catch (Exception exception) // TODO: specific exceptions here
            {
                // This action seems to be impossible to do
                // We should ask policy for alternative action
                var otherAction = policy.GetNextAction(environment, state); // TODO: do without deep recurse
                return otherAction.ExecuteAction(environment, state);
            }
        }

        public override int GetHashCode()
        {
            return CachedHash;
        }

        public override string ToString()
        {
            return CachedName;
        }
    }
}
