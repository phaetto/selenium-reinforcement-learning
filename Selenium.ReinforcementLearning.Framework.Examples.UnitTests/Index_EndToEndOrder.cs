namespace Selenium.ReinforcementLearning.Framework.Examples.UnitTests
{
    using Selenium.ReinforcementLearning.Framework.Examples.UnitTests.Framework;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using Shouldly;
    using OpenQA.Selenium;
    using System.Linq;
    using Selenium.Algorithms;

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

                var result = await driver.Train(
                    nameof(_1_Index_LoginAndAddItemToCart),
                    random,
                    Environments.Index_LoginAndAddItemToCart(driver),
                    Goals.DoesCartHasOneItem(),
                    Enumerable.Empty<ITrainedInput>(),
                    testFixture.GetPersistenceProvider(),
                    40,
                    maxActions: 100,
                    epochCleanupFunction: () =>
                    {
                        driver.Navigate().GoToUrl(Data.CartPage);
                        var removeButtons = driver.FindElements(By.CssSelector(".cart_button"));
                        foreach (var removeButton in removeButtons)
                        {
                            removeButton.Click();
                        }

                        return Task.CompletedTask;
                    }
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

                var result = await driver.Train(
                    nameof(_2_Cart_CheckOut),
                    random,
                    Environments.Cart_CheckOut(driver),
                    Goals.HasOrderBeenPosted(),
                    new[] {
                        Index_AddAnyItemToCartSelenium
                    },
                    testFixture.GetPersistenceProvider(),
                    40,
                    maxActions: 100,
                    maxDependencySteps: 50,
                    epochCleanupFunction: () =>
                    {
                        driver.Navigate().GoToUrl(Data.CartPage);
                        var removeButtons = driver.FindElements(By.CssSelector(".cart_button"));
                        foreach (var removeButton in removeButtons)
                        {
                            removeButton.Click();
                        }

                        return Task.CompletedTask;
                    }
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

            try
            {
                // Go to homepage
                driver.Navigate().GoToUrl(Data.HomePage);

                // Navigate to add an item to cart with our pre trained input:
                await driver.Navigate(
                    Index_AddAnyItemToCartSelenium,
                    testFixture.GetPersistenceProvider()
                );

                // We should have an item in the cart now
                var shoppingCart = driver.FindElement(By.CssSelector(".shopping_cart_badge"));
                shoppingCart.Text.ShouldBe("1");

                // Got to cart page
                driver.Navigate().GoToUrl(Data.CartPage);

                var removeButtons = driver.FindElements(By.CssSelector(".cart_button"));
                removeButtons.ShouldNotBeEmpty("No items found in the cart - something went wrong!");

                // Navigate to checkout using our pre trained checkout input:
                await driver.Navigate(
                    Cart_CheckOutSelenium,
                    testFixture.GetPersistenceProvider()
                );

                // We should be on the checkout page now - verify

                var target = driver.FindElement(By.CssSelector("h2.complete-header"));
                target.Text.ShouldBe("Thank you for your order!");
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }

        [TrainedFact]
        [TrainedInput(nameof(_1_Index_LoginAndAddItemToCart))]
        public async Task _4_EndToEndTest_CanWeAddMultipleItemsInCart(
            ITrainedInput Index_AddAnyItemToCartSelenium)
        {
            /* 
             * Parameterization example
             * This test uses the trained data to enter two items in the cart.
             * The main trained data have only trained with a specific item button, so we need to parameterize
             *   the existing actions, providing a searcher. This allows us to select a specific element
             *   in the flow, even if there has been many other elements and different goals
             * */

            using var driver = testFixture.GetWebDriver();

            try
            {
                // Go to homepage
                driver.Navigate().GoToUrl(Data.HomePage);

                // Login and add an item to cart with our pre trained input
                // Plot twist: Choose the first item instead in the list of all items - not necessary the one we trained it with!
                await driver.Navigate(
                    Index_AddAnyItemToCartSelenium,
                    testFixture.GetPersistenceProvider(),
                    seleniumTrainGoal: Goals.DoesCartHasOneItem(),
                    // When this class is going to be visible on page, we will instead click the first item
                    parameters: new[] { new ClassContainsParameter("btn_inventory", 0) }
                );

                // We should have an item in the cart now
                var shoppingCart = driver.FindElement(By.CssSelector(".shopping_cart_badge"));
                shoppingCart.Text.ShouldBe("1");

                // Add a second item
                // Notice that event if the test has been written from login to adding to cart,
                //   the algorithm will check what state we are (and we are in the inventory page)
                //   and do the necessary actions there.
                await driver.Navigate(
                    Index_AddAnyItemToCartSelenium,
                    testFixture.GetPersistenceProvider(),
                    seleniumTrainGoal: Goals.DoesCartHasTwoItems(),
                    // When this class is going to be visible on page, we will instead click the second item
                    parameters: new[] { new ClassContainsParameter("btn_inventory", 1) }
                );

                // We should have now 2 items on our cart

                shoppingCart = driver.FindElement(By.CssSelector(".shopping_cart_badge"));
                shoppingCart.Text.ShouldBe("2");
                driver.Navigate().GoToUrl(Data.CartPage);
                var removeButtons = driver.FindElements(By.CssSelector(".cart_button"));
                removeButtons.Count.ShouldBe(2);
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }
        }
    }
}
