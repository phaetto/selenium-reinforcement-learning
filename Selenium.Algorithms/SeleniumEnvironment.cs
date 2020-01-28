namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Remote;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class SeleniumEnvironment : Environment<IReadOnlyCollection<ElementData>>
    {
        public static readonly IReadOnlyDictionary<string, string> NoInputData = new Dictionary<string, string>();
        private readonly RemoteWebDriver webDriver;
        private readonly string url;
        private readonly IReadOnlyDictionary<string, string> inputTextData;
        private readonly IReadOnlyCollection<string> DefaultCssSelectors = new string[] { "body *[data-automation-id]" };

        public SeleniumEnvironment(
            RemoteWebDriver webDriver,
            string url,
            IReadOnlyDictionary<string, string> inputTextData
        )
        {
            this.webDriver = webDriver;
            this.url = url;
            this.inputTextData = inputTextData;

            // Setup webdriver training defaults
            this.webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1);
            this.webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(300);
        }

        public SeleniumEnvironment(
            RemoteWebDriver webDriver,
            string url
        ) : this(webDriver, url, new Dictionary<string, string>())
        {
        }

        public override Task<State<IReadOnlyCollection<ElementData>>> GetInitialState()
        {
            webDriver.Navigate().GoToUrl(url);
            return Task.FromResult(GetCurrentState());
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<IEnumerable<AgentAction<IReadOnlyCollection<ElementData>>>> GetPossibleActions(State<IReadOnlyCollection<ElementData>> state)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // We need to get the fresh page's state instead of using the input
            var seleniumState = state; // GetCurrentState(); // TODO: Check if we need this

            if (seleniumState.Data.Count == 0)
            {
                return new[] { new WaitAction(300) };
            }

            return seleniumState.Data.Select(x =>
                    (AgentAction<IReadOnlyCollection<ElementData>>)
                    (x.IsTypingElement switch
                    {
                        true => GetElementTypeAction(x, seleniumState),
                        _ => new ElementClickAction(x),
                    })
                )
                .Where(x => !Equals(x, ElementTypeAction.NoTypeAction));
        }

        public State<IReadOnlyCollection<ElementData>> GetCurrentState()
        {
            var actionableElementQuerySelectors = GetActionableElementsQuerySelectors();
            var actionableElements = actionableElementQuerySelectors.GetElementsFromQuerySelectors(webDriver);
            var elementList = new List<IWebElement>(actionableElements);

            var filteredActionableElements = elementList
                .Where(x =>
                {
                    try
                    {
                        return x.CanBeInteracted();
                    }
                    catch (StaleElementReferenceException)
                    {
                        return false;
                    }
                })
                .ToList()
                .AsReadOnly();

            var filteredElementsData = filteredActionableElements.GetElementsInformation();

            return new SeleniumState(filteredElementsData);
        }

        protected virtual IReadOnlyCollection<string> GetActionableElementsQuerySelectors()
        {
            return DefaultCssSelectors;
        }

        protected virtual ElementTypeAction GetElementTypeAction(ElementData elementData, State<IReadOnlyCollection<ElementData>> state)
        {
            if (inputTextData == NoInputData)
            {
                throw new InvalidOperationException("No data has been provided for this environment");
            }

            // TODO: make a more sofisticated 'the closest value to name'
            // Question: What happens if we have more that 1 good matches?
            var inputDataState = inputTextData.ContainsKey(elementData.Name)
                ? inputTextData[elementData.Name]
                : "todo: random string to provide";

            if (state.Data.Any(x => x.ExtraState == inputDataState)) // Should have ElementData.Equals
            {
                return ElementTypeAction.NoTypeAction;
            }

            return new ElementTypeAction(elementData, inputDataState);
        }
    }
}
