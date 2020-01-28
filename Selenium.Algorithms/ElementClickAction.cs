namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ElementClickAction : AgentAction<IReadOnlyCollection<ElementData>>
    {
        private readonly ElementData webElement;

        public readonly string CachedName;
        public readonly int CachedHash;

        public ElementClickAction(in ElementData webElement)
        {
            this.webElement = webElement;

            // We have to cache those values because the element will get out of the DOM eventually
            CachedHash = webElement.ExtendedGetHashCode();
            CachedName = $"Click on {webElement.GetQuery().Query}";
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<State<IReadOnlyCollection<ElementData>>> ExecuteAction(Environment<IReadOnlyCollection<ElementData>> environment, State<IReadOnlyCollection<ElementData>> state)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            try
            {
                Console.Write($"\t- clicking on {CachedName}");
                webElement.WebElementReference.Click();
                Console.WriteLine($" ... done!");
            }
            catch (ElementNotInteractableException)
            {
                Console.WriteLine($"... failed: non-interactable");
                // Do not move at all if we cannot click, it should penalize it
            }
            catch (StaleElementReferenceException)
            {
                Console.WriteLine($"... failed: stale");
                // Do not move at all if we cannot click, it should penalize it
            }

            return ((SeleniumEnvironment)environment).GetCurrentState();
        }

        public override bool Equals(object obj)
        {
            return obj is ElementClickAction otherAction
                && CachedHash == otherAction.CachedHash;
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
