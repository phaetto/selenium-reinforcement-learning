namespace Selenium.ReinforcementLearning.Framework.Examples.UnitTests
{
    using Selenium.ReinforcementLearning.Framework.Examples.UnitTests.Framework;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using Shouldly;
    using System.Text.Json;
    using OpenQA.Selenium;
    using System.Linq;

    /// <summary>
    /// Simulates an order done in https://www.saucedemo.com/.
    /// The test trains 2 cases:
    ///  - Train to login and pick an item
    ///  - Taking an order from the basket to the end
    ///  
    /// Then uses those two cases to run a test without needing to explicitly point out the process to the bot.
    /// 
    /// </summary>
    [Collection("saucedemo_order")]
    [TestCaseOrderer(
        "Selenium.ReinforcementLearning.Framework.Examples.UnitTests.Framework.AlphabeticalOrderer",
        "Selenium.ReinforcementLearning.Framework.Examples.UnitTests"
    )]
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

        [Train]
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

                var result = await SeleniumReinforcementTraining.Train(
                    driver,
                    nameof(_1_Index_LoginAndAddItemToCart),
                    random,
                    Environments.Index_LoginAndAddItemToCart(driver),
                    Goals.DoesCartHasOneItem(),
                    Enumerable.Empty<ITrainedInput>(),
                    new FileIO(),
                    40
                );

                result.TimesReachedGoal.ShouldBePositive();
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }

        [Train]
        [TrainedInput(nameof(_1_Index_LoginAndAddItemToCart))]
        public async Task _2_Cart_CheckOut(ITrainedInput Index_AddAnyItemToCartSelenium)
        {
            /*
             * This test simulates the dependency on another trainer.
             * Uses the output from #1 to login and add to cart before checking out.
             * */

            using var driver = testFixture.GetWebDriver();

            try
            {
                var random = new Random(1);

                var result = await SeleniumReinforcementTraining.Train(
                    driver,
                    nameof(_2_Cart_CheckOut),
                    random,
                    Environments.Cart_CheckOut(driver),
                    Goals.HasOrderBeenPosted(),
                    new[] {
                        Index_AddAnyItemToCartSelenium
                    },
                    new FileIO(),
                    40,
                    maxDependencySteps: 50
                );

                result.TimesReachedGoal.ShouldBePositive();
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }

        [TrainedFact]
        [TrainedInput(nameof(_1_Index_LoginAndAddItemToCart))]
        [TrainedInput(nameof(_2_Cart_CheckOut))]
        public async Task _3_EndToEndTest_BuyAnItemAndCheckout(
            ITrainedInput Index_AddAnyItemToCartSelenium,
            ITrainedInput Cart_CheckOutSelenium)
        {
            /* 
             * This is a test that uses the trained data to navigate without explicitly specifying the steps.
             * We load those data from the json output of the previous tests.
             * */

            using var driver = testFixture.GetWebDriver();
            var fileIO = new FileIO();
            var indexExperiment = await Index_AddAnyItemToCartSelenium.GetExperiment(driver, fileIO);
            var cartExperiment = await Cart_CheckOutSelenium.GetExperiment(driver, fileIO);

            try
            {
                // Go to homepage
                driver.Navigate().GoToUrl(Data.HomePage);

                // Navigate to add an item to cart with our pre trained input:
                await SeleniumReinforcementTraining.Navigate(
                    driver,
                    Index_AddAnyItemToCartSelenium,
                    new FileIO(),
                    seleniumTrainGoal: Goals.IsInventoryVisible()
                );

                // We should have an item in the cart now

                // Got to cart page
                driver.Navigate().GoToUrl(Data.CartPage);

                // Navigate to checkout using our pre trained checkout input:
                await SeleniumReinforcementTraining.Navigate(
                    driver,
                    Cart_CheckOutSelenium,
                    new FileIO()
                );

                // We should be on the checkout page now - verify

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
