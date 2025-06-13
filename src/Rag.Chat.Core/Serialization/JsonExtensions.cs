using Microsoft.SemanticKernel;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Rag.Chat.Core.Serialization
{
    public static class JsonExtensions
    {
        public static void AddNativePolymorphicTypeInfo(JsonTypeInfo jsonTypeInfo)
        {
            Type baseValueObjectType = typeof(KernelContent);
            if (jsonTypeInfo.Type == baseValueObjectType)
            {
                jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "$type",
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                };

                //var assembly = Assembly.GetExecutingAssembly();
                var assembly = typeof(KernelContent).Assembly;

                var types = assembly
                    .GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(KernelContent)));

                foreach (var t in types.Select(t => new JsonDerivedType(t, t.Name.ToLowerInvariant())))
                {
                    jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(t);
                }
            }
        }
    }
}
