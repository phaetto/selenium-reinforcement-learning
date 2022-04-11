namespace Selenium.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public interface ISeleniumEnvironmentOptions
    {
        IReadOnlyCollection<string> ActionableElementsCssSelectors { get; set; }
        IReadOnlyDictionary<string, string> InputTextData { get; set; }
        IReadOnlyCollection<string> LoadingElementsCssSelectors { get; set; }
        IReadOnlyCollection<string> GoalElementSelectors { get; set; }
        string Url { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        Action<string> WriteLine { get; set; }
    }
}