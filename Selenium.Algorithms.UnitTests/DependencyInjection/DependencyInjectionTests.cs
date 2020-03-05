namespace Selenium.Algorithms.IntegrationTests.DependencyInjection
{
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using OpenQA.Selenium;
    using Selenium.Algorithms.ReinforcementLearning;
    using System.Collections.Generic;
    using Xunit;

    /// <summary>
    /// DI tests are here to ensure architectural integrity
    /// </summary>
    public sealed class DependencyInjectionTests
    {
        [Fact]
        public void RLTrainer_WhenUsedWithDI_ThenIsDIFriendly()
        {
            var services = new ServiceCollection();
            services.AddTransient<IRLTrainer<int>, RLTrainer<int>>();
            services.AddSingleton(new Mock<IRLTrainerOptions<int>>().Object);

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<IRLTrainer<int>>();
        }

        [Fact]
        public void SeleniumEnvironment_WhenUsedWithDI_ThenIsDIFriendly()
        {
            var services = new ServiceCollection();
            services.AddTransient<IEnvironment<IReadOnlyCollection<ElementData>>, SeleniumEnvironment>();
            services.AddSingleton(new Mock<ISeleniumEnvironmentOptions>().Object);
            services.AddSingleton(new Mock<IWebDriver>().Object);
            services.AddSingleton(new Mock<IJavaScriptExecutor>().Object);

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<IEnvironment<IReadOnlyCollection<ElementData>>>();
        }
    }
}
