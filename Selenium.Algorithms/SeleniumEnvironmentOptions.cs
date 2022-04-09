namespace Selenium.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text.Json.Serialization;

    public sealed class SeleniumEnvironmentOptions : ISeleniumEnvironmentOptions
    {
        public IReadOnlyDictionary<string, string> InputTextData { get; set; } = new Dictionary<string, string>();

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Action<string> WriteLine { get; set; } = x => Debug.WriteLine(x);

        public IReadOnlyCollection<string> ActionableElementsCssSelectors { get; set; } = new string[] { "body *[data-automation-id]" };

        public IReadOnlyCollection<string> LoadingElementsCssSelectors { get; set; } = new string[0];

        public IReadOnlyCollection<string> GoalElementSelectors { get; set; } = new string[] { "body *[data-automation-goal]" };

        public string Url { get; set; } = string.Empty;
    }
}
