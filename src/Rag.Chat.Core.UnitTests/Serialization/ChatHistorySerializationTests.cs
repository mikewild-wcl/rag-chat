using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Serialization;
using Rag.Chat.Core.UnitTests.TestExtensions;
using System.Text;
using System.Text.Json;

namespace Rag.Chat.Core.UnitTests.Serialization
{
    public class ChatHistorySerializationTests
    {
        private readonly ChatHistory _chatHistory;
        private readonly JsonSerializerOptions _serializerOptions;

        public ChatHistorySerializationTests()
        {
            _chatHistory = [];
            _chatHistory.AddSystemMessage("You are a helpful test assistant.");
            _chatHistory.AddUserMessage("Can you write some test for me?");
            _chatHistory.AddAssistantMessage("Sure thing.");
            _chatHistory.AddDeveloperMessage("Done.");

            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
        }

        [Fact]
        public void Can_Serialize_And_Deserialize_With_System_Text_Json()
        {
            // Arrange
            var json = JsonSerializer.Serialize(_chatHistory, _serializerOptions);

            // Act
            var chatHistoryDeserialized = JsonSerializer.Deserialize<ChatHistory>(json, _serializerOptions);

            // Assert
            chatHistoryDeserialized.Should().NotBeNull();
            chatHistoryDeserialized.Should().BeEquivalentTo(_chatHistory);

            chatHistoryDeserialized.MatchesExpectedValue(_chatHistory);
        }

        [Fact]
        public void Can_Serialize_And_Deserialize_With_CosmosSystemTextJsonSerializer()
        {
            // Arrange
            // https://stackoverflow.com/questions/30349695/could-not-create-an-instance-of-type-x-type-is-an-interface-or-abstract-class-a
            var serializerSettings = new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            };

            var serializer = new CosmosSystemTextJsonSerializer(_serializerOptions);

            using var stream = serializer.ToStream(_chatHistory);

            stream.Should().NotBeNull();

            using var reader = new StreamReader(stream, Encoding.UTF8);
            var json = reader.ReadToEnd();

            using var newStream = new MemoryStream();
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(newStream);
            newStream.Seek(0, SeekOrigin.Begin);

            // Act
            var chatHistoryDeserialized = serializer.FromStream<ChatHistory>(newStream);

            // Assert
            chatHistoryDeserialized.Should().NotBeNull();

            chatHistoryDeserialized.MatchesExpectedValue(_chatHistory);
        }
    }
}
