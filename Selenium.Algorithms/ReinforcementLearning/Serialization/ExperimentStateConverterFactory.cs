namespace Selenium.Algorithms.ReinforcementLearning.Serialization
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public sealed class ExperimentStateConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType || typeToConvert.GetGenericTypeDefinition() != typeof(IExperimentState<>))
            {
                var interfaces = typeToConvert.GetInterfaces();

                if (!interfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IExperimentState<>)))
                {
                    return false;
                }
            }

            return true;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var experimentStateType = typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(IExperimentState<>)
                ? typeToConvert
                : typeToConvert.GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IExperimentState<>));
            var internalType = experimentStateType.GetGenericArguments()[0];

            var converter = (JsonConverter)Activator.CreateInstance(
                typeof(ExperimentStateConverter<>).MakeGenericType(new[] { internalType }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { },
                culture: null)!;

            return converter;
        }

        private sealed class ExperimentStateConverter<TData> : JsonConverter<IExperimentState<TData>>
        {
            private const string KeyPropertyName = "Key";
            private const string ValuePropertyName = "Value";

            public override IExperimentState<TData>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                var experimentState = (IExperimentState<TData>)Activator.CreateInstance(
                    typeToConvert,
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    args: new object[] { },
                    culture: null)!;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return experimentState;
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    StateAndActionPair<TData>? stateAndActionPair = null;
                    double entryValue = 0D;
                    _ = reader.GetString(); // This is the hash number

                    reader.Read();

                    if (reader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new JsonException();
                    }

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject)
                        {
                            break;
                        }

                        if (reader.TokenType != JsonTokenType.PropertyName)
                        {
                            throw new JsonException();
                        }

                        string? dictionaryPropertyName = reader.GetString(); // This is the key/value
                        reader.Read();

                        switch (dictionaryPropertyName)
                        {
                            case KeyPropertyName:
                                var stateAndActionPairConverter = (JsonConverter<StateAndActionPair<TData>>)options.GetConverter(typeof(StateAndActionPair<TData>));
                                stateAndActionPair = stateAndActionPairConverter.Read(ref reader, typeof(JsonConverter<StateAndActionPair<TData>>), options);
                                break;
                            case ValuePropertyName:
                                entryValue = reader.GetDouble();
                                break;
                        }
                    }

                    if (stateAndActionPair == null)
                    {
                        throw new InvalidOperationException();
                    }

                    experimentState.QualityMatrix.Add(stateAndActionPair, entryValue);
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, IExperimentState<TData> objectValue, JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                foreach ((StateAndActionPair<TData> key, double value) in objectValue.QualityMatrix)
                {
                    var stateAndActionPairConverter = (JsonConverter<StateAndActionPair<TData>>)options.GetConverter(key.GetType());
                    var propertyName = key.GetHashCode().ToString(); // How we can convert the object in key?
                    writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName);

                    writer.WriteStartObject();

                    writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(ExperimentStateConverter<TData>.KeyPropertyName) ?? ExperimentStateConverter<TData>.KeyPropertyName);
                    stateAndActionPairConverter.Write(writer, key, options);

                    writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(ExperimentStateConverter<TData>.ValuePropertyName) ?? ExperimentStateConverter<TData>.ValuePropertyName);
                    JsonSerializer.Serialize(writer, value, options);

                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
            }
        }
    }
}
