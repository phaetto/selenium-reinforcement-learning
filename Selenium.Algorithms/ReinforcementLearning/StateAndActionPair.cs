namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Diagnostics;

    [DebuggerDisplay("[{State.ToString()}, {Action.ToString()}]")]
    public class StateAndActionPair<TData>
    {
        public StateAndActionPair(in State<TData> state, in AgentAction<TData> action)
        {
            State = state;
            Action = action;
        }

        public State<TData> State { get; }
        public AgentAction<TData> Action { get; }

        public override bool Equals(object obj)
        {
            return obj is StateAndActionPair<TData> stateAndAction
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
