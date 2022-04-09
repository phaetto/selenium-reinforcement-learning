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

    public sealed class SimpleTestCaseTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture testFixture;

        public SimpleTestCaseTests(TestFixture testFixture)
        {
            this.testFixture = testFixture;
        }

        [Fact]
        public async Task Run_WhenTrainingASimpleTestCase_ThenItSuccessfullyFindsTheCorrectActions()
        {
            using var driver = testFixture.GetWebDriver();

            try
            {
                var fileUri = new Uri(Path.GetFullPath($"{nameof(Run_WhenTrainingASimpleTestCase_ThenItSuccessfullyFindsTheCorrectActions)}.html"));
                var random = new Random(1);
                var seleniumTrainGoal = new SeleniumClassContainsGoal("third");
                var seleniumEnvironment = new SeleniumEnvironment(
                    driver,
                    driver,
                    new SeleniumEnvironmentOptions
                    {
                        Url = fileUri.AbsoluteUri,
                    });
                var seleniumRandomStepPolicy = new SeleniumRandomStepPolicy(random);
                var seleniumExperimentState = new SeleniumExperimentState();
                var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(new RLTrainerOptions<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumRandomStepPolicy, seleniumExperimentState, seleniumTrainGoal));

                var trainerReport = await rlTrainer.Run(epochs: 4, maximumActions: 20);
                trainerReport.TimesReachedGoal.ShouldBePositive();

                var initialState = await seleniumEnvironment.GetInitialState();
                var pathFinder = new RLPathFinder<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumExperimentState);
                var pathList = await pathFinder.FindRoute(initialState, seleniumTrainGoal);

                pathList.State.ShouldBe(PathFindResultState.GoalReached);
                pathList.Steps.ShouldNotBeNull();
                pathList.Steps.ShouldNotBeEmpty();
                pathList.Steps.Count.ShouldBe(3);
                pathList.Steps[0].Action.ToString().ShouldEndWith("input[data-automation-id='first']");
                pathList.Steps[1].Action.ToString().ShouldEndWith("input[data-automation-id='second']");
                pathList.Steps[2].Action.ToString().ShouldEndWith("button[data-automation-id='third']");
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }

        [Fact]
        public async Task Run_WhenTrainingASimpleTestCase_ThenWeCanFindRouteWithoutUsingBrowser()
        {
            using var driver = testFixture.GetWebDriver();

            try
            {
                var fileUri = new Uri(Path.GetFullPath($"{nameof(Run_WhenTrainingASimpleTestCase_ThenItSuccessfullyFindsTheCorrectActions)}.html"));
                var random = new Random(1);
                var seleniumTrainGoal = new SeleniumClassContainsGoal("third");
                var seleniumEnvironment = new SeleniumEnvironment(
                    driver,
                    driver,
                    new SeleniumEnvironmentOptions
                    {
                        Url = fileUri.AbsoluteUri,
                    });
                var seleniumRandomStepPolicy = new SeleniumRandomStepPolicy(random);
                var seleniumExperimentState = new SeleniumExperimentState();
                var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(new RLTrainerOptions<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumRandomStepPolicy, seleniumExperimentState, seleniumTrainGoal));

                var trainerReport = await rlTrainer.Run(epochs: 4, maximumActions: 20);
                trainerReport.TimesReachedGoal.ShouldBePositive();

                var initialState = await seleniumEnvironment.GetInitialState();
                var pathFinder = new RLPathFinder<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumExperimentState);
                var pathList = await pathFinder.FindRouteWithoutApplyingActions(initialState, seleniumTrainGoal);

                pathList.State.ShouldBe(PathFindResultState.GoalReached);
                pathList.Steps.ShouldNotBeNull();
                pathList.Steps.ShouldNotBeEmpty();
                pathList.Steps.Count.ShouldBe(3);
                pathList.Steps[0].Action.ToString().ShouldEndWith("input[data-automation-id='first']");
                pathList.Steps[1].Action.ToString().ShouldEndWith("input[data-automation-id='second']");
                pathList.Steps[2].Action.ToString().ShouldEndWith("button[data-automation-id='third']");
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }
    }
}
