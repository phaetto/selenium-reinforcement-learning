namespace Selenium.ReinforcementLearning.Framework
{
    using System;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class TrainedInputAttribute : Attribute
    {
        public TrainedInputAttribute(string trainingMethodName)
        {
            TrainingMethodName = trainingMethodName;
        }

        public string TrainingMethodName { get; }
    }
}
