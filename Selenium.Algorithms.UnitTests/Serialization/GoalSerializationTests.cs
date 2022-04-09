namespace Selenium.Algorithms.IntegrationTests.Serialization
{
    using Selenium.Algorithms.ReinforcementLearning;
    using Shouldly;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class GoalSerializationTests
    {
        [Fact]
        public void Serialize_WhenSerializingAGoal_ThenCanDoItWithoutAnExplicitConverter()
        {
            ITrainGoal<int> initGoal = new Goal { SerializableProperty = "custom" };
            var json = JsonSerializer.Serialize(initGoal);

            var goal = JsonSerializer.Deserialize<ITrainGoal<int>>(json);

            goal.ShouldBeOfType(typeof(Goal));
            ((Goal)goal).SerializableProperty.ShouldBe("custom");
        }

        private sealed class Goal : ITrainGoal<int>
        {
            public string? SerializableProperty { get; set; }

            public Task<bool> HasReachedAGoalCondition(IState<int> state, IAgentAction<int> action)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
