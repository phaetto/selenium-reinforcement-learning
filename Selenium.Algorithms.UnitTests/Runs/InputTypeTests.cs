namespace Selenium.Algorithms.IntegrationTests.Runs
{
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

    public sealed class InputTypeTests
    {
        [Fact]
        public async Task Run_WhenThereAreInputElements_ThenItSuccessfullyTypesRelevantInformation()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("headless");

            using var driver = new ChromeDriver(@".\", chromeOptions);
            driver.Manage().Window.Size = new Size(1000, 768);

            var inputData = new Dictionary<string, string>
                {
                    { "name", "Alex" },
                    { "description", "Awesome Dev" },
                    { "text", "This is a random text for me" }
                };

            try
            {
                var fileUri = new Uri(Path.GetFullPath($"{nameof(Run_WhenThereAreInputElements_ThenItSuccessfullyTypesRelevantInformation)}.html"));
                var random = new Random(1);
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
                var seleniumTrainGoal = new SeleniumTrainGoal<IReadOnlyCollection<ElementData>>(async (_1, _2) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                {
                    var target = driver.FindElementByCssSelector(".third-panel");
                    return target.Displayed && target.Enabled;
                });
                var seleniumEnvironment = new SeleniumEnvironment(
                    driver,
                    fileUri.AbsoluteUri,
                    inputTextData: inputData);
                var seleniumQLearningStepPolicy = new SeleniumQLearningStepPolicy(random);
                var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumQLearningStepPolicy, seleniumTrainGoal);

                await rlTrainer.Run(epochs: 5, maximumActions: 20);

                var initialState = await seleniumEnvironment.GetInitialState();
                var pathFinder = new RLPathFinder<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumQLearningStepPolicy);
                var pathList = await pathFinder.Walk(initialState, goalCondition: seleniumTrainGoal.HasReachedAGoalCondition);

                pathList.State.ShouldBe(WalkResultState.GoalReached);
                pathList.Steps.ShouldNotBeNull();
                pathList.Steps.ShouldNotBeEmpty();
                pathList.Steps.Count.ShouldBe(5);
                pathList.Steps[0].Action.ToString().ShouldEndWith("input[data-automation-id='name']");
                pathList.Steps[1].Action.ToString().ShouldEndWith("textarea[data-automation-id='text']");
                pathList.Steps[2].Action.ToString().ShouldEndWith("input[data-automation-id='description']");
                pathList.Steps[3].Action.ToString().ShouldEndWith("input[data-automation-id='done']");
                pathList.Steps[4].Action.ToString().ShouldEndWith("input[data-automation-id='accept']");
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }
    }
}
