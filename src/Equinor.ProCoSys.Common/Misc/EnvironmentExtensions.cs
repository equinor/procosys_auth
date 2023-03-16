using Microsoft.Extensions.Hosting;

namespace Equinor.ProCoSys.Common.Misc
{
    public static class EnvironmentExtensions
    {
        public static bool IsTest(this IHostEnvironment hostEnvironment)
            => hostEnvironment.EnvironmentName == TestEnvironmentName;
        public static string TestEnvironmentName => "Test";

        public static bool IsIntegrationTest(this IHostEnvironment hostEnvironment)
            => hostEnvironment.EnvironmentName == IntegrationTestEnvironmentName;
        public static string IntegrationTestEnvironmentName => "IntegrationTests";
    }
}
