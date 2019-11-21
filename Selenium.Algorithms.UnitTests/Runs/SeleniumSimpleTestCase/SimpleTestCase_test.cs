namespace Selenium.Algorithms.UnitTests.Runs.SeleniumSimpleTestCase
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using Xunit;
    using Shouldly;
    using System.IO;
    using System.Threading.Tasks;

    public sealed class SimpleTestCase_test
    {
        [Fact]
        public async Task Run_WhenTrainingOnTestFile_ThenItSuccessfullyFindsTheCorrectActions()
        {
            // TODO: Needs configuration for build agent
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("headless");
            chromeOptions.BinaryLocation = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";// @"C:\Program Files (x86)\Chromium\Application\chrome.exe";
            chromeOptions.AddArgument("--log-level=3");
            chromeOptions.SetLoggingPreference(LogType.Browser, LogLevel.Warning);

            using (var driver = new ChromeDriver(@".\", chromeOptions))
            {
                try
                {
                    // TODO: need a base class with these stuff
                    var fileUri = new Uri(Path.GetFullPath($"{nameof(SimpleTestCase_test)}.html"));
                    var random = new Random(1);
                    var seleniumEnvironment = new SeleniumEnvironment(
                        driver,
                        fileUri.AbsoluteUri,
                        (driver, _) => {
                            var target = driver.FindElementByCssSelector(".third");
                            return target.Displayed && target.Enabled;
                        });
                    var seleniumRandomStepPolicy = new SeleniumRandomStepPolicy(random);
                    var rlTrainer = new RLTrainer<IReadOnlyCollection<IWebElement>>(seleniumEnvironment, seleniumRandomStepPolicy);

                    await rlTrainer.Run(epochs: 2);

                    var initialState = await seleniumEnvironment.GetInitialState();
                    var pathList = await rlTrainer.Walk(initialState, goalCondition: (s, a) => seleniumEnvironment.HasReachedAGoalCondition(s, a));

                    // TODO: verify state
                    pathList.ShouldNotBeEmpty();
                    pathList.Count.ShouldBe(3);
                    pathList[0].Action.ToString().ShouldEndWith("input[data-automation-id='first']");
                    pathList[1].Action.ToString().ShouldEndWith("input[data-automation-id='second']");
                    pathList[2].Action.ToString().ShouldEndWith("button[data-automation-id='third']");
                }
                finally
                {
                    driver.Close();
                    driver.Quit();
                }
            }
        }
    }
}
