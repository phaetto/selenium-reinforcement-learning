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

        public SeleniumEnvironmentOptions Options { get; }

        public SeleniumEnvironment(
            RemoteWebDriver webDriver,
            SeleniumEnvironmentOptions seleniumEnvironmentOptions
        )
        {
            this.webDriver = webDriver;
            Options = seleniumEnvironmentOptions;

            // if (string.IsNullOrWhiteSpace(seleniumEnvironmentOptions.Url))  // TODO: guard?

            // Setup webdriver training defaults
            this.webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1);
            this.webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(5);
        }

        public override async Task<State<IReadOnlyCollection<ElementData>>> GetInitialState()
        {
            webDriver.Navigate().GoToUrl(Options.Url);
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
            var filteredActionableElements = actionableElements.ToInteractibleElements();
            var filteredElementsData = filteredActionableElements.GetElementsInformation();
            return new SeleniumState(filteredElementsData);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<bool> IsIntermediateState(State<IReadOnlyCollection<ElementData>> state)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (state.Data.Count == 0)
            {
                return true;
            }

            if (Options.LoadingElementsCssSelectors.Count == 0)
            {
                return false;
            }

            var actionableElements = Options.LoadingElementsCssSelectors.GetElementsFromQuerySelectors(webDriver);
            var areLoadingElementsVisible = actionableElements.IsAnyInteractibleElement();

            return areLoadingElementsVisible;
        }

        public override async Task WaitForPostActionIntermediateStabilization(RepetitionContext repetitionContext)
        {
            State<IReadOnlyCollection<ElementData>> state;
            do
            {
                await Task.Delay(300);
                state = await GetCurrentState();

                if (state.Data.Count > 0)
                {
                    if (Options.LoadingElementsCssSelectors.Count == 0)
                    {
                        break;
                    }

                    var actionableElements = Options.LoadingElementsCssSelectors.GetElementsFromQuerySelectors(webDriver);
                    var areLoadingElementsVisible = actionableElements.IsAnyInteractibleElement();

                    if (!areLoadingElementsVisible)
                    {
                        break;
                    }
                }
            }
            while (repetitionContext.Step());
        }

        protected virtual IReadOnlyCollection<string> GetActionableElementsQuerySelectors()
        {
            return DefaultCssSelectors;
        }

        protected virtual ElementTypeAction GetElementTypeAction(ElementData elementData, State<IReadOnlyCollection<ElementData>> state)
        {
            if (!Options.InputTextData.Any())
            {
                throw new InvalidOperationException("No data has been provided for this environment");
            }

            // TODO: make a more sofisticated 'the closest value to name'
            // Question: What happens if we have more that 1 good matches?
            var inputDataState = Options.InputTextData.ContainsKey(elementData.Name)
                ? Options.InputTextData[elementData.Name]
                : "todo: random string to provide";

            if (state.Data.Any(x => x.ExtraState == inputDataState)) // Should have ElementData.Equals
            {
                return ElementTypeAction.NoTypeAction;
            }

            return new ElementTypeAction(elementData, inputDataState);
        }
    }
}
