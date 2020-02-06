namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using Selenium.Algorithms.ReinforcementLearning;
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
            var seleniumEnvironment = (SeleniumEnvironment)environment;
            try
            {
                seleniumEnvironment.Options.WriteLine($"\t{CachedName}");
                webElement.WebElementReference.Click();
                seleniumEnvironment.Options.WriteLine($"\t\t... done!");
            }
            catch (ElementNotInteractableException)
            {
                seleniumEnvironment.Options.WriteLine($"\t\t... failed: non-interactable");
                // Do not move at all if we cannot click, it should penalize it
            }
            catch (StaleElementReferenceException)
            {
                seleniumEnvironment.Options.WriteLine($"\t\t... failed: stale");
                // Do not move at all if we cannot click, it should penalize it
            }

            return await environment.GetCurrentState();
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
