// EndToEndTesting.cs
namespace SampleApp
{
    using Microsoft.AspNetCore.Mvc.Testing;
    using Xunit;
    using System.Net;
    using System.Threading.Tasks;

    public class EndToEndTesting
    {
        public static async Task E2ETesting()
        {
            // <E2ETesting>
            var application = new WebApplicationFactory<Program>();
            var client = application.CreateClient();

            var response = await client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // </E2ETesting>
        }
    }

    public class Program // Placeholder for WebApplicationFactory
    {
        public static void Main(string[] args) { }
    }
}