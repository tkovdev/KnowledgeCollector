using OpenAI.Chat;
using System.Text.Json;

namespace Data.Mappers;

public static class SessionMapper
{
    public static string ToJson(Session session)
    {
        ArgumentNullException.ThrowIfNull(session);

        return JsonSerializer.Serialize(new
        {
            Id = session.Id.ToString(),
            session.UserId,
            session.Messages
        }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    public static SessionMessage ToStored(ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var stored = new SessionMessage
        {
            TextParts = message.Content.Select(part =>
            {
                if (part.Kind != ChatMessageContentPartKind.Text)
                {
                    throw new NotSupportedException(
                        $"Content part kind '{part.Kind}' is not supported.");
                }

                return part.Text
                    ?? throw new InvalidOperationException("Text content is null.");
            }).ToList()
        };

        switch (message)
        {
            case SystemChatMessage system:
                stored.Role = "system";
                stored.ParticipantName = system.ParticipantName;
                break;

            case UserChatMessage user:
                stored.Role = "user";
                stored.ParticipantName = user.ParticipantName;
                break;

            case AssistantChatMessage assistant:
                stored.Role = "assistant";
                stored.ParticipantName = assistant.ParticipantName;

                if (assistant.Refusal is not null)
                {
                    throw new NotSupportedException(
                        "Assistant refusals are not supported by this mapper.");
                }

#pragma warning disable CS0618
                if (assistant.FunctionCall is not null)
                {
                    throw new NotSupportedException(
                        "Legacy function calls are not supported by this mapper.");
                }
#pragma warning restore CS0618

                stored.ToolCalls = assistant.ToolCalls.Select(call =>
                {
                    if (call.Kind != ChatToolCallKind.Function)
                    {
                        throw new NotSupportedException(
                            $"Tool call kind '{call.Kind}' is not supported.");
                    }

                    return new SessionToolCall
                    {
                        Id = RequireValue(call.Id, "Tool call ID"),
                        FunctionName = RequireValue(
                            call.FunctionName, "Tool function name"),
                        ArgumentsJson = call.FunctionArguments?.ToString()
                            ?? throw new InvalidOperationException(
                                "Tool function arguments are null.")
                    };
                }).ToList();
                break;

            case ToolChatMessage tool:
                stored.Role = "tool";
                stored.ToolCallId = RequireValue(tool.ToolCallId, "Tool call ID");
                break;

            default:
                throw new NotSupportedException(
                    $"Message type '{message.GetType().Name}' is not supported.");
        }

        return stored;
    }

    public static ChatMessage ToSdk(SessionMessage stored)
    {
        ArgumentNullException.ThrowIfNull(stored);

        var textParts = stored.TextParts
            ?? throw new InvalidOperationException("TextParts is null.");
        var toolCalls = stored.ToolCalls
            ?? throw new InvalidOperationException("ToolCalls is null.");

        var parts = textParts.Select(text =>
            ChatMessageContentPart.CreateTextPart(
                text ?? throw new InvalidOperationException(
                    "A text part is null.")))
            .ToArray();

        switch (stored.Role)
        {
            case "system":
                RequireTextParts(parts, stored.Role);
                return new SystemChatMessage(parts)
                {
                    ParticipantName = stored.ParticipantName
                };

            case "user":
                RequireTextParts(parts, stored.Role);
                return new UserChatMessage(parts)
                {
                    ParticipantName = stored.ParticipantName
                };

            case "assistant":
                return CreateAssistant(stored, parts, toolCalls);

            case "tool":
                RequireTextParts(parts, stored.Role);
                return new ToolChatMessage(
                    RequireValue(stored.ToolCallId, "Tool call ID"),
                    parts);

            default:
                throw new NotSupportedException(
                    $"Stored message role '{stored.Role}' is not supported.");
        }
    }

    private static AssistantChatMessage CreateAssistant(
        SessionMessage stored,
        ChatMessageContentPart[] parts,
        List<SessionToolCall> storedCalls)
    {
        var calls = storedCalls.Select(call =>
            ChatToolCall.CreateFunctionToolCall(
                RequireValue(call.Id, "Tool call ID"),
                RequireValue(call.FunctionName, "Tool function name"),
                BinaryData.FromString(
                    call.ArgumentsJson
                    ?? throw new InvalidOperationException(
                        "Tool function arguments are null."))))
            .ToList();

        AssistantChatMessage assistant;

        if (parts.Length > 0)
        {
            assistant = new AssistantChatMessage(parts);

            foreach (var call in calls)
            {
                assistant.ToolCalls.Add(call);
            }
        }
        else if (calls.Count > 0)
        {
            assistant = new AssistantChatMessage(calls);
        }
        else
        {
            throw new InvalidOperationException(
                "An assistant message must have text or tool calls.");
        }

        assistant.ParticipantName = stored.ParticipantName;
        return assistant;
    }

    private static void RequireTextParts(
        ChatMessageContentPart[] parts,
        string role)
    {
        if (parts.Length == 0)
        {
            throw new InvalidOperationException(
                $"A '{role}' message must have at least one text part.");
        }
    }

    private static string RequireValue(string? value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{field} is required.");
        }

        return value;
    }
}