namespace Selenium.Algorithms.ReinforcementLearning.Serialization
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public sealed class TrainGoalConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType || typeToConvert.GetGenericTypeDefinition() != typeof(ITrainGoal<>))
            {
                var interfaces = typeToConvert.GetInterfaces();

                if (!interfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ITrainGoal<>)))
                {
                    return false;
                }
            }

            return true;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var type = typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(ITrainGoal<>)
                ? typeToConvert
                : typeToConvert.GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ITrainGoal<>));
            var internalType = type.GetGenericArguments()[0];

            var converter = (JsonConverter)Activator.CreateInstance(
                typeof(TrainGoalConverter<>).MakeGenericType(new[] { internalType }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] {},
                culture: null)!;

            return converter;
        }

        private sealed class TrainGoalConverter<TData> : JsonConverter<ITrainGoal<TData>>
        {
            private const string ObjectPropertyName = "Object";
            private const string TypePropertyName = "Type";

            public override ITrainGoal<TData>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                string? typeName = null;
                ITrainGoal<TData>? trainGoal = null;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        if (trainGoal == null)
                        {
                            throw new InvalidOperationException();
                        }

                        return trainGoal;
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    string? propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName)
                    {
                        case TypePropertyName:
                            typeName = reader.GetString();
                            break;
                        case ObjectPropertyName:
                            if (string.IsNullOrWhiteSpace(typeName))
                            {
                                throw new InvalidOperationException("Type must come first in properties");
                            }

                            var type = Type.GetType(typeName);
                            var converter = options.GetConverter(type);
                            if (converter is JsonConverter<ITrainGoal<TData>> trainGoalConverter)
                            {
                                trainGoal = trainGoalConverter.Read(ref reader, typeof(ITrainGoal<TData>), options);
                            }
                            else
                            {
                                var trainGoalObject = JsonSerializer.Deserialize(ref reader, type, options);
                                if (trainGoalObject == null)
                                {
                                    throw new InvalidOperationException();
                                }
                                trainGoal = (ITrainGoal<TData>)trainGoalObject;
                            }
                            
                            break;
                    }
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, ITrainGoal<TData> objectValue, JsonSerializerOptions options)
            {
                var objectValueType = objectValue.GetType();

                writer.WriteStartObject();

                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(TypePropertyName) ?? TypePropertyName);
                writer.WriteStringValue(objectValueType.AssemblyQualifiedName);

                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(ObjectPropertyName) ?? ObjectPropertyName);
                var converter = options.GetConverter(objectValueType);
                if (converter is JsonConverter<ITrainGoal<TData>> trainGoalConverter)
                {
                    trainGoalConverter.Write(writer, objectValue, options);
                }
                else
                {
                    JsonSerializer.Serialize(writer, objectValue, objectValue.GetType(), options);
                }

                writer.WriteEndObject();
            }
        }
    }
}
