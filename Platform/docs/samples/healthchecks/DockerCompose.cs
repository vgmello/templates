// DockerCompose.cs
namespace SampleApp
{
    using Microsoft.Extensions.Hosting;

    public class DockerCompose
    {
        public static void DockerConfig()
        {
            var builder = Host.CreateApplicationBuilder();
            // No specific code changes for Docker Compose health checks in C#,
            // as they are typically configured in the docker-compose.yml file.
            // <DockerConfig>
            // Example docker-compose.yml snippet:
            /*
            services:
              myservice:
                image: myapp:latest
                healthcheck:
                  test: ["CMD", "curl", "-f", "http://localhost/healthz"]
                  interval: 30s
                  timeout: 10s
                  retries: 3
            */
            // </DockerConfig>
            var app = builder.Build();
            app.Run();
        }
    }
}