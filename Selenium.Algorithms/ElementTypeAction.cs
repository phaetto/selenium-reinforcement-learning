namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using Selenium.Algorithms.ReinforcementLearning;
    using Selenium.Algorithms.Serialization;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    [JsonConverter(typeof(ElementTypeActionConverter))]
    public sealed class ElementTypeAction : IAgentAction<IReadOnlyCollection<ElementData>>, IAgentActionForElement
    {
        public static readonly ElementTypeAction NoTypeAction = new ElementTypeAction();
        public readonly string Text;
        public readonly string CachedName;
        public readonly int CachedHash;
        public ElementData ElementData { get; private set; }

        public ElementTypeAction(in ElementData webElement, in string text)
        {
            ElementData = webElement;
            Text = text;

            // We have to cache those values because the element will get out of the DOM eventually
            CachedHash = webElement.ExtendedGetHashCode();
            CachedHash = (CachedHash * 7) + text.GetHashCode();
            CachedName = $"Type '{text}' on {webElement.GetQuery().Query}";
        }

        private ElementTypeAction()
        {
            Text = "No type action";
            CachedName = string.Empty;
            CachedHash = 0;
        }

        public async Task<IState<IReadOnlyCollection<ElementData>>> ExecuteAction(IEnvironment<IReadOnlyCollection<ElementData>> environment, IState<IReadOnlyCollection<ElementData>> state)
        {
            var seleniumEnvironment = (SeleniumEnvironment)environment;
            try
            {
                seleniumEnvironment.Options.WriteLine($"\t{CachedName}");
                ElementData.WebElementReference.Clear();
                ElementData.WebElementReference.SendKeys(Text);
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
            return obj is ElementTypeAction otherAction
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
