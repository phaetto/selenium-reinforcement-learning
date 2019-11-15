namespace SeleniumReinforcementLearning
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Remote;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    internal class Program
    {
        public static void Main(string[] args)
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("headless");
            chromeOptions.BinaryLocation = @"C:\Program Files (x86)\Chromium\Application\chrome.exe";
            chromeOptions.AddArgument("--log-level=3");
            chromeOptions.SetLoggingPreference(LogType.Browser, LogLevel.Warning);

            using (var driver = new ChromeDriver(@".\", chromeOptions))
            {
                try
                {
                    Console.WriteLine("\nLoading the environment...");
                    var random = new Random(1);
                    /**
                     * Try to create a code scenario for test.html/test.spec pair
                     **/
                    var seleniumEnvironment = new SeleniumEnvironment(driver, ".third");
                    var seleniumRandomStepPolicy = new SeleniumRandomStepPolicy(random);
                    var rlTrainer = new RLTrainer<IReadOnlyCollection<IWebElement>>(seleniumEnvironment, seleniumRandomStepPolicy);

                    // Execute
                    Console.WriteLine("Training...");
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    rlTrainer.Run(epochs: 10);
                    stopWatch.Stop();
                    Console.WriteLine($"\tDone training ({stopWatch.Elapsed.TotalSeconds} sec)");

                    Console.WriteLine("Walk to goal...");
                    var initialState = seleniumEnvironment.GetInitialState();
                    var path = rlTrainer.Walk(initialState, goalCondition: s => seleniumEnvironment.HasReachedAGoalState(s));

                    Console.WriteLine("To reach the goal you need to:");
                    foreach (var pair in path)
                    {
                        Console.WriteLine($"\t{pair.Action.ToString()}");
                    }
                }
                finally
                {
                    driver.Close();
                    driver.Quit();
                }

                Console.WriteLine("\nDone.");
                Console.ReadLine();
                Console.WriteLine("(Unloading...)");
            }
        }

        class SeleniumEnvironment : Environment<IReadOnlyCollection<IWebElement>>
        {
            private readonly RemoteWebDriver webDriver;
            private readonly string queryElementTarget;

            public SeleniumEnvironment(
                RemoteWebDriver webDriver,
                string queryElementTarget
            )
            {
                this.webDriver = webDriver;
                this.queryElementTarget = queryElementTarget;
            }

            public override State<IReadOnlyCollection<IWebElement>> GetInitialState()
            {
                webDriver.Navigate().GoToUrl("file:///C:/sources/SeleniumReinforcementLearning/SeleniumReinforcementLearning/bin/Debug/test.html");
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
                var actionableElements = webDriver.FindElementsByCssSelector("body *[data-actionable-item]");
                var elementList = new List<IWebElement>(actionableElements)
                {
                    webDriver.FindElementByCssSelector(queryElementTarget)
                };

                var actionableElementsWithTarget = elementList
                    .Where(x => x.Displayed && x.Enabled);
                return new SeleniumState(actionableElementsWithTarget.ToList().AsReadOnly(), actionableElements);
            }
        }

        class SeleniumRandomStepPolicy : Policy<IReadOnlyCollection<IWebElement>>
        {
            private readonly Random random;

            public SeleniumRandomStepPolicy(Random random)
            {
                this.random = random;
            }

            public override AgentAction<IReadOnlyCollection<IWebElement>> GetNextAction(in Environment<IReadOnlyCollection<IWebElement>> environment, in State<IReadOnlyCollection<IWebElement>> state)
            {
                var randomElement = state.Data.ElementAt(random.Next(0, state.Data.Count));
                return new ElementClickAction(randomElement, this);
            }
        }

        class ElementClickAction : AgentAction<IReadOnlyCollection<IWebElement>>
        {
            private readonly IWebElement webElement;
            private readonly Policy<IReadOnlyCollection<IWebElement>> policy;

            public readonly string CachedName;
            public readonly int CachedHash;

            public ElementClickAction(IWebElement webElement, Policy<IReadOnlyCollection<IWebElement>> policy)
            {
                this.webElement = webElement;
                this.policy = policy;

                // We have to cache those values because the element will get out of the DOM eventually
                CachedHash = webElement.ExtendedGetHashCode();
                CachedName = $"Click on {webElement.ExtendedToString()}";
            }

            public override bool Equals(object obj)
            {
                var otherAction = obj as ElementClickAction;

                return otherAction != null
                    && CachedHash == otherAction.CachedHash;
            }

            public override State<IReadOnlyCollection<IWebElement>> ExecuteAction(in Environment<IReadOnlyCollection<IWebElement>> environment, in State<IReadOnlyCollection<IWebElement>> state)
            {
                try
                {
                    webElement.Click();
                    return (environment as SeleniumEnvironment).GetCurrentState();
                }
                catch (Exception exception) // TODO: specific exceptions here
                {
                    // This action seems to be impossible to do
                    // We should ask policy for alternative action
                    var otherAction = policy.GetNextAction(environment, state); // TODO: do without deep recurse
                    return otherAction.ExecuteAction(environment, state);
                }
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

        /// <summary>
        /// State is defined as the actionable elements state plus the target(s) (?) elements state
        /// </summary>
        class SeleniumState : State<IReadOnlyCollection<IWebElement>>
        {
            public readonly string CachedName;
            public readonly int CachedHash;

            public SeleniumState(in IReadOnlyCollection<IWebElement> data, in IReadOnlyCollection<IWebElement> actionableElements) : base(data)
            {
                ActionableElements = actionableElements;

                // We have to cache those values because the elements will get out of the DOM eventually
                CachedHash = 13;
                foreach (var item in Data)
                {
                    CachedHash = (CachedHash * 7) + item.ExtendedGetHashCode();
                }

                CachedName = string.Join(", ", Data.Select(x => x.ExtendedToString()));
            }

            public IReadOnlyCollection<IWebElement> ActionableElements { get; }

            public override bool Equals(object obj)
            {
                var otherState = obj as SeleniumState;
                if (obj == null || Data.Count != otherState.Data.Count)
                {
                    return false;
                }

                for (var i = 0; i < Data.Count; ++i)
                {
                    if (CachedHash != otherState.CachedHash)
                    {
                        return false;
                    }
                }

                return true;
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

    public static class WebElementExtensions
    {
        public static bool ExtendedEquals(this IWebElement webElement, IWebElement otherWebElement)
        {
            // TODO: revise this comparison
            try
            {
                return webElement.Displayed == otherWebElement.Displayed
                    && webElement.Enabled == otherWebElement.Enabled
                    && webElement.Selected == otherWebElement.Selected
                    && webElement.TagName == otherWebElement.TagName
                    && webElement.Text == otherWebElement.Text
                    && webElement.GetAttribute("innerHTML") == otherWebElement.GetAttribute("innerHTML");
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        }

        public static int ExtendedGetHashCode(this IWebElement webElement)
        {
            var hash = 13;
            hash = (hash * 7) + webElement.Displayed.GetHashCode();
            hash = (hash * 7) + webElement.Enabled.GetHashCode();
            hash = (hash * 7) + webElement.Selected.GetHashCode();
            hash = (hash * 7) + webElement.TagName.GetHashCode();
            hash = (hash * 7) + webElement.Text.GetHashCode();
            hash = (hash * 7) + webElement.GetAttribute("innerHTML").GetHashCode();
            return hash;
        }

        public static string ExtendedToString(this IWebElement webElement)
        {
            var idString = webElement.GetAttribute("id");
            if (!string.IsNullOrEmpty(idString))
            {
                return $"{webElement.TagName}#{idString}";
            }

            var classString = webElement.GetAttribute("class");
            if (!string.IsNullOrEmpty(classString))
            {
                return $"{webElement.TagName}.{classString}";
            }

            return $"{webElement.TagName}";
        }
    }
}
