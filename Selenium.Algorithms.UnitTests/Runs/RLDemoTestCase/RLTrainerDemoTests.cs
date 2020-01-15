namespace Selenium.Algorithms.IntegrationTests.Runs.RLDemoTestCase
{
    using Selenium.Algorithms.ReinforcementLearning;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class RLTrainerDemoTests
    {
        /// <summary>
        /// Tests the following scenario at
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2018/august/test-run-introduction-to-q-learning-using-csharp
        /// </summary>
        [Fact]
        public async Task Run_WhenTrainingWithPredefinedDemo_ThenItFindsTheCorrectSolution()
        {
            // Prepare
            var testEnvironment = new TestEnvironment();
            var testPolicy = new TestPolicy();
            var trainGoal = new TrainGoal();
            var rlTrainer = new RLTrainer<int>(testEnvironment, testPolicy, trainGoal);

            // Execute
            await rlTrainer.Run(epochs: 50, maximumActions: 100);

            var pathFinder = new RLPathFinder<int>(testEnvironment, testPolicy);
            var result = await pathFinder.Walk(new TestState(8), trainGoal.HasReachedAGoalCondition);
            result.State.ShouldBe(WalkResultState.GoalReached);
            result.Steps.ShouldNotBeNull();
            result.Steps.ShouldNotBeEmpty();
            result.Steps[0].ShouldBe(new StateAndActionPairWithResultState<int>(new TestState(8), new TestAction(new TestState(9))));
            result.Steps[1].ShouldBe(new StateAndActionPairWithResultState<int>(new TestState(9), new TestAction(new TestState(5))));
            result.Steps[2].ShouldBe(new StateAndActionPairWithResultState<int>(new TestState(5), new TestAction(new TestState(6))));
            result.Steps[3].ShouldBe(new StateAndActionPairWithResultState<int>(new TestState(6), new TestAction(new TestState(7))));
            result.Steps[4].ShouldBe(new StateAndActionPairWithResultState<int>(new TestState(7), new TestAction(new TestState(11))));
        }

        class TrainGoal : ITrainGoal<int>
        {
            private readonly double[][] R;

            public TrainGoal()
            {
                R = CreateReward(12);
            }

            public async Task<double> RewardFunction(State<int> state, AgentAction<int> action)
            {
                if (action is TestAction testAction)
                {
                    return R[state.Data][testAction.ToState.Data];
                }

                throw new InvalidCastException();
            }

            public async Task<bool> HasReachedAGoalCondition(State<int> state, AgentAction<int> action)
            {
                return new TestState(11).Equals(state);
            }

            private double[][] CreateReward(int ns)
            {
                double[][] R = new double[ns][];
                for (int i = 0; i < ns; ++i)
                    R[i] = new double[ns];
                R[0][1] = R[0][4] = R[1][0] = R[1][5] = R[2][3] = -0.1;
                R[2][6] = R[3][2] = R[3][7] = R[4][0] = R[4][8] = -0.1;
                R[5][1] = R[5][6] = R[5][9] = R[6][2] = R[6][5] = -0.1;
                R[6][7] = R[7][3] = R[7][6] = R[7][11] = R[8][4] = -0.1;
                R[8][9] = R[9][5] = R[9][8] = R[9][10] = R[10][9] = -0.1;
                R[7][11] = 10.0;  // goal
                return R;
            }
        }

        class TestEnvironment : Environment<int>
        {
            private readonly Random rnd = new Random(1);
            private readonly int[][] FT;

            public TestEnvironment()
            {
                FT = CreateMaze(12);
            }

            public override async Task<State<int>> GetInitialState()
            {
                var randomState = rnd.Next(0, 12);
                return new TestState(randomState);
            }

            public override async Task<IEnumerable<AgentAction<int>>> GetPossibleActions(State<int> state)
            {
                return GetPossNextStates(state.Data)
                    .Select(x => new TestAction(new TestState(x)));
            }

            public List<int> GetPossNextStates(int s)
            {
                List<int> result = new List<int>();
                for (int j = 0; j < FT.Length; ++j)
                    if (FT[s][j] == 1)
                        result.Add(j);
                return result;
            }

            public int GetRandNextState(int s)
            {
                List<int> possNextStates = GetPossNextStates(s);
                int ct = possNextStates.Count;
                int idx = rnd.Next(0, ct);
                return possNextStates[idx];
            }

            private int[][] CreateMaze(int ns)
            {
                int[][] FT = new int[ns][];
                for (int i = 0; i < ns; ++i)
                    FT[i] = new int[ns];
                FT[0][1] = FT[0][4] = FT[1][0] = FT[1][5] = FT[2][3] = 1;
                FT[2][6] = FT[3][2] = FT[3][7] = FT[4][0] = FT[4][8] = 1;
                FT[5][1] = FT[5][6] = FT[5][9] = FT[6][2] = FT[6][5] = 1;
                FT[6][7] = FT[7][3] = FT[7][6] = FT[7][11] = FT[8][4] = 1;
                FT[8][9] = FT[9][5] = FT[9][8] = FT[9][10] = FT[10][9] = 1;
                FT[11][7] = 1;  // goal
                return FT;
            }
        }

        class TestAction : AgentAction<int>
        {
            public TestAction(State<int> toState)
            {
                ToState = toState;
            }

            public State<int> ToState { get; }

            public override bool Equals(object obj)
            {
                var action = obj as TestAction;
                return action != null && action.ToState.Data == ToState.Data;
            }

            public override Task<State<int>> ExecuteAction(Environment<int> environment, State<int> state)
            {
                return Task.FromResult(ToState);
            }

            public override int GetHashCode()
            {
                return ToState.Data.GetHashCode();
            }

            public override string ToString()
            {
                return $"ToState = {ToState.Data}";
            }
        }

        class TestPolicy : Policy<int>
        {
            public override async Task<AgentAction<int>> GetNextAction(Environment<int> environment, State<int> state)
            {
                var randNextAction = (environment as TestEnvironment).GetRandNextState(state.Data);
                return new TestAction(new TestState(randNextAction));
            }
        }

        class TestState : State<int>
        {
            public TestState(int data) : base(data)
            {
            }

            public override bool Equals(object obj)
            {
                var state = obj as TestState;
                return state != null && state.Data == Data;
            }

            public override int GetHashCode()
            {
                return Data.GetHashCode();
            }

            public override string ToString()
            {
                return $"State = {Data}";
            }
        }
    }
}
