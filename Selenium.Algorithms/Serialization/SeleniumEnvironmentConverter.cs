namespace Selenium.Algorithms.Serialization
{
    using OpenQA.Selenium;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public sealed class SeleniumEnvironmentConverter : JsonConverter<IEnvironment<IReadOnlyCollection<ElementData>>>
    {
        private readonly IWebDriver webDriver;
        private readonly IJavaScriptExecutor javaScriptExecutor;

        public SeleniumEnvironmentConverter(IWebDriver webDriver, IJavaScriptExecutor javaScriptExecutor)
        {
            this.webDriver = webDriver;
            this.javaScriptExecutor = javaScriptExecutor;
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(SeleniumEnvironment);
        }

        public override IEnvironment<IReadOnlyCollection<ElementData>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            SeleniumEnvironmentOptions? environmentOptions = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    if (environmentOptions == null)
                    {
                        throw new InvalidOperationException();
                    }

                    return new SeleniumEnvironment(webDriver, javaScriptExecutor, environmentOptions);
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                string? propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case nameof(SeleniumEnvironment.Options):
                        var converter = (JsonConverter<SeleniumEnvironmentOptions>)options.GetConverter(typeof(SeleniumEnvironmentOptions));
                        environmentOptions = converter.Read(ref reader, typeof(SeleniumEnvironmentOptions), options);
                        break;
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, IEnvironment<IReadOnlyCollection<ElementData>> value, JsonSerializerOptions options)
        {
            var environment = (SeleniumEnvironment)value;
            writer.WriteStartObject();

            writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(environment.Options)) ?? nameof(environment.Options));
            JsonSerializer.Serialize(writer, environment.Options, options);

            writer.WriteEndObject();
        }
    }
}
