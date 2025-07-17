using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnitV3;
using System.Text.RegularExpressions;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Rag.Chat.AppTests;

public partial class ArchitectureTests
{
    const int FluentAssertionsCommercialVersion = 8;

    [GeneratedRegex(@"Version=([0-9]+\.[0-9]+\.[0-9]+\.[0-9]+)")]
    private static partial Regex AssemblyVersionRegex();

    private static readonly Architecture Architecture = new ArchLoader().LoadAssemblies(
                System.Reflection.Assembly.Load("Rag.Chat.Core"),
                System.Reflection.Assembly.Load("Rag.Chat.Web")
            ).Build();

    private static readonly Architecture Tests = new ArchLoader().LoadAssemblies(
            System.Reflection.Assembly.GetExecutingAssembly(), // This assembly
            System.Reflection.Assembly.Load("Rag.Chat.Core.UnitTests")
        ).Build();

    //declare variables you'll use throughout your tests up here
    //use As() to give them a custom description
    private readonly IObjectProvider<IType> CoreLayer =
        Types().That().ResideInAssembly("Rag.Chat.Core").As("Core Layer");

    private readonly IObjectProvider<IType> WebLayer =
        Types().That().ResideInNamespace("Rag.Chat.Web").As("Forbidden Layer");

    /*
    private readonly IObjectProvider<Class> ExampleClasses =
        Classes().That().ImplementInterface("IDocumentIngestionService").As("Example Classes");


    private readonly IObjectProvider<Interface> ForbiddenInterfaces =
        Interfaces().That().HaveFullNameContaining("forbidden").As("Forbidden Interfaces");


    //write some tests
    [Fact]
    public void TypesShouldBeInCorrectLayer()
    {
        //you can use the fluent API to write your own rules
        IArchRule exampleClassesShouldBeInExampleLayer =
            Classes().That().Are(ExampleClasses).Should().Be(ExampleLayer);
        IArchRule forbiddenInterfacesShouldBeInForbiddenLayer =
            Interfaces().That().Are(ForbiddenInterfaces).Should().Be(ForbiddenLayer);

        //check if your architecture fulfils your rules
        exampleClassesShouldBeInExampleLayer.Check(Architecture);
        forbiddenInterfacesShouldBeInForbiddenLayer.Check(Architecture);

        //you can also combine your rules
        IArchRule combinedArchRule =
            exampleClassesShouldBeInExampleLayer.And(forbiddenInterfacesShouldBeInForbiddenLayer);
        combinedArchRule.Check(Architecture);
    }

    [Fact]
    public void ExampleLayerShouldNotAccessForbiddenLayer()
    {
        //you can give your rules a custom reason, which is displayed when it fails
        //(together with the types that failed the rule)
        IArchRule exampleLayerShouldNotAccessForbiddenLayer = Types().That().Are(ExampleLayer).Should()
            .NotDependOnAny(ForbiddenLayer).Because("it's forbidden");
        exampleLayerShouldNotAccessForbiddenLayer.Check(Architecture);
    }

    [Fact]
    public void ForbiddenClassesShouldHaveCorrectName()
    {
        Classes().That().AreAssignableTo(ForbiddenInterfaces).Should().HaveNameContaining("forbidden")
            .Check(Architecture);
    }

    [Fact]
    public void ExampleClassesShouldNotCallForbiddenMethods()
    {
        Classes().That().Are(ExampleClasses).Should().NotCallAny(
                MethodMembers().That().AreDeclaredIn(ForbiddenLayer).Or().HaveNameContaining("forbidden"))
            .Check(Architecture);
    }
    */

    [Fact]
    public void CoreLayer_ShouldNotAccess_WebLayer()
    {
        var exampleLayerShouldNotAccessForbiddenLayer = Types()
            .That()
            .Are(CoreLayer)
            .Should()
            .NotDependOnAny(WebLayer)
            .Because("it's forbidden")
            .WithoutRequiringPositiveResults();
        exampleLayerShouldNotAccessForbiddenLayer.Check(Architecture);
    }

    [Fact]
    public void FluentAssertions_InTestAssemblies_ShouldNot_BeCommercialVersion()
    {
        var fluentAssertionVersions = Tests.ReferencedTypes
            .Where(t => t.Assembly.FullName.StartsWith("FluentAssertions") == true)
            .Select(t => t.Assembly.FullName)
            //.Select(t => System.Reflection.Assembly.Load(t.FullName).GetName().Version.Major)
            .Distinct()
            .Select(ExtractVersionFromAssemblyName)
            .Where(v => v is not null);

        fluentAssertionVersions
            .Should()
            .NotContain(v => v.Major >= FluentAssertionsCommercialVersion,
                $"because we should not use commercial version of FluentAssertions in tests. Found versions: {string.Join(", ", fluentAssertionVersions)}");
    }

    private static Version? ExtractVersionFromAssemblyName(string assemblyName)
    {
        var match = AssemblyVersionRegex()
            .Match(assemblyName);

        return match.Success
            && Version.TryParse(match.Groups[1].Value, out var version)
            ? version
            : default;
    }
}
