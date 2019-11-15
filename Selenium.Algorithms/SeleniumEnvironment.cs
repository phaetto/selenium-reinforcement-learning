using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using Selenium.Algorithms.ReinforcementLearning;
using System.Collections.Generic;
using System.Linq;

namespace Selenium.Algorithms
{
    public class SeleniumEnvironment : Environment<IReadOnlyCollection<IWebElement>>
    {
        private readonly RemoteWebDriver webDriver;
        private readonly string url;
        private readonly string queryElementTarget;

        public SeleniumEnvironment(
            RemoteWebDriver webDriver,
            string url,
            string queryElementTarget
        )
        {
            this.webDriver = webDriver;
            this.url = url;
            this.queryElementTarget = queryElementTarget;
        }

        public override State<IReadOnlyCollection<IWebElement>> GetInitialState()
        {
            webDriver.Navigate().GoToUrl(url);
            return GetCurrentState();
        }

        public override IEnumerable<AgentAction<IReadOnlyCollection<IWebElement>>> GetPossibleActions(in State<IReadOnlyCollection<IWebElement>> state)
        {
            var seleniumState = state as SeleniumState;
            return seleniumState.ActionableElements.Select(x => new ElementClickAction(x, null)); // These actions will not run, only compared
        }

        public override double RewardFunction(in State<IReadOnlyCollection<IWebElement>> stateFrom, in AgentAction<IReadOnlyCollection<IWebElement>> action)
        {
            var target = webDriver.FindElementByCssSelector(queryElementTarget);
            if (target.Displayed && target.Enabled)
            {
                return 100;
            }

            return -1;
        }

        public override bool HasReachedAGoalState(in State<IReadOnlyCollection<IWebElement>> state)
        {
            var target = webDriver.FindElementByCssSelector(queryElementTarget);
            return target.Displayed && target.Enabled;
        }

        public State<IReadOnlyCollection<IWebElement>> GetCurrentState()
        {
            var actionableElements = GetActionableElements(webDriver);
            var elementList = new List<IWebElement>(actionableElements)
                {
                    webDriver.FindElementByCssSelector(queryElementTarget)
                };

            var actionableElementsWithTarget = elementList
                .Where(x => x.Displayed && x.Enabled);
            return new SeleniumState(actionableElementsWithTarget.ToList().AsReadOnly(), actionableElements);
        }

        protected virtual IReadOnlyCollection<IWebElement> GetActionableElements(RemoteWebDriver remoteWebDriver)
        {
            return remoteWebDriver.FindElementsByCssSelector("body *[data-actionable-item]");
        }
    }
}
