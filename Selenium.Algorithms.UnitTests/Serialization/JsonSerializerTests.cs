﻿namespace Selenium.Algorithms.IntegrationTests.Serialization
{
    using Selenium.Algorithms.IntegrationTests.Framework;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit;
    using Shouldly;

    public sealed  class JsonSerializerTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture testFixture;

        public JsonSerializerTests(TestFixture testFixture)
        {
            this.testFixture = testFixture;
        }

        [Fact]
        public async Task Serialize_WhenTrainingASimpleTestCase_ThenItSuccessfullySerializesAndDeserializesState()
        {
            using var driver = testFixture.GetWebDriver();

            try
            {
                var fileUri = new Uri(Path.GetFullPath(Path.Combine("Serialization", $"{nameof(Serialize_WhenTrainingASimpleTestCase_ThenItSuccessfullySerializesAndDeserializesState)}.html")));
                var random = new Random(1);
                var seleniumTrainGoal = new SeleniumClassContainsGoal("third");
                var seleniumEnvironment = new SeleniumEnvironment(
                    driver,
                    driver,
                    new SeleniumEnvironmentOptions
                    {
                        Url = fileUri.AbsoluteUri,
                    });
                var seleniumRandomStepPolicy = new SeleniumRandomStepPolicy(random);
                var seleniumExperimentState = new SeleniumExperimentState();
                var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(new RLTrainerOptions<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumRandomStepPolicy, seleniumExperimentState, seleniumTrainGoal));

                var trainerReport = await rlTrainer.Run(epochs: 2, maximumActions: 20);
                trainerReport.TimesReachedGoal.ShouldBePositive();

                var seleniumExperimentStateJsonData = JsonSerializer.Serialize(seleniumExperimentState);
                var seleniumExperimentStateDeserialized = JsonSerializer.Deserialize<SeleniumExperimentState>(seleniumExperimentStateJsonData);
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }

        [Fact]
        public async Task JsonSerializer_WhenTrainingSerializingAndDeserializing_ThenWeCanContinueTraining()
        {
            using var driver = testFixture.GetWebDriver();

            try
            {
                var fileUri = new Uri(Path.GetFullPath(Path.Combine("Serialization", $"{nameof(Serialize_WhenTrainingASimpleTestCase_ThenItSuccessfullySerializesAndDeserializesState)}.html")));
                var random = new Random(1);
                var seleniumTrainGoal = new SeleniumClassContainsGoal("third");
                var seleniumEnvironment = new SeleniumEnvironment(
                    driver,
                    driver,
                    new SeleniumEnvironmentOptions
                    {
                        Url = fileUri.AbsoluteUri,
                    });
                var seleniumRandomStepPolicy = new SeleniumRandomStepPolicy(random);
                var seleniumExperimentState = new SeleniumExperimentState();
                var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(new RLTrainerOptions<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumRandomStepPolicy, seleniumExperimentState, seleniumTrainGoal));

                var trainerReport = await rlTrainer.Run(epochs: 1, maximumActions: 2);

                trainerReport.TimesReachedGoal.ShouldBe(0);

                var seleniumExperimentStateJsonData = JsonSerializer.Serialize(seleniumExperimentState);
                var seleniumExperimentStateDeserialized = JsonSerializer.Deserialize<SeleniumExperimentState>(seleniumExperimentStateJsonData);

                if (seleniumExperimentStateDeserialized == null)
                {
                    throw new InvalidOperationException();
                }

                await seleniumEnvironment.GetInitialState();
                var rlTrainer2 = new RLTrainer<IReadOnlyCollection<ElementData>>(new RLTrainerOptions<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumRandomStepPolicy, seleniumExperimentStateDeserialized, seleniumTrainGoal));
                var trainerReport2 = await rlTrainer2.Run(epochs: 2, maximumActions: 20);

                trainerReport2.TimesReachedGoal.ShouldBePositive();
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }
    }
}
