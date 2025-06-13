//using System.Text.Json;
//using System.Text.Json.Serialization;
//using System.Text.Json.Serialization.Metadata;

//namespace Rag.Chat.Core.Serialization;

//public class ChatHistoryTypeInfoResolver : DefaultJsonTypeInfoResolver
//{
//    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
//    {
//        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

//        Type baseValueObjectType = typeof(ValueObject);
//        if (jsonTypeInfo.Type == baseValueObjectType)
//        {
//            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
//            {
//                TypeDiscriminatorPropertyName = "$mytype",
//                IgnoreUnrecognizedTypeDiscriminators = true,
//                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
//                DerivedTypes = { }
//            };
//        }

//        return jsonTypeInfo;
//    }
//}

/*
 // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?source=recommendations&pivots=dotnet-7-0#configure-polymorphism-with-the-contract-model
public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        Type basePointType = typeof(BasePoint);
        if (jsonTypeInfo.Type == basePointType)
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$point-type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                DerivedTypes =
                {
                    new JsonDerivedType(typeof(ThreeDimensionalPoint), "3d"),
                    new JsonDerivedType(typeof(FourDimensionalPoint), "4d")
                }
            };
        }

        return jsonTypeInfo;
    }
}
*/
