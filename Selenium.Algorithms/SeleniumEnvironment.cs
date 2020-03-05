namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class SeleniumEnvironment : IEnvironment<IReadOnlyCollection<ElementData>>
    {
        private readonly IWebDriver webDriver;
        private readonly IJavaScriptExecutor javaScriptExecutor;
        private readonly IReadOnlyCollection<string> DefaultCssSelectors = new string[] { "body *[data-automation-id]" };

        public ISeleniumEnvironmentOptions Options { get; }

        public SeleniumEnvironment(
            IWebDriver webDriver,
            IJavaScriptExecutor javaScriptExecutor,
            ISeleniumEnvironmentOptions seleniumEnvironmentOptions
        )
        {
            this.webDriver = webDriver;
            this.javaScriptExecutor = javaScriptExecutor;
            Options = seleniumEnvironmentOptions;

            // TODO: Add options: GetActionableElementsQuerySelectors / DefaultCssSelectors
            // TODO: Add options: GetInitialState

            // if (string.IsNullOrWhiteSpace(seleniumEnvironmentOptions.Url))  // TODO: guard?
        }

        public async Task<IState<IReadOnlyCollection<ElementData>>> GetInitialState()
        {
            // Setup webdriver training defaults
            this.webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1);
            this.webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(5);

            Options.WriteLine("SeleniumEnvironment: Getting the initial state...");
            webDriver.Navigate().GoToUrl(Options.Url);
            return await GetCurrentState();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IEnumerable<IAgentAction<IReadOnlyCollection<ElementData>>>> GetPossibleActions(IState<IReadOnlyCollection<ElementData>> state)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // We need to get the fresh page's state instead of using the input
            var seleniumState = state; // GetCurrentState(); // TODO: Check if we need this

            Debug.Assert(state.Data.Count > 0, $"A state reached {nameof(SeleniumEnvironment)} that has no data");

            return seleniumState.Data.Select(x =>
                    (IAgentAction<IReadOnlyCollection<ElementData>>)
                    (x.IsTypingElement switch
                    {
                        true => GetElementTypeAction(x, seleniumState),
                        _ => new ElementClickAction(x),
                    })
                )
                .Where(x => !Equals(x, ElementTypeAction.NoTypeAction));
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IState<IReadOnlyCollection<ElementData>>> GetCurrentState()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var actionableElementQuerySelectors = GetActionableElementsQuerySelectors();
            var actionableElements = actionableElementQuerySelectors.GetElementsFromQuerySelectors(javaScriptExecutor);
            var filteredActionableElements = actionableElements.ToInteractibleElements();
            var filteredElementsData = filteredActionableElements.GetElementsInformation();
            return new SeleniumState(filteredElementsData);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<bool> IsIntermediateState(IState<IReadOnlyCollection<ElementData>> state)
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

            var actionableElements = Options.LoadingElementsCssSelectors.GetElementsFromQuerySelectors(javaScriptExecutor);
            var areLoadingElementsVisible = actionableElements.IsAnyInteractibleElement();

            return areLoadingElementsVisible;
        }

        public async Task WaitForPostActionIntermediateStabilization()
        {
            await Task.Delay(300);
        }

        protected virtual IReadOnlyCollection<string> GetActionableElementsQuerySelectors()
        {
            return DefaultCssSelectors;
        }

        private ElementTypeAction GetElementTypeAction(ElementData elementData, IState<IReadOnlyCollection<ElementData>> state)
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
