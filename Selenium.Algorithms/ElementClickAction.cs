namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using Selenium.Algorithms.ReinforcementLearning;
    using Selenium.Algorithms.Serialization;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    [JsonConverter(typeof(ElementClickActionConverter))]
    public sealed class ElementClickAction : IAgentAction<IReadOnlyCollection<ElementData>>
    {
        public readonly ElementData WebElement;
        public readonly string CachedName;
        public readonly int CachedHash;

        public ElementClickAction(in ElementData webElement)
        {
            this.WebElement = webElement;

            // We have to cache those values because the element will get out of the DOM eventually
            CachedHash = webElement.ExtendedGetHashCode();
            CachedName = $"Click on {webElement.GetQuery().Query}";
        }

        public async Task<IState<IReadOnlyCollection<ElementData>>> ExecuteAction(IEnvironment<IReadOnlyCollection<ElementData>> environment, IState<IReadOnlyCollection<ElementData>> state)
        {
            var seleniumEnvironment = (SeleniumEnvironment)environment;
            try
            {
                seleniumEnvironment.Options.WriteLine($"\t{CachedName}");
                WebElement.WebElementReference.Click();
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
