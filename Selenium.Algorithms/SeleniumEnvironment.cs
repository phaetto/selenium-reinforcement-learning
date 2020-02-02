namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Remote;
    using Selenium.Algorithms.ReinforcementLearning;
    using Selenium.Algorithms.ReinforcementLearning.Repetitions;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class SeleniumEnvironment : Environment<IReadOnlyCollection<ElementData>>
    {
        private readonly RemoteWebDriver webDriver;
        private readonly IReadOnlyCollection<string> DefaultCssSelectors = new string[] { "body *[data-automation-id]" };

        public SeleniumEnvironmentOptions SeleniumEnvironmentOptions { get; }

        public SeleniumEnvironment(
            RemoteWebDriver webDriver,
            SeleniumEnvironmentOptions seleniumEnvironmentOptions
        )
        {
            this.webDriver = webDriver;
            SeleniumEnvironmentOptions = seleniumEnvironmentOptions;

            // Setup webdriver training defaults
            this.webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1);
            this.webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(300);
        }

        public override async Task<State<IReadOnlyCollection<ElementData>>> GetInitialState()
        {
            webDriver.Navigate().GoToUrl(SeleniumEnvironmentOptions.Url);
            return await GetCurrentState();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<IEnumerable<AgentAction<IReadOnlyCollection<ElementData>>>> GetPossibleActions(State<IReadOnlyCollection<ElementData>> state)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // We need to get the fresh page's state instead of using the input
            var seleniumState = state; // GetCurrentState(); // TODO: Check if we need this

            Debug.Assert(state.Data.Count > 0, $"A state reached {nameof(SeleniumEnvironment)} that has no data");

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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<State<IReadOnlyCollection<ElementData>>> GetCurrentState()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<bool> IsIntermediateState(State<IReadOnlyCollection<ElementData>> state)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return state.Data.Count == 0;
        }

        public override async Task WaitForPostActionIntermediateStabilization(RepetitionContext repetitionContext)
        {
            State<IReadOnlyCollection<ElementData>> state;
            do
            {
                await Task.Delay(300);
                state = await GetCurrentState();
            }
            while (state.Data.Count == 0 && repetitionContext.Step());
        }

        protected virtual IReadOnlyCollection<string> GetActionableElementsQuerySelectors()
        {
            return DefaultCssSelectors;
        }

        protected virtual ElementTypeAction GetElementTypeAction(ElementData elementData, State<IReadOnlyCollection<ElementData>> state)
        {
            if (!SeleniumEnvironmentOptions.InputTextData.Any())
            {
                throw new InvalidOperationException("No data has been provided for this environment");
            }

            // TODO: make a more sofisticated 'the closest value to name'
            // Question: What happens if we have more that 1 good matches?
            var inputDataState = SeleniumEnvironmentOptions.InputTextData.ContainsKey(elementData.Name)
                ? SeleniumEnvironmentOptions.InputTextData[elementData.Name]
                : "todo: random string to provide";

            if (state.Data.Any(x => x.ExtraState == inputDataState)) // Should have ElementData.Equals
            {
                return ElementTypeAction.NoTypeAction;
            }

            return new ElementTypeAction(elementData, inputDataState);
        }
    }
}
