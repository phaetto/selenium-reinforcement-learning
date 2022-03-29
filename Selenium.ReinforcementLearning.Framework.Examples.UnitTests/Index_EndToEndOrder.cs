namespace Selenium.ReinforcementLearning.Framework.Examples.UnitTests
{
    using Selenium.Algorithms;
    using Selenium.Algorithms.ReinforcementLearning;
    using Selenium.ReinforcementLearning.Framework.Examples.UnitTests.Framework;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;
    using Shouldly;
    using System.IO;
    using System.Text.Json;

    /// <summary>
    /// Simulates an order done in https://www.saucedemo.com/.
    /// The test trains 2 cases:
    ///  - Train to login and pick an item
    ///  - Taking an order from the basket to the end
    ///  
    /// Then uses those two cases to run a test without needing to explicitly point out the process to the bot.
    /// 
    /// </summary>
    public sealed class Index_EndToEndOrder : IClassFixture<TestFixture>
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        private readonly TestFixture testFixture;

        public Index_EndToEndOrder(TestFixture testFixture)
        {
            this.testFixture = testFixture;
        }

        /*
         * Some TODOs for me:
         * SetupInitialState must move to something else since a trained session had connection to its environment
         * Maybe: Make API for traits
         * Maybe: Make API for reloading seamlesly the trained matrix and replaying
         */

        [Fact]
        // [Train] // Note: The API is not there yet
        public async Task Index_LoginAndAddItemToCart()
        {
            using var driver = testFixture.GetWebDriver();

            try
            {
                var random = new Random(1);
                var seleniumEnvironment = Environments.Index_LoginAndAddItemToCart(driver);
                var seleniumRandomStepPolicy = new SeleniumQLearningStepPolicy(random);
                var seleniumExperimentState = new SeleniumExperimentState();
                var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(new RLTrainerOptions<IReadOnlyCollection<ElementData>>(
                    seleniumEnvironment,
                    seleniumRandomStepPolicy,
                    seleniumExperimentState,
                    Goals.DoesCartHasOneItem(driver)));

                var trainerReport = await rlTrainer.Run(epochs: 5, maximumActions: 40);
                trainerReport.TimesReachedGoal.ShouldBePositive();

                File.WriteAllText($"{nameof(Index_LoginAndAddItemToCart)}.trained.json", JsonSerializer.Serialize(seleniumExperimentState, jsonSerializerOptions));
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }

        [Fact]
        // [Train]
        // [Dependency(Index_AddToCart)]
        public async Task Cart_CheckOut()
        {
            // Simulates the dependency on another trainer
            // Needs Index_AddToCart to set up the state
            var Index_AddAnyItemToCartJson = File.ReadAllText($"{nameof(Index_LoginAndAddItemToCart)}.trained.json");
            var Index_AddAnyItemToCartSeleniumExperimentState = JsonSerializer.Deserialize<SeleniumExperimentState>(Index_AddAnyItemToCartJson);

            if (Index_AddAnyItemToCartSeleniumExperimentState == null)
            {
                throw new InvalidOperationException();
            }

            using var driver = testFixture.GetWebDriver();

            try
            {
                var random = new Random(1);
                var seleniumEnvironment = Environments.Cart_CheckOut(driver, Index_AddAnyItemToCartSeleniumExperimentState);
                var seleniumRandomStepPolicy = new SeleniumQLearningStepPolicy(random);
                var seleniumExperimentState = new SeleniumExperimentState();
                var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(new RLTrainerOptions<IReadOnlyCollection<ElementData>>(
                    seleniumEnvironment,
                    seleniumRandomStepPolicy,
                    seleniumExperimentState,
                    Goals.HasOrderBeenPosted(driver)));

                var trainerReport = await rlTrainer.Run(epochs: 10, maximumActions: 50);
                trainerReport.TimesReachedGoal.ShouldBePositive();

                File.WriteAllText($"{nameof(Cart_CheckOut)}.trained.json", JsonSerializer.Serialize(seleniumExperimentState, jsonSerializerOptions));
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }

        // Add last end to end test here
    }
}
