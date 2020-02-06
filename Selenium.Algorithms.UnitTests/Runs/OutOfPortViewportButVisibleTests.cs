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

    public sealed class OutOfPortViewportButVisibleTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture testFixture;

        public OutOfPortViewportButVisibleTests(TestFixture testFixture)
        {
            this.testFixture = testFixture;
        }

        [Fact]
        public async Task Run_WhenOutOfViewPortElementsExist_ThenItSuccessfullyFindsTheCorrectActions()
        {
            using var driver = testFixture.GetWebDriver();

            try
            {
                var fileUri = new Uri(Path.GetFullPath($"{nameof(Run_WhenOutOfViewPortElementsExist_ThenItSuccessfullyFindsTheCorrectActions)}.html"));
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
                    new SeleniumEnvironmentOptions
                    {
                        Url = fileUri.AbsoluteUri,
                    });
                var seleniumQLearningStepPolicy = new SeleniumQLearningStepPolicy(random);
                var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumQLearningStepPolicy, seleniumTrainGoal);

                await rlTrainer.Run(epochs: 2, maximumActions: 20);

                var initialState = await seleniumEnvironment.GetInitialState();
                var pathFinder = new RLPathFinder<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumQLearningStepPolicy);
                var pathList = await pathFinder.FindRoute(initialState, seleniumTrainGoal);

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
        public async Task Run_WhenOutOfViewPortElementsExistOnTheRight_ThenItSuccessfullyFindsTheCorrectActions()
        {
            using var driver = testFixture.GetWebDriver();

            try
            {
                var fileUri = new Uri(Path.GetFullPath($"{nameof(Run_WhenOutOfViewPortElementsExistOnTheRight_ThenItSuccessfullyFindsTheCorrectActions)}.html"));
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
                    new SeleniumEnvironmentOptions
                    {
                        Url = fileUri.AbsoluteUri,
                    });
                var seleniumQLearningStepPolicy = new SeleniumQLearningStepPolicy(random);
                var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumQLearningStepPolicy, seleniumTrainGoal);

                await rlTrainer.Run(epochs: 2, maximumActions: 20);

                var initialState = await seleniumEnvironment.GetInitialState();
                var pathFinder = new RLPathFinder<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumQLearningStepPolicy);
                var pathList = await pathFinder.FindRoute(initialState, seleniumTrainGoal);

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
        public async Task Run_WhenOutOfViewPortElementsExistWithSomeScrolling_ThenItSuccessfullyFindsTheCorrectActions()
        {
            using var driver = testFixture.GetWebDriver();

            try
            {
                var fileUri = new Uri(Path.GetFullPath($"{nameof(Run_WhenOutOfViewPortElementsExistWithSomeScrolling_ThenItSuccessfullyFindsTheCorrectActions)}.html"));
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
                    new SeleniumEnvironmentOptions
                    {
                        Url = fileUri.AbsoluteUri,
                    });
                var seleniumQLearningStepPolicy = new SeleniumQLearningStepPolicy(random);
                var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumQLearningStepPolicy, seleniumTrainGoal);

                await rlTrainer.Run(epochs: 2, maximumActions: 20);

                var initialState = await seleniumEnvironment.GetInitialState();
                var pathFinder = new RLPathFinder<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumQLearningStepPolicy);
                var pathList = await pathFinder.FindRoute(initialState, seleniumTrainGoal);

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
