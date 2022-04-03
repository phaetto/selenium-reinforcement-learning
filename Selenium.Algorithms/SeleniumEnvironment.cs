namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using Selenium.Algorithms.Exceptions;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed class SeleniumEnvironment : IEnvironment<IReadOnlyCollection<ElementData>>
    {
        private readonly IWebDriver webDriver;
        private readonly IJavaScriptExecutor javaScriptExecutor;

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
        }

        public async Task<IState<IReadOnlyCollection<ElementData>>> GetInitialState()
        {
            Options.WriteLine("SeleniumEnvironment: Getting the initial state...");
            await Options.SetupInitialState(webDriver, Options);
            return await GetCurrentState();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IEnumerable<IAgentAction<IReadOnlyCollection<ElementData>>>> GetPossibleActions(IState<IReadOnlyCollection<ElementData>> state)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var seleniumState = state;

            if (state.Data.Count == 0)
            {
                throw new InvalidStateException("State cannot be empty. Make sure you are using the correct environment." +
                    "\n(This probably means that either options.LoadingElementsCssSelectors" +
                    " need to be provided or you missed elements in options.ActionableElementsCssSelectors)" +
                    $"\nUrl: {webDriver.Url}");
            }

            Debug.Assert(state.Data.Count > 0, $"A state reached {nameof(SeleniumEnvironment)} that has no data");

            return seleniumState.Data.Select(x =>
                    (IAgentAction<IReadOnlyCollection<ElementData>>)
                    (x.IsTypingElement switch
                    {
                        true => GetElementTypeAction(x, seleniumState),
                        _ => new ElementClickAction(x), // TODO: Should be able to choose the available actions and types [uses data-automation-actions]
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

        private IReadOnlyCollection<string> GetActionableElementsQuerySelectors()
        {
            return Options.ActionableElementsCssSelectors
                .Concat(Options.GoalElementSelectors)
                .ToList()
                .AsReadOnly();
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

            if (state.Data.Any(x => x.ExtraState == inputDataState))
            {
                return ElementTypeAction.NoTypeAction;
            }

            return new ElementTypeAction(elementData, inputDataState);
        }
    }
}
