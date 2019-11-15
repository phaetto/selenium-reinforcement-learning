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

    public static class WebDriverWaitExtensions
    {
        public static void UntilElementVisible(this WebDriverWait webDriverWait, By selector)
        {
            webDriverWait.Until(x =>
            {
                try
                {
                    var element = x.FindElement(selector);
                    return (element?.Displayed ?? false)
                        && (element?.Enabled ?? false);
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });
        }
    }
}
