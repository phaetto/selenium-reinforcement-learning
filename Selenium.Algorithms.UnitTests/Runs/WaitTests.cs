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

    public sealed class WaitTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture testFixture;

        public WaitTests(TestFixture testFixture)
        {
            this.testFixture = testFixture;
        }

        [Fact]
        public async Task Run_WhenThereIsADelayedAction_ThenItSuccessfullyWaits()
        {
            using var driver = testFixture.GetWebDriver();

            try
            {
                var fileUri = new Uri(Path.GetFullPath($"{nameof(Run_WhenThereIsADelayedAction_ThenItSuccessfullyWaits)}.html"));
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
                    });
                var seleniumQLearningStepPolicy = new SeleniumQLearningStepPolicy(random);
                var seleniumExperimentState = new SeleniumExperimentState();
                var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(new RLTrainerOptions<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumQLearningStepPolicy, seleniumExperimentState, seleniumTrainGoal));

                var trainerReport = await rlTrainer.Run(epochs: 4, maximumActions: 50);
                trainerReport.TimesReachedGoal.ShouldBePositive();

                var initialState = await seleniumEnvironment.GetInitialState();
                var pathFinder = new RLPathFinder<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumExperimentState);
                var pathList = await pathFinder.FindRoute(initialState, seleniumTrainGoal, maxSteps: 50);

                pathList.State.ShouldBe(PathFindResultState.GoalReached);
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

        [Fact]
        public async Task Run_WhenThereIsADelayedActionWithLoader_ThenItSuccessfullyWaits()
        {
            using var driver = testFixture.GetWebDriver();

            try
            {
                var fileUri = new Uri(Path.GetFullPath($"{nameof(Run_WhenThereIsADelayedActionWithLoader_ThenItSuccessfullyWaits)}.html"));
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
                        LoadingElementsCssSelectors = new string[] { ".loader" },
                    });
                var seleniumQLearningStepPolicy = new SeleniumQLearningStepPolicy(random);
                var seleniumExperimentState = new SeleniumExperimentState();
                var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(new RLTrainerOptions<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumQLearningStepPolicy, seleniumExperimentState, seleniumTrainGoal));

                var trainerReport = await rlTrainer.Run(epochs: 4, maximumActions: 50);
                trainerReport.TimesReachedGoal.ShouldBePositive();

                var initialState = await seleniumEnvironment.GetInitialState();
                var pathFinder = new RLPathFinder<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumExperimentState);
                var pathList = await pathFinder.FindRoute(initialState, seleniumTrainGoal, maxSteps: 50);

                pathList.State.ShouldBe(PathFindResultState.GoalReached);
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
