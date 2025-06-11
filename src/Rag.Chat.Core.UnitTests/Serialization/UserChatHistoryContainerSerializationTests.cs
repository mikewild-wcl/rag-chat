using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Serialization;
using System.Text;
using System.Text.Json;

namespace Rag.Chat.Core.UnitTests.Serialization
{
    public class UserChatHistoryContainerSerializationTests
    {
        private readonly Guid _id = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();
        private readonly ChatHistory _chatHistory;
        private readonly UserChatHistoryContainer _chatHistoryContainer;
        private readonly JsonSerializerOptions _serializerOptions;

        public UserChatHistoryContainerSerializationTests()
        {
            _chatHistory = [];
            _chatHistory.AddSystemMessage("You are a helpful test assistant.");
            _chatHistory.AddUserMessage("Can you write some test for me?");
            _chatHistory.AddAssistantMessage("Sure thing.");
            _chatHistory.AddDeveloperMessage("Done.");

            _chatHistoryContainer = new UserChatHistoryContainer(
                _id.ToString(),
                _userId.ToString(),
                _chatHistory);

            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
        }

        [Fact]
        public void Can_Serialize_And_Deserialize_With_System_Text_Json()
        {
            // Arrange
            var json = JsonSerializer.Serialize(_chatHistoryContainer, _serializerOptions);

            // Act
            var chatHistoryContainerDeserialized = JsonSerializer.Deserialize<UserChatHistoryContainer>(json, _serializerOptions);

            // Assert
            chatHistoryContainerDeserialized.Should().NotBeNull();
            chatHistoryContainerDeserialized.Should().BeEquivalentTo(_chatHistoryContainer);

            ValidateChatHistoryContainer(chatHistoryContainerDeserialized);
        }

        [Fact]
        public void Can_Serialize_And_Deserialize_With_CosmosSystemTextJsonSerializer()
        {
            // Arrange
            var serializer = new CosmosSystemTextJsonSerializer(_serializerOptions);

            using var stream = serializer.ToStream(_chatHistoryContainer);

            stream.Should().NotBeNull();

            using var reader = new StreamReader(stream, Encoding.UTF8);
            var json = reader.ReadToEnd();

            var newStream = new MemoryStream();
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(newStream);
            newStream.Seek(0, SeekOrigin.Begin);

            // Act
            var chatHistoryDeserialized = serializer.FromStream<UserChatHistoryContainer>(newStream);

            // Assert
            chatHistoryDeserialized.Should().NotBeNull();

            ValidateChatHistoryContainer(chatHistoryDeserialized);
        }

        private void ValidateChatHistoryContainer(UserChatHistoryContainer chatHistoryContainer)
        {
            chatHistoryContainer.Id.Should().Be(_id.ToString());
            chatHistoryContainer.UserId.Should().Be(_userId.ToString());

            chatHistoryContainer.ChatHistory.Should().NotBeNull();
            chatHistoryContainer.ChatHistory.Count.Should().Be(_chatHistory.Count);

            for (var i = 0; i < chatHistoryContainer.ChatHistory.Count; i++)
            {
                chatHistoryContainer.ChatHistory[i].Role.Label.Should().Be(_chatHistory[i].Role.Label);
                chatHistoryContainer.ChatHistory[i].Content.Should().Be(_chatHistory[i].Content);
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                chatHistoryContainer.ChatHistory[i].AuthorName.Should().Be(_chatHistory[i].AuthorName);
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                chatHistoryContainer.ChatHistory[i].Items.Count.Should().Be(_chatHistory[i].Items.Count);
                chatHistoryContainer.ChatHistory[i].Items.OfType<TextContent>().Single().Text.Should().Be(_chatHistory[i].Items.OfType<TextContent>().Single().Text);
            }
        }
    }
}
