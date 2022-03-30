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
    using OpenQA.Selenium;

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

        [Fact]
        // [Train] // Note: The API is not there yet
        public async Task _1_Index_LoginAndAddItemToCart()
        {
            /*
             * Common usage of the trainer. We want the system to find and navigate automatically throught the UI
             * and return that result as a state which we then save as json.
             */
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

                File.WriteAllText($"{nameof(_1_Index_LoginAndAddItemToCart)}.trained.json", JsonSerializer.Serialize(seleniumExperimentState, jsonSerializerOptions));
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
        public async Task _2_Cart_CheckOut()
        {
            /*
             * This test simulates the dependency on another trainer.
             * Uses the output from #1 to login and add to cart before checking out.
             * */

            var Index_AddAnyItemToCartJson = File.ReadAllText($"{nameof(_1_Index_LoginAndAddItemToCart)}.trained.json");
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

                File.WriteAllText($"{nameof(_2_Cart_CheckOut)}.trained.json", JsonSerializer.Serialize(seleniumExperimentState, jsonSerializerOptions));
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }

        [Fact]
        public async Task _3_EndToEndTest_BuyAnItemAndCheckout()
        {
            /* 
             * This is a test that uses the trained data to navigate without explicitly specifying the steps.
             * We load those data from the json output of the previous tests.
             * */
            var Index_AddAnyItemToCartJson = File.ReadAllText($"{nameof(_1_Index_LoginAndAddItemToCart)}.trained.json");
            var Index_AddAnyItemToCartSeleniumExperimentState = JsonSerializer.Deserialize<SeleniumExperimentState>(Index_AddAnyItemToCartJson);

            if (Index_AddAnyItemToCartSeleniumExperimentState == null)
            {
                throw new InvalidOperationException();
            }

            var Cart_CheckOutJson = File.ReadAllText($"{nameof(_2_Cart_CheckOut)}.trained.json");
            var Cart_CheckOutSeleniumExperimentState = JsonSerializer.Deserialize<SeleniumExperimentState>(Cart_CheckOutJson);

            if (Cart_CheckOutSeleniumExperimentState == null)
            {
                throw new InvalidOperationException();
            }

            using var driver = testFixture.GetWebDriver();

            try
            {
                // Go to homepage
                driver.Navigate().GoToUrl(Data.HomePage);

                // Run the trained output from #1
                // (This will be abstracted hopefully in the framework)
                var Index_AddAnyItemToCartEnvironment = Environments.Index_LoginAndAddItemToCart(driver);
                var indexState = await Index_AddAnyItemToCartEnvironment.GetCurrentState();
                var pathFinder = new RLPathFinder<IReadOnlyCollection<ElementData>>(Index_AddAnyItemToCartEnvironment, Index_AddAnyItemToCartSeleniumExperimentState);
                var pathList = await pathFinder.FindRoute(indexState, Goals.DoesCartHasOneItem(driver));
                if (pathList.State != PathFindResultState.GoalReached)
                {
                    throw new InvalidOperationException();
                }

                // We should have an item in the cart now

                // Got to cart
                driver.Navigate().GoToUrl(Data.CartPage);

                // Run the trained output from #2
                var Cart_CheckoutEnvironment = Environments.Cart_CheckOut(driver, Index_AddAnyItemToCartSeleniumExperimentState);
                var cartState = await Cart_CheckoutEnvironment.GetCurrentState();
                pathFinder = new RLPathFinder<IReadOnlyCollection<ElementData>>(Cart_CheckoutEnvironment, Cart_CheckOutSeleniumExperimentState);
                pathList = await pathFinder.FindRoute(cartState, Goals.HasOrderBeenPosted(driver));
                if (pathList.State != PathFindResultState.GoalReached)
                {
                    throw new InvalidOperationException();
                }

                // We should be on the checkout page now

                var target = driver.FindElement(By.CssSelector("h2.complete-header"));
                target.Text.ShouldBe("THANK YOU FOR YOUR ORDER");
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }
    }
}
