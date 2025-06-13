using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Models;
using Rag.Chat.Core.Serialization;
using Rag.Chat.Core.UnitTests.TestExtensions;
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

            chatHistoryContainerDeserialized.MatchesExpectedValue(_chatHistoryContainer, _id, _userId);
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

            using var newStream = new MemoryStream();
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(newStream);
            newStream.Seek(0, SeekOrigin.Begin);

            // Act
            var chatHistoryContainerDeserialized = serializer.FromStream<UserChatHistoryContainer>(newStream);

            // Assert
            chatHistoryContainerDeserialized.Should().NotBeNull();
            chatHistoryContainerDeserialized.MatchesExpectedValue(_chatHistoryContainer, _id, _userId);
        }
    }
}
