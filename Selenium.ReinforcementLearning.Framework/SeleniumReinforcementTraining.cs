namespace Selenium.ReinforcementLearning.Framework
{
    using OpenQA.Selenium;
    using Selenium.Algorithms;
    using Selenium.Algorithms.ReinforcementLearning;
    using Selenium.Algorithms.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// Contains ready made actions that involve training and executing using best practices
    /// </summary>
    public static class SeleniumReinforcementTraining
    {
        public static async Task<TrainerReport> Train(
            IWebDriver webDriver,
            string experimentName,
            Random random,
            SeleniumEnvironment seleniumEnvironment,
            ITrainGoal<IReadOnlyCollection<ElementData>> seleniumTrainGoal,
            IEnumerable<ITrainedInput> trainedInputs,
            IPersistenceIO persistenceIO,
            int maxIterations,
            int maxActions = 1000,
            int maxDependencySteps = 1000)
        {
            var seleniumRandomStepPolicy = new SeleniumQLearningStepPolicy(random);
            var seleniumExperimentState = new SeleniumExperimentState();
            var experimentDependencies = await trainedInputs
                .SelectAsync(async x => (await x.GetExperiment(webDriver, persistenceIO)).ToDependency(maxDependencySteps));
            var rlTrainer = new RLTrainer<IReadOnlyCollection<ElementData>>(new RLTrainerOptions<IReadOnlyCollection<ElementData>>(
                seleniumEnvironment,
                seleniumRandomStepPolicy,
                seleniumExperimentState,
                seleniumTrainGoal,
                experimentDependencies));

            var iteration = 0;
            TrainerReport totalTrainerReport = new TrainerReport();
            var isTrainedGoodEnough = false;
            while (++iteration < maxIterations && !isTrainedGoodEnough)
            {
                totalTrainerReport += await rlTrainer.Run(epochs: 5, maximumActions: maxActions);

                if (totalTrainerReport.TimesReachedGoal > 0)
                {
                    var pathFinder = new RLPathFinder<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumExperimentState);
                    var initialState = await seleniumEnvironment.GetInitialState();
                    var pathList = await pathFinder.FindRoute(initialState, seleniumTrainGoal);
                    if (pathList.State == PathFindResultState.GoalReached)
                    {
                        isTrainedGoodEnough = true;
                    }
                }
            }

            var experiment = new Experiment<IReadOnlyCollection<ElementData>>(seleniumEnvironment, seleniumTrainGoal, seleniumExperimentState);
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.Converters.Add(new SeleniumEnvironmentConverter(webDriver, (IJavaScriptExecutor)webDriver));
            await persistenceIO.Write(experimentName, JsonSerializer.Serialize(experiment, jsonSerializerOptions));
            
            return totalTrainerReport;
        }

        public static async Task<WalkResult<IReadOnlyCollection<ElementData>>> Navigate(
            IWebDriver webDriver,
            ITrainedInput trainedInput,
            IPersistenceIO persistenceIO,
            int maxSteps = 1000,
            SeleniumEnvironment? seleniumEnvironment = null,
            ITrainGoal<IReadOnlyCollection<ElementData>>? seleniumTrainGoal = null)
        {
            var experiment = await trainedInput.GetExperiment(webDriver, persistenceIO);
            var selectedSeleniumEnvironment = seleniumEnvironment ?? experiment.Environment;
            var selectedSeleniumGoal = seleniumTrainGoal ?? experiment.TrainGoal;

            var currentState = await selectedSeleniumEnvironment.GetCurrentState();
            var pathFinder = new RLPathFinder<IReadOnlyCollection<ElementData>>(selectedSeleniumEnvironment, experiment.ExperimentState);
            var pathList = await pathFinder.FindRoute(currentState, selectedSeleniumGoal, maxSteps);
            if (pathList.State != PathFindResultState.GoalReached)
            {
                throw new InvalidOperationException();
            }
            return pathList;
        }

        private static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> selector)
        {
            var list = new List<TResult>();
            foreach (var item in source)
            {
                list.Add(await selector(item));
            }
            return list;
        }
    }
}
