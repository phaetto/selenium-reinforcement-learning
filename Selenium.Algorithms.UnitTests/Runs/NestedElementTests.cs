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

    public sealed class NestedElementTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture testFixture;

        public NestedElementTests(TestFixture testFixture)
        {
            this.testFixture = testFixture;
        }

        [Fact]
        public async Task Run_WhenAnActionableElementHasOtherNestedElements_ThenItSuccessfullyFindsTheCorrectActions()
        {
            using var driver = testFixture.GetWebDriver();

            try
            {
                var fileUri = new Uri(Path.GetFullPath($"{nameof(Run_WhenAnActionableElementHasOtherNestedElements_ThenItSuccessfullyFindsTheCorrectActions)}.html"));
                var random = new Random(1);
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
                var seleniumTrainGoal = new SeleniumTrainGoal<IReadOnlyCollection<ElementData>>(async (_1, _2) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                {
                    var target = driver.FindElement(By.CssSelector(".third"));
                    return target.Displayed && target.Enabled;
                });
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

                await rlTrainer.Run(epochs: 2, maximumActions: 15);

                var initialState = await seleniumEnvironment.GetInitialState();
                var pathFinder = new RLPathFinder<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumExperimentState);
                var pathList = await pathFinder.FindRoute(initialState, seleniumTrainGoal);

                pathList.State.ShouldBe(PathFindResultState.GoalReached);
                pathList.Steps.ShouldNotBeNull();
                pathList.Steps.ShouldNotBeEmpty();
                pathList.Steps.Count.ShouldBe(3);
                pathList.Steps[0].Action.ToString().ShouldEndWith("input[data-automation-id='first']");
                pathList.Steps[1].Action.ToString().ShouldEndWith("input[data-automation-id='second']");
                pathList.Steps[2].Action.ToString().ShouldEndWith("div[data-automation-id='third']");
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }
    }
}
