namespace Selenium.Algorithms.ReinforcementLearning.Serialization
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public sealed class ExperimentConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType || typeToConvert.GetGenericTypeDefinition() != typeof(Experiment<>))
            {
                var interfaces = typeToConvert.GetInterfaces();

                if (!interfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(Experiment<>)))
                {
                    return false;
                }
            }

            return true;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var type = typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Experiment<>)
                ? typeToConvert
                : typeToConvert.GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(Experiment<>));
            var internalType = type.GetGenericArguments()[0];

            var converter = (JsonConverter)Activator.CreateInstance(
                typeof(ExperimentConverter<>).MakeGenericType(new[] { internalType }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] {},
                culture: null)!;

            return converter;
        }

        private sealed class ExperimentConverter<TData> : JsonConverter<Experiment<TData>>
        {
            private const string TrainGoalTypePropertyName = "TrainGoalType";
            private const string EnvironmentTypePropertyName = "EnvironmentType";
            private const string EnvironmentObjectPropertyName = "EnvironmentObject";
            private const string TrainGoalObjectPropertyName = "TrainGoalObject";
            private const string ExperimentStateTypePropertyName = "ExperimentStateType";
            private const string ExperimentStateObjectPropertyName = "ExperimentStateObject";

            public override Experiment<TData>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                string? environmentTypeName = null;
                string? trainGoalTypeName = null;
                string? experimentStateTypeName = null;
                IEnvironment<TData>? environment = null;
                ITrainGoal<TData>? trainGoal = null;
                IExperimentState<TData>? experimentState = null;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        if (environment == null)
                        {
                            throw new InvalidOperationException();
                        }

                        if (trainGoal == null)
                        {
                            throw new InvalidOperationException();
                        }

                        if (experimentState == null)
                        {
                            throw new InvalidOperationException();
                        }

                        return new Experiment<TData>(environment, trainGoal, experimentState);
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    string? propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName)
                    {
                        case EnvironmentTypePropertyName:
                            environmentTypeName = reader.GetString();
                            break;
                        case EnvironmentObjectPropertyName:
                            if (string.IsNullOrWhiteSpace(environmentTypeName))
                            {
                                throw new InvalidOperationException("Type must come first in properties");
                            }

                            var environmentType = Type.GetType(environmentTypeName);
                            var environmentConverter = (JsonConverter<IEnvironment<TData>>)options.GetConverter(environmentType);
                            environment = environmentConverter.Read(ref reader, typeof(IEnvironment<TData>), options);
                            break;
                        case TrainGoalTypePropertyName:
                            trainGoalTypeName = reader.GetString();
                            break;
                        case TrainGoalObjectPropertyName:
                            if (string.IsNullOrWhiteSpace(trainGoalTypeName))
                            {
                                throw new InvalidOperationException("Type must come first in properties");
                            }

                            var trainGoalType = Type.GetType(environmentTypeName);
                            var trainGoalConverter = (JsonConverter<ITrainGoal<TData>>)options.GetConverter(typeof(ITrainGoal<TData>));
                            trainGoal = trainGoalConverter.Read(ref reader, trainGoalType, options);
                            break;
                        case ExperimentStateTypePropertyName:
                            experimentStateTypeName = reader.GetString();
                            break;
                        case ExperimentStateObjectPropertyName:
                            if (string.IsNullOrWhiteSpace(trainGoalTypeName))
                            {
                                throw new InvalidOperationException("Type must come first in properties");
                            }

                            var experimentStateType = Type.GetType(experimentStateTypeName);
                            var experimentStateConverter = (JsonConverter<IExperimentState<TData>>)options.GetConverter(experimentStateType);
                            experimentState = experimentStateConverter.Read(ref reader, experimentStateType, options);
                            break;
                    }
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, Experiment<TData> objectValue, JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(EnvironmentTypePropertyName) ?? EnvironmentTypePropertyName);
                writer.WriteStringValue(objectValue.Environment.GetType().AssemblyQualifiedName);
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(EnvironmentObjectPropertyName) ?? EnvironmentObjectPropertyName);
                var environmentConverter = (JsonConverter<IEnvironment<TData>>)options.GetConverter(objectValue.Environment.GetType());
                environmentConverter.Write(writer, objectValue.Environment, options);

                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(TrainGoalTypePropertyName) ?? TrainGoalTypePropertyName);
                writer.WriteStringValue(objectValue.TrainGoal.GetType().AssemblyQualifiedName);
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(TrainGoalObjectPropertyName) ?? TrainGoalObjectPropertyName);
                var traingGoalConverter = (JsonConverter<ITrainGoal<TData>>)options.GetConverter(typeof(ITrainGoal<TData>));
                traingGoalConverter.Write(writer, objectValue.TrainGoal, options);

                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(ExperimentStateTypePropertyName) ?? ExperimentStateTypePropertyName);
                writer.WriteStringValue(objectValue.ExperimentState.GetType().AssemblyQualifiedName);
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(ExperimentStateObjectPropertyName) ?? ExperimentStateObjectPropertyName);
                var experimentStateConverter = (JsonConverter<IExperimentState<TData>>)options.GetConverter(typeof(IExperimentState<TData>));
                experimentStateConverter.Write(writer, objectValue.ExperimentState, options);

                writer.WriteEndObject();
            }
        }
    }
}
