namespace Selenium.Algorithms.ReinforcementLearning.Serialization
{
    using System;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public sealed class StateAndActionConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(StateAndActionPair<>))
            {
                return true;
            }

            if (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(StateAndActionPairWithResultState<>))
            {
                return true;
            }

            return false;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert.IsGenericType && 
                (typeToConvert.GetGenericTypeDefinition() == typeof(StateAndActionPair<>) || typeToConvert.GetGenericTypeDefinition() == typeof(StateAndActionPairWithResultState<>))
                )
            {
                var internalType = typeToConvert.GetGenericArguments()[0];

                var converter = (JsonConverter)Activator.CreateInstance(
                    typeof(StateAndActionConverter<>).MakeGenericType(new Type[] { internalType }),
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    args: new object[] { },
                    culture: null)!;

                return converter;
            }

            throw new NotSupportedException();
        }

        private sealed class StateAndActionConverter<TData> : JsonConverter<StateAndActionPair<TData>>
        {
            public override StateAndActionPair<TData>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                IState<TData>? state = null;
                IAgentAction<TData>? agentAction = null;
                IState<TData>? resultState = null;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        if (agentAction == null)
                        {
                            throw new InvalidOperationException();
                        }

                        if (state == null)
                        {
                            throw new InvalidOperationException();
                        }

                        if (resultState == null)
                        {
                            return new StateAndActionPair<TData>(state, agentAction);
                        }
                        else
                        {
                            return new StateAndActionPairWithResultState<TData>(state, agentAction, resultState);
                        }
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    string? propertyName = reader.GetString();

                    switch (propertyName)
                    {
                        case nameof(StateAndActionPair<TData>.State):
                            reader.Read();
                            var stateConverter = (JsonConverter<IState<TData>>)options.GetConverter(typeof(IState<TData>));
                            state = stateConverter.Read(ref reader, typeof(IState<TData>), options);
                            break;
                        case nameof(StateAndActionPair<TData>.Action):
                            reader.Read();
                            var agentActionConverter = (JsonConverter<IAgentAction<TData>>)options.GetConverter(typeof(IAgentAction<TData>));
                            agentAction = agentActionConverter.Read(ref reader, typeof(IAgentAction<TData>), options);
                            break;
                        case nameof(StateAndActionPairWithResultState<TData>.ResultState):
                            reader.Read();
                            var resultStateConverter = (JsonConverter<IState<TData>>)options.GetConverter(typeof(IState<TData>));
                            resultState = resultStateConverter.Read(ref reader, typeof(IState<TData>), options);
                            break;
                    }
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, StateAndActionPair<TData> objectValue, JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                // State
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(objectValue.State)) ?? nameof(objectValue.State));
                var stateType = typeof(IState<>).MakeGenericType(new[] { typeof(TData) });
                var stateConverter = (JsonConverter<IState<TData>>)options.GetConverter(stateType);
                stateConverter.Write(writer, objectValue.State, options);

                // Action
                writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(objectValue.Action)) ?? nameof(objectValue.Action));
                var agentActionType = typeof(IAgentAction<>).MakeGenericType(new[] { typeof(TData) });
                var agentActionConverter = (JsonConverter<IAgentAction<TData>>)options.GetConverter(agentActionType);
                agentActionConverter.Write(writer, objectValue.Action, options);

                // Result State (if possible)
                if (objectValue is StateAndActionPairWithResultState<TData> stateAndActionPairWithResultData)
                {
                    writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(nameof(stateAndActionPairWithResultData.ResultState)) ?? nameof(stateAndActionPairWithResultData.ResultState));
                    JsonSerializer.Serialize(writer, stateAndActionPairWithResultData.ResultState, options);
                }

                writer.WriteEndObject();

            }
        }
    }
}
