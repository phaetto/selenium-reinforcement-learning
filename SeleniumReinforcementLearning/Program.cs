namespace SeleniumReinforcementLearning
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Remote;
    using OpenQA.Selenium.Support.UI;
    using Selenium.Algorithms;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Threading.Tasks;

    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("headless");
            chromeOptions.BinaryLocation = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";// @"C:\Program Files (x86)\Chromium\Application\chrome.exe";
            chromeOptions.AddArgument("--log-level=3");
            chromeOptions.SetLoggingPreference(LogType.Browser, LogLevel.Warning);

            using (var driver = new ChromeDriver(@".\", chromeOptions))
            {
                try
                {
                    Console.WriteLine("\nLoading the environment...");
                    driver.Manage().Window.Size = new Size(1000, 768);
                    var random = new Random(1);
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                    driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(300);

                    // Login to the site, so we don't have to 'discover' this (for now)
                    //Console.WriteLine("Login in for the first time...");
                    //driver.Navigate().GoToUrl("https://intraactiveamatest.sharepoint.com/sites/MessagesRLUITest");
                    //wait.UntilElementVisible(By.CssSelector("input[name='loginfmt'][type='email']"));

                    //var emailInput = driver.FindElement(By.CssSelector("input[name='loginfmt'][type='email']"));
                    //emailInput.SendKeys("ama@intraactiveamatest.onmicrosoft.com");
                    //wait.UntilElementVisible(By.CssSelector("input[type='submit']"));
                    //driver.FindElement(By.CssSelector("input[type='submit']")).Click();

                    //wait.UntilElementVisible(By.CssSelector("input[name='passwd'][type='password']"));
                    //var passwordInput = driver.FindElement(By.CssSelector("input[name='passwd'][type='password']"));
                    //passwordInput.SendKeys("qaz123WSX!@#$");
                    //wait.UntilElementVisible(By.CssSelector("input[type='submit']"));
                    //driver.FindElement(By.CssSelector("input[type='submit']")).Click();

                    // Start training
                    var seleniumEnvironment = new IntraActiveAmaTestSeleniumEnvironment(
                        driver,
                        "https://intraactive-sdk-ui.azurewebsites.net/");
                    var seleniumRandomStepPolicy = new SeleniumRandomStepPolicy(random);
                    var rlTrainer = new RLTrainer<IReadOnlyCollection<IWebElement>>(seleniumEnvironment, seleniumRandomStepPolicy);

                    // Execute
                    Console.WriteLine("Training...");
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    await rlTrainer.Run(epochs: 5);
                    stopWatch.Stop();
                    Console.WriteLine($"\tDone training ({stopWatch.Elapsed.TotalSeconds} sec)");

                    Console.WriteLine("Walk to goal...");
                    var initialState = await seleniumEnvironment.GetInitialState();
                    var path = await rlTrainer.Walk(initialState, goalCondition: async (s, a) => await seleniumEnvironment.HasReachedAGoalCondition(s, a));

                    Console.WriteLine("To reach the goal you need to:");
                    foreach (var pair in path)
                    {
                        Console.WriteLine($"\t from {pair.State.ToString()}");
                        Console.WriteLine($"\t\t{pair.Action.ToString()}");
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error:");
                    Console.WriteLine(exception.Message);
                    Console.WriteLine();
                    Console.WriteLine(exception.StackTrace);
                }
                finally
                {
                    Console.WriteLine("\nClosing...");
                    driver.Close();
                    driver.Quit();
                }

                Console.WriteLine("\nDone.");
                Console.ReadLine();
                Console.WriteLine("(Unloading...)");
            }
        }

        private class IntraActiveAmaTestSeleniumEnvironment : SeleniumEnvironment
        {
            private readonly WebDriverWait wait;
            private readonly RemoteWebDriver webDriver;
            private readonly string url;

            public IntraActiveAmaTestSeleniumEnvironment(RemoteWebDriver webDriver, string url) : base(webDriver, url, null)
            {
                this.wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
                this.webDriver = webDriver;
                this.url = url;
            }
            public override async Task<State<IReadOnlyCollection<IWebElement>>> GetInitialState()
            {
                webDriver.Navigate().GoToUrl(url);
                // wait.UntilUrl(url);

                wait.UntilElementVisible(By.CssSelector("div[class^='subMenuItem_']:nth-child(15)"));
                webDriver.FindElementByCssSelector("div[class^='subMenuItem_']:nth-child(15)").Click();

                wait.UntilElementVisible(By.CssSelector("div[class^='IA_pivotItem_']:nth-child(3)"));
                webDriver.FindElementByCssSelector("div[class^='IA_pivotItem_']:nth-child(3)").Click();

                return GetCurrentState();
            }
            public override async Task<bool> HasReachedAGoalCondition(State<IReadOnlyCollection<IWebElement>> state, AgentAction<IReadOnlyCollection<IWebElement>> action)
            {
                // *[data-automation-id='messages-edit-panel-save']
                // ExternalClass8514166CB2C745279663A372003C929D
                var goal = webDriver.FindElementsByCssSelector(".ExternalClass8514166CB2C745279663A372003C929D")  
                    .Where(x => x.Enabled && x.Displayed);

                return goal.Count() > 0;
            }
        }
    }

    public static class WebDriverWaitExtensions
    {
        public static void UntilLocalStorageIsUpdated(this WebDriverWait webDriverWait)
        {
            webDriverWait.Until(x =>
            {
                try
                {
                    var s = (string) ((RemoteWebDriver)x).ExecuteScript("return window.localStorage.getItem('navigator-1')");
                    return !string.IsNullOrWhiteSpace(s);
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        public static void UntilElementVisible(this WebDriverWait webDriverWait, By selector)
        {
            webDriverWait.Until(x =>
            {
                try
                {
                    var element = x.FindElement(selector);
                    return element.CanBeInteracted(x);
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }

        public static void UntilUrl(this WebDriverWait webDriverWait, string url)
        {
            webDriverWait.Until(x =>
            {
                try
                {
                    return x.Url == url;
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }
    }
}
