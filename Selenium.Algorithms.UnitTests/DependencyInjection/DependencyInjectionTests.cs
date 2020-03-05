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
            services.AddTransient<SeleniumEnvironment>();
            services.AddSingleton(new Mock<ISeleniumEnvironmentOptions>().Object);
            services.AddSingleton(new Mock<RemoteWebDriver>().Object);

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<SeleniumEnvironment>(); // TODO: Use interface
        }

        class TrainGoal : ITrainGoal<int>
        {
            public int TimesReachedGoal { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

            public Task<bool> HasReachedAGoalCondition(State<int> state, AgentAction<int> action)
            {
                throw new System.NotImplementedException();
            }

            public Task<double> RewardFunction(State<int> stateFrom, AgentAction<int> action)
            {
                throw new System.NotImplementedException();
            }
        }

        class TestEnvironment : Environment<int>
        {
            public override Task<State<int>> GetCurrentState()
            {
                throw new System.NotImplementedException();
            }

            public override Task<State<int>> GetInitialState()
            {
                throw new System.NotImplementedException();
            }

            public override Task<IEnumerable<AgentAction<int>>> GetPossibleActions(State<int> state)
            {
                throw new System.NotImplementedException();
            }

            public override Task<bool> IsIntermediateState(State<int> state)
            {
                throw new System.NotImplementedException();
            }

            public override Task WaitForPostActionIntermediateStabilization()
            {
                throw new System.NotImplementedException();
            }
        }

        class TestPolicy : Policy<int>
        {
            public override Task<AgentAction<int>> GetNextAction(Environment<int> environment, State<int> state)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
