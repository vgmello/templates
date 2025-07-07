// ArchitectureTesting.cs
namespace SampleApp
{
    using Xunit;
    using NetArchTest.Rules;

    public class ArchitectureTesting
    {
        public static void RunArchitectureTests()
        {
            // <ArchitectureTesting>
            // Example: Ensure domain layer does not depend on infrastructure layer
            // var result = Types.InCurrentDomain()
            //     .That()
            //     .ResideInNamespace("SampleApp.Domain")
            //     .ShouldNot()
            //     .HaveDependencyOn("SampleApp.Infrastructure")
            //     .Get  Result();
            // Assert.True(result.IsSuccessful);
            // </ArchitectureTesting>
        }
    }
}