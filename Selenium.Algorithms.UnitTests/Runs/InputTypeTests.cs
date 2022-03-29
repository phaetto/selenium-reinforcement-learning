namespace Selenium.Algorithms.IntegrationTests.Runs
{
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using Xunit;
    using Shouldly;
    using System.IO;
    using System.Threading.Tasks;
    using Selenium.Algorithms;
    using Selenium.Algorithms.IntegrationTests.Framework;
    using OpenQA.Selenium;

    public sealed class InputTypeTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture testFixture;

        public InputTypeTests(TestFixture testFixture)
        {
            this.testFixture = testFixture;
        }

        [Fact]
        public async Task Run_WhenThereAreInputElements_ThenItSuccessfullyTypesRelevantInformation()
        {
            using var driver = testFixture.GetWebDriver();

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
                    var target = driver.FindElement(By.CssSelector(".third-panel"));
                    return target.Displayed && target.Enabled;
                });
                var seleniumEnvironment = new SeleniumEnvironment(
                    driver,
                    driver,
                    new SeleniumEnvironmentOptions
                    {
                        Url = fileUri.AbsoluteUri,
                        InputTextData = inputData,
                    });
                var seleniumQLearningStepPolicy = new SeleniumQLearningStepPolicy(random);
                var seleniumExperimentState = new SeleniumExperimentState();
                var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(new RLTrainerOptions<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumQLearningStepPolicy, seleniumExperimentState, seleniumTrainGoal));

                var trainerReport = await rlTrainer.Run(epochs: 5, maximumActions: 20);
                trainerReport.TimesReachedGoal.ShouldBePositive();

                var initialState = await seleniumEnvironment.GetInitialState();
                var pathFinder = new RLPathFinder<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumExperimentState);
                var pathList = await pathFinder.FindRoute(initialState, seleniumTrainGoal);

                pathList.State.ShouldBe(PathFindResultState.GoalReached);
                pathList.Steps.ShouldNotBeNull();
                pathList.Steps.ShouldNotBeEmpty();
                pathList.Steps.Count.ShouldBe(5);
                pathList.Steps[0].Action.ToString().ShouldEndWith("input[data-automation-id='name']");
                pathList.Steps[1].Action.ToString().ShouldEndWith("input[data-automation-id='description']");
                pathList.Steps[2].Action.ToString().ShouldEndWith("textarea[data-automation-id='text']");
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
