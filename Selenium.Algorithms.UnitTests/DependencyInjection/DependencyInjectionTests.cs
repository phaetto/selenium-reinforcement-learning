namespace Selenium.Algorithms.IntegrationTests.DependencyInjection
{
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using OpenQA.Selenium.Remote;
    using Selenium.Algorithms.ReinforcementLearning;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    /// <summary>
    /// DI tests are here to ensure architectural integrity
    /// </summary>
    public sealed class DependencyInjectionTests
    {
        [Fact]
        public void RLTrainer_WhenUsedWithDI_ThenIsDIFriendly()
        {
            var services = new ServiceCollection();
            services.AddTransient<IRLTrainer<int>, RLTrainer<int>>();
            services.AddSingleton(new Mock<IRLTrainerOptions<int>>().Object);

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<IRLTrainer<int>>();
        }

        [Fact]
        public void SeleniumEnvironment_WhenUsedWithDI_ThenIsDIFriendly()
        {
            var services = new ServiceCollection();
            services.AddTransient<IEnvironment<IReadOnlyCollection<ElementData>>, SeleniumEnvironment>();
            services.AddSingleton(new Mock<ISeleniumEnvironmentOptions>().Object);
            services.AddSingleton(new Mock<RemoteWebDriver>().Object);

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<IEnvironment<IReadOnlyCollection<ElementData>>>();
        }

        class TrainGoal : ITrainGoal<int>
        {
            public int TimesReachedGoal { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

            public Task<bool> HasReachedAGoalCondition(IState<int> state, IAgentAction<int> action)
            {
                throw new System.NotImplementedException();
            }

            public Task<double> RewardFunction(IState<int> stateFrom, IAgentAction<int> action)
            {
                throw new System.NotImplementedException();
            }
        }

        class TestEnvironment : IEnvironment<int>
        {
            public Task<IState<int>> GetCurrentState()
            {
                throw new System.NotImplementedException();
            }

            public Task<IState<int>> GetInitialState()
            {
                throw new System.NotImplementedException();
            }

            public Task<IEnumerable<IAgentAction<int>>> GetPossibleActions(IState<int> state)
            {
                throw new System.NotImplementedException();
            }

            public Task<bool> IsIntermediateState(IState<int> state)
            {
                throw new System.NotImplementedException();
            }

            public Task WaitForPostActionIntermediateStabilization()
            {
                throw new System.NotImplementedException();
            }
        }

        class TestPolicy : IPolicy<int>
        {
            public IDictionary<StateAndActionPair<int>, double> QualityMatrix { get; } = new Dictionary<StateAndActionPair<int>, double>();

            public Task<IAgentAction<int>> GetNextAction(IEnvironment<int> environment, IState<int> state)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
