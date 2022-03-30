namespace Selenium.Algorithms.ReinforcementLearning
{
    using Selenium.Algorithms.ReinforcementLearning.Serialization;
    using System.Text.Json.Serialization;

    [JsonConverter(typeof(StateConverterFactory))]
    public interface IState<TData>
    {
        public TData Data { get; }

        public bool Equals(object obj);
        public int GetHashCode();
        public string ToString();
    }
}
