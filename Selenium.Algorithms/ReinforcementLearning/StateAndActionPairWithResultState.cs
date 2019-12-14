namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Diagnostics;

    [DebuggerDisplay("State: {State.ToString()} --- Action: {Action.ToString()}")]
    public class StateAndActionPairWithResultState<TData>
    {
        public StateAndActionPairWithResultState(
            in State<TData> state,
            in AgentAction<TData> action,
            in State<TData> resultState = null)
        {
            State = state;
            Action = action;
            ResultState = resultState;
        }

        public State<TData> State { get; }
        public AgentAction<TData> Action { get; }
        public State<TData> ResultState { get; }

        public override bool Equals(object obj)
        {
            return obj is StateAndActionPairWithResultState<TData> stateAndAction
                && Equals(stateAndAction.State, State)
                && Equals(stateAndAction.Action, Action);
        }

        public override int GetHashCode()
        {
            var hash = 13;
            hash = (hash * 7) + State.GetHashCode();
            hash = (hash * 7) + Action.GetHashCode();
            return hash;
        }
    }
}
