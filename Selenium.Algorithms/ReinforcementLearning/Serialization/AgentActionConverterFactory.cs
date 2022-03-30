namespace Selenium.Algorithms.ReinforcementLearning.Serialization
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public sealed class AgentActionConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType || typeToConvert.GetGenericTypeDefinition() != typeof(IAgentAction<>))
            {
                var interfaces = typeToConvert.GetInterfaces();

                if (!interfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAgentAction<>)))
                {
                    return false;
                }
            }

            return true;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var type = typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(IAgentAction<>)
                ? typeToConvert
                : typeToConvert.GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAgentAction<>));
            var internalType = type.GetGenericArguments()[0];

            var converter = (JsonConverter)Activator.CreateInstance(
                typeof(AgentActionConverter<>).MakeGenericType(new[] { internalType }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] {},
                culture: null)!;

            return converter;
        }

        private sealed class AgentActionConverter<TData> : JsonConverter<IAgentAction<TData>>
        {
            private const string ObjectPropertyName = "Object";
            private const string TypePropertyName = "Type";

            public override IAgentAction<TData>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                string? typeName = null;
                IAgentAction<TData>? agentAction = null;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        if (agentAction == null)
                        {
                            throw new InvalidOperationException();
                        }

                        return agentAction;
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
                            var converter = (JsonConverter<IAgentAction<TData>>) options.GetConverter(type);
                            agentAction = converter.Read(ref reader, typeof(IAgentAction<TData>), options);
                            break;
                    }
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, IAgentAction<TData> objectValue, JsonSerializerOptions options)
            {
                var objectValueType = objectValue.GetType();

                writer.WriteStartObject();

                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(TypePropertyName) ?? TypePropertyName);
                writer.WriteStringValue(objectValueType.FullName);

                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(ObjectPropertyName) ?? ObjectPropertyName);
                var converter = (JsonConverter<IAgentAction<TData>>)options.GetConverter(objectValueType);
                converter.Write(writer, objectValue, options);

                writer.WriteEndObject();
            }
        }
    }
}
