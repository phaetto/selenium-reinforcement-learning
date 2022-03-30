namespace Selenium.Algorithms.ReinforcementLearning.Serialization
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public sealed class StateConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType || typeToConvert.GetGenericTypeDefinition() != typeof(IState<>))
            {
                var interfaces = typeToConvert.GetInterfaces();

                if (!interfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IState<>)))
                {
                    return false;
                }
            }

            return true;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var type = typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(IState<>)
                ? typeToConvert
                : typeToConvert.GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IState<>));
            var internalType = type.GetGenericArguments()[0];

            var converter = (JsonConverter)Activator.CreateInstance(
                typeof(AgentActionConverter<>).MakeGenericType(new[] { internalType }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] {},
                culture: null)!;

            return converter;
        }

        private sealed class AgentActionConverter<TData> : JsonConverter<IState<TData>>
        {
            private const string ObjectPropertyName = "Object";
            private const string TypePropertyName = "Type";

            public override IState<TData>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                string? typeName = null;
                IState<TData>? state = null;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        if (state == null)
                        {
                            throw new InvalidOperationException();
                        }

                        return state;
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
                            var converter = (JsonConverter<IState<TData>>) options.GetConverter(type);
                            state = converter.Read(ref reader, typeof(IState<TData>), options);
                            break;
                    }
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, IState<TData> objectValue, JsonSerializerOptions options)
            {
                var objectValueType = objectValue.GetType();

                writer.WriteStartObject();

                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(TypePropertyName) ?? TypePropertyName);
                writer.WriteStringValue(objectValueType.FullName);

                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(ObjectPropertyName) ?? ObjectPropertyName);
                var converter = (JsonConverter<IState<TData>>)options.GetConverter(objectValueType);
                converter.Write(writer, objectValue, options);

                writer.WriteEndObject();
            }
        }
    }
}
