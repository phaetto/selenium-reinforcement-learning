namespace Selenium.Algorithms.UnitTests.Runs
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
    using Selenium.Algorithms;
    using System.Drawing;

    public sealed class OutOfPortViewportButVisibleTests
    {
        [Fact]
        public async Task Run_WhenOutOfViewPortElementsExist_ThenItSuccessfullyFindsTheCorrectActions()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("headless");

            using (var driver = new ChromeDriver(@".\", chromeOptions))
            {
                driver.Manage().Window.Size = new Size(1000, 768);

                try
                {
                    var fileUri = new Uri(Path.GetFullPath($"{nameof(Run_WhenOutOfViewPortElementsExist_ThenItSuccessfullyFindsTheCorrectActions)}.html"));
                    var random = new Random(1);
                    var seleniumEnvironment = new SeleniumEnvironment(
                        driver,
                        fileUri.AbsoluteUri,
                        (driver, _) =>
                        {
                            var target = driver.FindElementByCssSelector(".third-panel");
                            return target.Displayed && target.Enabled;
                        });
                    var seleniumRandomStepPolicy = new SeleniumQLearningStepPolicy(random);
                    var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumRandomStepPolicy);

                    await rlTrainer.Run(epochs: 2, maximumActions: 20);

                    var initialState = await seleniumEnvironment.GetInitialState();
                    var pathList = await rlTrainer.Walk(initialState, goalCondition: (s, a) => seleniumEnvironment.HasReachedAGoalCondition(s, a));

                    pathList.State.ShouldBe(WalkResultState.GoalReached);
                    pathList.Steps.ShouldNotBeNull();
                    pathList.Steps.ShouldNotBeEmpty();
                    pathList.Steps.Count.ShouldBe(2);
                    pathList.Steps[0].Action.ToString().ShouldEndWith("input[data-automation-id='first']");
                    pathList.Steps[1].Action.ToString().ShouldEndWith("input[data-automation-id='second']");
                }
                finally
                {
                    driver.Close();
                    driver.Quit();
                }
            }
        }

        [Fact]
        public async Task Run_WhenOutOfViewPortElementsExistOnTheRight_ThenItSuccessfullyFindsTheCorrectActions()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("headless");

            using (var driver = new ChromeDriver(@".\", chromeOptions))
            {
                driver.Manage().Window.Size = new Size(500, 500);

                try
                {
                    var fileUri = new Uri(Path.GetFullPath($"{nameof(Run_WhenOutOfViewPortElementsExistOnTheRight_ThenItSuccessfullyFindsTheCorrectActions)}.html"));
                    var random = new Random(1);
                    var seleniumEnvironment = new SeleniumEnvironment(
                        driver,
                        fileUri.AbsoluteUri,
                        (driver, _) =>
                        {
                            var target = driver.FindElementByCssSelector(".third-panel");
                            return target.Displayed && target.Enabled;
                        });
                    var seleniumRandomStepPolicy = new SeleniumQLearningStepPolicy(random);
                    var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumRandomStepPolicy);

                    await rlTrainer.Run(epochs: 2, maximumActions: 20);

                    var initialState = await seleniumEnvironment.GetInitialState();
                    var pathList = await rlTrainer.Walk(initialState, goalCondition: (s, a) => seleniumEnvironment.HasReachedAGoalCondition(s, a));

                    pathList.State.ShouldBe(WalkResultState.GoalReached);
                    pathList.Steps.ShouldNotBeNull();
                    pathList.Steps.ShouldNotBeEmpty();
                    pathList.Steps.Count.ShouldBe(2);
                    pathList.Steps[0].Action.ToString().ShouldEndWith("input[data-automation-id='first']");
                    pathList.Steps[1].Action.ToString().ShouldEndWith("input[data-automation-id='second']");
                }
                finally
                {
                    driver.Close();
                    driver.Quit();
                }
            }
        }

        [Fact]
        public async Task Run_WhenOutOfViewPortElementsExistWithSomeScrolling_ThenItSuccessfullyFindsTheCorrectActions()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("headless");

            using (var driver = new ChromeDriver(@".\", chromeOptions))
            {
                driver.Manage().Window.Size = new Size(500, 500);

                try
                {
                    var fileUri = new Uri(Path.GetFullPath($"{nameof(Run_WhenOutOfViewPortElementsExistWithSomeScrolling_ThenItSuccessfullyFindsTheCorrectActions)}.html"));
                    var random = new Random(1);
                    var seleniumEnvironment = new SeleniumEnvironment(
                        driver,
                        fileUri.AbsoluteUri,
                        (driver, _) =>
                        {
                            var target = driver.FindElementByCssSelector(".third-panel");
                            return target.Displayed && target.Enabled;
                        });
                    var seleniumRandomStepPolicy = new SeleniumQLearningStepPolicy(random);
                    var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumRandomStepPolicy);

                    await rlTrainer.Run(epochs: 2, maximumActions: 20);

                    var initialState = await seleniumEnvironment.GetInitialState();
                    var pathList = await rlTrainer.Walk(initialState, goalCondition: (s, a) => seleniumEnvironment.HasReachedAGoalCondition(s, a));

                    pathList.State.ShouldBe(WalkResultState.GoalReached);
                    pathList.Steps.ShouldNotBeNull();
                    pathList.Steps.ShouldNotBeEmpty();
                    pathList.Steps.Count.ShouldBe(2);
                    pathList.Steps[0].Action.ToString().ShouldEndWith("input[data-automation-id='first']");
                    pathList.Steps[1].Action.ToString().ShouldEndWith("input[data-automation-id='second']");
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
