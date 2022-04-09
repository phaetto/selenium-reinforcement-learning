namespace Selenium.ReinforcementLearning.Framework
{
    using OpenQA.Selenium;
    using Selenium.Algorithms;
    using Selenium.Algorithms.ReinforcementLearning;
    using Selenium.Algorithms.Serialization;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public sealed class TestCaseTrainFactDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink diagnosticMessageSink;

        public IMessageSink DiagnosticMessageSink => diagnosticMessageSink;

        public TestCaseTrainFactDiscoverer(
            IMessageSink diagnosticMessageSink
        )
        {
            this.diagnosticMessageSink = diagnosticMessageSink ?? throw new ArgumentNullException(nameof(diagnosticMessageSink));
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            var data = new List<TrainedInput>();
            var trainedInputs = Attribute.GetCustomAttributes(testMethod.Method.ToRuntimeMethod(), typeof(TrainedInputAttribute))
                .Cast<TrainedInputAttribute>()
                .ToArray();

            foreach (var trainedInput in trainedInputs)
            {
                data.Add(new TrainedInput(trainedInput.TrainingMethodName));
            }

            if (data.Count != testMethod.Method.GetParameters().Count())
            {
                return new IXunitTestCase[] {
                    new ExecutionErrorTestCase(
                        DiagnosticMessageSink,
                        discoveryOptions.MethodDisplayOrDefault(),
                        discoveryOptions.MethodDisplayOptionsOrDefault(),
                        testMethod,
                        $"Failed to find trained inputs for all parameters:\n\tParameters found:{testMethod.Method.GetParameters().Count()}\n\tInputs found: {data.Count}")
                };
            }

            var xUnitTestCase = new XunitTestCase(
                DiagnosticMessageSink,
                discoveryOptions.MethodDisplayOrDefault(),
                discoveryOptions.MethodDisplayOptionsOrDefault(),
                testMethod,
                data.Cast<object>().ToArray()
            );

            return new IXunitTestCase[] { xUnitTestCase };
        }

        private sealed class TrainedInput : ITrainedInput, IXunitSerializable
        {
            [EditorBrowsable(EditorBrowsableState.Never)]
            [Obsolete("Called by the de-serializer", error: true)]
            public TrainedInput()
            { }

            public TrainedInput(string experimentName)
            {
                ExperimentName = experimentName;
            }

            public string ExperimentName { get; private set; } = string.Empty;

            public void Deserialize(IXunitSerializationInfo info)
            {
                ExperimentName = info.GetValue<string>(nameof(ExperimentName));
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue(nameof(ExperimentName), ExperimentName);
            }

            public async Task<Experiment<IReadOnlyCollection<ElementData>>> GetExperiment(IWebDriver driver, IPersistenceIO persistenceIO)
            {
                var json = await persistenceIO.Read(ExperimentName);
                var jsonSerializerOptions = new JsonSerializerOptions();
                jsonSerializerOptions.Converters.Add(new SeleniumEnvironmentConverter(driver, (IJavaScriptExecutor)driver));
                var experiment = JsonSerializer.Deserialize<Experiment<IReadOnlyCollection<ElementData>>>(json, jsonSerializerOptions);
                if (experiment == null)
                {
                    throw new InvalidOperationException();
                }

                return experiment;
            }
        }
    }
}
