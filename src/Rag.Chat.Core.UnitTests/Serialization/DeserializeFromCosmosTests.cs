using Rag.Chat.Core.Models;
using Rag.Chat.Core.Serialization;
using System.Text;
using System.Text.Json;

namespace Rag.Chat.Core.UnitTests.Serialization;

public class CosmosSystemTextJsonSerializerTests
{
    private readonly JsonSerializerOptions _serializerOptions;

    public CosmosSystemTextJsonSerializerTests()
    {
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            //TypeInfoResolver =
            //https://stackoverflow.com/questions/77809445/custom-typeinforesolver-to-manage-polymorphic-deserialization-in-system-text-jso
            //TypeInfoResolver = new DefaultJsonTypeInfoResolver() // Or use some source-generated context if you prefer
            //    .WithAddedModifier(JsonExtensions.AddNativePolymorphicTypeInfo),
        };
    }

    [Fact]
    public void Can_Deserialize_Cosmos_Chat_History()
    {
        // Arrange
        var jsonFile = Path.Combine(Environment.CurrentDirectory, "TestData", "chatHistory.json");
        var json = File.ReadAllText(jsonFile);
        json.Should().NotBeNull();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        stream.Seek(0, SeekOrigin.Begin);
        
        var serializer = new CosmosSystemTextJsonSerializer(_serializerOptions);

        // Act
        var chatHistoryDeserialized = serializer.FromStream<UserChatHistoryContainer>(stream);

        /*
         * The JSON payload for polymorphic interface or abstract type 'Microsoft.SemanticKernel.KernelContent' must specify a type discriminator
         * 
            [JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(KernelContent), UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
            [JsonDerivedType(typeof(ChatMessageContent), nameof(ChatMessageContent))]

        https://github.com/microsoft/semantic-kernel/issues/7478
        https://github.com/microsoft/semantic-kernel/pull/8901
         
         https://stackoverflow.com/questions/77809445/custom-typeinforesolver-to-manage-polymorphic-deserialization-in-system-text-jso
         
         */

        // Assert
        chatHistoryDeserialized.Should().NotBeNull();

    }
}
