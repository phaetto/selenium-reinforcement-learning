namespace Selenium.Algorithms.Serialization
{
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public sealed class SeleniumStateConverter : JsonConverter<IState<IReadOnlyCollection<ElementData>>>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(SeleniumState);
        }

        public override IState<IReadOnlyCollection<ElementData>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            IReadOnlyCollection<ElementData>? readOnlyCollection = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    if (readOnlyCollection == null)
                    {
                        throw new InvalidOperationException();
                    }

                    return new SeleniumState(readOnlyCollection);
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                string? propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case nameof(SeleniumState.Data):
                        var converter = (JsonConverter<IReadOnlyCollection<ElementData>>)options.GetConverter(typeof(IReadOnlyCollection<ElementData>));
                        readOnlyCollection = converter.Read(ref reader, typeof(IReadOnlyCollection<ElementData>), options);
                        break;
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, IState<IReadOnlyCollection<ElementData>> value, JsonSerializerOptions options)
        {
            var seleniumState = (SeleniumState)value;
            writer.WriteStartObject();

            writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(seleniumState.Data)) ?? nameof(seleniumState.Data));
            JsonSerializer.Serialize(writer, seleniumState.Data, options);

            writer.WriteEndObject();
        }
    }
}
