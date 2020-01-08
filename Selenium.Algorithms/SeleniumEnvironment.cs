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
        private readonly RemoteWebDriver webDriver;
        private readonly string url;
        private readonly IReadOnlyDictionary<string, string> inputTextData;
        private readonly Func<RemoteWebDriver, State<IReadOnlyCollection<ElementData>>, bool> hasReachedGoalCondition;
        private readonly IReadOnlyCollection<string> DefaultCssSelectors = new string[] { "body *[data-automation-id]" };

        public SeleniumEnvironment(
            RemoteWebDriver webDriver,
            string url,
            IReadOnlyDictionary<string, string> inputTextData = null,
            Func<RemoteWebDriver, State<IReadOnlyCollection<ElementData>>, bool> hasReachedGoalCondition = null
        )
        {
            this.webDriver = webDriver;
            this.url = url;
            this.inputTextData = inputTextData;
            this.hasReachedGoalCondition = hasReachedGoalCondition;

            // Setup webdriver training defaults
            this.webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1);
            this.webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(300);
        }

        public override Task<State<IReadOnlyCollection<ElementData>>> GetInitialState()
        {
            webDriver.Navigate().GoToUrl(url);
            return Task.FromResult(GetCurrentState());
        }

        public override async Task<IEnumerable<AgentAction<IReadOnlyCollection<ElementData>>>> GetPossibleActions(State<IReadOnlyCollection<ElementData>> state)
        {
            // We need to get the fresh page's state instead of using the input
            var seleniumState = GetCurrentState();

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
                .Where(x => x != null);
        }

        public override async Task<double> RewardFunction(State<IReadOnlyCollection<ElementData>> stateFrom, AgentAction<IReadOnlyCollection<ElementData>> action)
        {
            if (await HasReachedAGoalCondition(stateFrom, action))
            {
                return 100;
            }

            return -1;
        }

        public override async Task<bool> HasReachedAGoalCondition(State<IReadOnlyCollection<ElementData>> state, AgentAction<IReadOnlyCollection<ElementData>> action)
        {
            return hasReachedGoalCondition(webDriver, state);
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
            if (inputTextData == null)
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
                return null;
            }

            return new ElementTypeAction(elementData, inputDataState);
        }
    }
}
