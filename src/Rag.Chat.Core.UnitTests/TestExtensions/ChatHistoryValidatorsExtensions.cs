using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Models;

namespace Rag.Chat.Core.UnitTests.TestExtensions;

public static class ChatHistoryValidatorsExtensions
{
    public static void MatchesExpectedValue(
        this ChatHistory chatHistory,
        ChatHistory expectedChatHistory)
    {
        chatHistory.Count.Should().Be(expectedChatHistory.Count);
        for (var i = 0; i < chatHistory.Count; i++)
        {
            chatHistory[i].Role.Label.Should().Be(expectedChatHistory[i].Role.Label);
            chatHistory[i].Content.Should().Be(expectedChatHistory[i].Content);
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            chatHistory[i].AuthorName.Should().Be(expectedChatHistory[i].AuthorName);
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            chatHistory[i].Items.Count.Should().Be(expectedChatHistory[i].Items.Count);
            chatHistory[i].Items.OfType<TextContent>().Single().Text.Should().Be(expectedChatHistory[i].Items.OfType<TextContent>().Single().Text);
        }
    }

    public static void MatchesExpectedValue(
        this UserChatHistoryContainer chatHistoryContainer,
        UserChatHistoryContainer expectedChatHistoryContainer,
        Guid expectedId,
        Guid expectedUserId)
    {
        chatHistoryContainer.Id.Should().Be(expectedId.ToString());
        chatHistoryContainer.UserId.Should().Be(expectedUserId.ToString());

        expectedChatHistoryContainer.Id.Should().Be(expectedId.ToString());
        expectedChatHistoryContainer.UserId.Should().Be(expectedUserId.ToString());

        chatHistoryContainer.ChatHistory.Should().NotBeNull();
        chatHistoryContainer.ChatHistory.Count.Should().Be(expectedChatHistoryContainer.ChatHistory.Count);

        for (var i = 0; i < chatHistoryContainer.ChatHistory.Count; i++)
        {
            var expectedChatHistory = expectedChatHistoryContainer.ChatHistory[i];
            chatHistoryContainer.ChatHistory[i].Role.Label.Should().Be(expectedChatHistory.Role.Label);
            chatHistoryContainer.ChatHistory[i].Content.Should().Be(expectedChatHistory.Content);
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            chatHistoryContainer.ChatHistory[i].AuthorName.Should().Be(expectedChatHistory.AuthorName);
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            chatHistoryContainer.ChatHistory[i].Items.Count.Should().Be(expectedChatHistory.Items.Count);
            chatHistoryContainer.ChatHistory[i].Items.OfType<TextContent>().Single().Text.Should().Be(expectedChatHistory.Items.OfType<TextContent>().Single().Text);
        }
    }
}
