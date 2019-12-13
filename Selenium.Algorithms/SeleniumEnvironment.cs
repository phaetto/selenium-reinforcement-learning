namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Remote;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class SeleniumEnvironment : Environment<IReadOnlyCollection<IWebElement>>
    {
        private readonly RemoteWebDriver webDriver;
        private readonly string url;
        private readonly Func<RemoteWebDriver, State<IReadOnlyCollection<IWebElement>>, bool> hasReachedGoalCondition;

        public SeleniumEnvironment(
            RemoteWebDriver webDriver,
            string url,
            Func<RemoteWebDriver, State<IReadOnlyCollection<IWebElement>>, bool> hasReachedGoalCondition
        )
        {
            this.webDriver = webDriver;
            this.url = url;
            this.hasReachedGoalCondition = hasReachedGoalCondition;

            // Setup webdriver training defaults
            this.webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1);
            this.webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(300);
        }

        public override Task<State<IReadOnlyCollection<IWebElement>>> GetInitialState()
        {
            webDriver.Navigate().GoToUrl(url);
            return Task.FromResult(GetCurrentState());
        }

        public override async Task<IEnumerable<AgentAction<IReadOnlyCollection<IWebElement>>>> GetPossibleActions(State<IReadOnlyCollection<IWebElement>> state)
        {
            //var seleniumState = state as SeleniumState;
            var seleniumState = GetCurrentState();

            if (seleniumState.Data.Count == 0)
            {
                return new [] { new WaitAction(300) };
            }

            return seleniumState.Data.Select(x => new ElementClickAction(x));
        }

        public override async Task<double> RewardFunction(State<IReadOnlyCollection<IWebElement>> stateFrom, AgentAction<IReadOnlyCollection<IWebElement>> action)
        {
            if (await HasReachedAGoalCondition(stateFrom, action))
            {
                return 100;
            }

            return -1;
        }

        public override async Task<bool> HasReachedAGoalCondition(State<IReadOnlyCollection<IWebElement>> state, AgentAction<IReadOnlyCollection<IWebElement>> action)
        {
            return hasReachedGoalCondition(webDriver, state);
        }

        public State<IReadOnlyCollection<IWebElement>> GetCurrentState()
        {
            var actionableElements = GetActionableElements();
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

#if DEBUG
            // Debug info
            var allElementsData = elementList.GetElementsInformation().ToArray();
            var elementsData = filteredActionableElements.GetElementsInformation().ToArray();
#endif

            return new SeleniumState(filteredActionableElements);
        }

        protected virtual IReadOnlyCollection<IWebElement> GetActionableElements()
        {
            return webDriver.FindElementsByCssSelector("body *[data-automation-id]");
        }
    }
}
