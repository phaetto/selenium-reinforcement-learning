namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ElementClickAction : AgentAction<IReadOnlyCollection<IWebElement>>
    {
        private readonly IWebElement webElement;

        public readonly string CachedName;
        public readonly int CachedHash;

        public ElementClickAction(IWebElement webElement)
        {
            this.webElement = webElement;

            // We have to cache those values because the element will get out of the DOM eventually
            CachedHash = webElement.ExtendedGetHashCode();
            CachedName = $"Click on {webElement.ExtendedToString()}";
        }

        public override bool Equals(object obj)
        {
            return obj is ElementClickAction otherAction
                && CachedHash == otherAction.CachedHash;
        }

        public override async Task<State<IReadOnlyCollection<IWebElement>>> ExecuteAction(Environment<IReadOnlyCollection<IWebElement>> environment, State<IReadOnlyCollection<IWebElement>> state)
        {
            try
            {
                Console.Write($"\t- clicking on {webElement.ExtendedToString()}");
                webElement.Click();
                Console.WriteLine($" ... done!");
            }
            catch (ElementNotInteractableException) // TODO: specific exceptions here
            {
                Console.WriteLine($"... failed: non-interactable");
                // Do not move at all if we cannot click, it should penalize it
            }
            catch (StaleElementReferenceException) // TODO: specific exceptions here
            {
                Console.WriteLine($"... failed: stale");
                // Do not move at all if we cannot click, it should penalize it
            }

            return (environment as SeleniumEnvironment).GetCurrentState();
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
