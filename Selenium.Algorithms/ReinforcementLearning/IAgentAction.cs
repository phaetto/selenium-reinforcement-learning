namespace Selenium.Algorithms.ReinforcementLearning
{
    using Selenium.Algorithms.ReinforcementLearning.Serialization;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    [JsonConverter(typeof(AgentActionConverterFactory))]
    public interface IAgentAction<TData>
    {
        public Task<IState<TData>> ExecuteAction(IEnvironment<TData> environment, IState<TData> state);
        public bool Equals(object obj);
        public int GetHashCode();
        public string ToString();
    }
}
