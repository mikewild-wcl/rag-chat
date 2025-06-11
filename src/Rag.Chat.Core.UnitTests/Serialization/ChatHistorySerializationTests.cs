using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Rag.Chat.Core.Serialization;
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

            ValidateChatHistory(chatHistoryDeserialized);
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

            var newStream = new MemoryStream();
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(newStream);
            newStream.Seek(0, SeekOrigin.Begin);

            // Act
            var chatHistoryDeserialized = serializer.FromStream<ChatHistory>(newStream);

            // Assert
            chatHistoryDeserialized.Should().NotBeNull();

            ValidateChatHistory(chatHistoryDeserialized);
        }

        private void ValidateChatHistory(ChatHistory chatHistory)
        {
            chatHistory.Count.Should().Be(_chatHistory.Count);
            for (var i = 0; i < chatHistory.Count; i++)
            {
                chatHistory[i].Role.Label.Should().Be(_chatHistory[i].Role.Label);
                chatHistory[i].Content.Should().Be(_chatHistory[i].Content);
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                chatHistory[i].AuthorName.Should().Be(_chatHistory[i].AuthorName);
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                chatHistory[i].Items.Count.Should().Be(_chatHistory[i].Items.Count);
                chatHistory[i].Items.OfType<TextContent>().Single().Text.Should().Be(_chatHistory[i].Items.OfType<TextContent>().Single().Text);
            }
        }
    }
}
