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
            var trainExperimentState = new TestExperimentState();
            var rlTrainer = new RLTrainer<int>(new RLTrainerOptions<int>(testEnvironment, testPolicy, trainExperimentState, trainGoal));

            // Execute
            var trainerReport = await rlTrainer.Run(epochs: 50, maximumActions: 100);
            trainerReport.TimesReachedGoal.ShouldBePositive();

            var pathFinder = new RLPathFinder<int>(testEnvironment, trainExperimentState);
            var result = await pathFinder.FindRoute(new TestState(8), trainGoal);
            result.State.ShouldBe(PathFindResultState.GoalReached);
            result.Steps.ShouldNotBeNull();
            result.Steps.ShouldNotBeEmpty();
            result.Steps[0].ShouldBe(new StateAndActionPair<int>(new TestState(8), new TestAction(new TestState(9))));
            result.Steps[1].ShouldBe(new StateAndActionPair<int>(new TestState(9), new TestAction(new TestState(5))));
            result.Steps[2].ShouldBe(new StateAndActionPair<int>(new TestState(5), new TestAction(new TestState(6))));
            result.Steps[3].ShouldBe(new StateAndActionPair<int>(new TestState(6), new TestAction(new TestState(7))));
            result.Steps[4].ShouldBe(new StateAndActionPair<int>(new TestState(7), new TestAction(new TestState(11))));
        }

        class TrainGoal : ITrainGoal<int>
        {
            private readonly double[][] R;

            public int TimesReachedGoal { get; set; }

            public TrainGoal()
            {
                R = CreateReward(12);
            }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            public async Task<double> RewardFunction(IState<int> state, IAgentAction<int> action)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                if (action is TestAction testAction)
                {
                    return R[state.Data][testAction.ToState.Data];
                }

                throw new InvalidCastException();
            }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            public async Task<bool> HasReachedAGoalCondition(IState<int> state, IAgentAction<int> action)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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

        class TestEnvironment : IEnvironment<int>
        {
            private readonly Random rnd = new Random(1);
            private readonly int[][] FT;

            public TestEnvironment()
            {
                FT = CreateMaze(12);
            }

            public Task<IState<int>> GetCurrentState()
            {
                throw new NotSupportedException();
            }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            public async Task<IState<int>> GetInitialState()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var randomState = rnd.Next(0, 12);
                return new TestState(randomState);
            }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            public async Task<IEnumerable<IAgentAction<int>>> GetPossibleActions(IState<int> state)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            public async Task<bool> IsIntermediateState(IState<int> state)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                return false;
            }

            public Task WaitForPostActionIntermediateStabilization()
            {
                throw new NotSupportedException();
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

        class TestExperimentState : IExperimentState<int>
        {
            public IDictionary<StateAndActionPair<int>, double> QualityMatrix { get; } = new Dictionary<StateAndActionPair<int>, double>();
        }

        class TestAction : IAgentAction<int>
        {
            public TestAction(IState<int> toState)
            {
                ToState = toState;
            }

            public IState<int> ToState { get; }

            public override bool Equals(object? obj)
            {
                return obj is TestAction action && action.ToState.Data == ToState.Data;
            }

            public Task<IState<int>> ExecuteAction(IEnvironment<int> environment, IState<int> state)
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

        class TestPolicy : IPolicy<int>
        {
            public IDictionary<StateAndActionPair<int>, double> QualityMatrix { get; } = new Dictionary<StateAndActionPair<int>, double>();

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            public async Task<IAgentAction<int>> GetNextAction(IEnvironment<int> environment, IState<int> state, IExperimentState<int> experimentState)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var randNextAction = ((TestEnvironment)environment).GetRandNextState(state.Data);
                return new TestAction(new TestState(randNextAction));
            }
        }

        class TestState : IState<int>
        {
            public TestState(int data)
            {
                Data = data;
            }

            public int Data { get; }

            public override bool Equals(object? obj)
            {
                return obj is TestState state && state.Data == Data;
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
