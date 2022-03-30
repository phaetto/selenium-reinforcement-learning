namespace Selenium.Algorithms.Serialization
{
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public sealed class ElementTypeActionConverter : JsonConverter<IAgentAction<IReadOnlyCollection<ElementData>>>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(ElementTypeAction);
        }

        public override IAgentAction<IReadOnlyCollection<ElementData>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            string? text = null;
            ElementData? elementData = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    if (elementData == null)
                    {
                        throw new InvalidOperationException();
                    }

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        throw new InvalidOperationException();
                    }

                    return new ElementTypeAction(elementData.Value, text);
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                string? propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case nameof(ElementTypeAction.WebElement):
                        var converter = (JsonConverter<ElementData>)options.GetConverter(typeof(ElementData));
                        elementData = converter.Read(ref reader, typeof(ElementData), options);
                        break;
                    case nameof(ElementTypeAction.Text):
                        text = reader.GetString();
                        break;
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, IAgentAction<IReadOnlyCollection<ElementData>> value, JsonSerializerOptions options)
        {
            var elementClickAction = (ElementTypeAction)value;
            writer.WriteStartObject();

            writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(elementClickAction.WebElement)) ?? nameof(elementClickAction.WebElement));
            JsonSerializer.Serialize(writer, elementClickAction.WebElement, options);

            writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(elementClickAction.Text)) ?? nameof(elementClickAction.Text));
            JsonSerializer.Serialize(writer, elementClickAction.Text, options);

            writer.WriteEndObject();
        }
    }
}
