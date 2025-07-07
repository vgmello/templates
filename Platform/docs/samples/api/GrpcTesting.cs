using Grpc.Net.Client;
using System.Threading.Tasks;
using Xunit;

public class GrpcTests
{
    [Fact]
    public async Task SayHello_ReturnsAHelloReply()
    {
        // Arrange
        using var channel = GrpcChannel.ForAddress("http://localhost:5000");
        var client = new Greeter.GreeterClient(channel);

        // Act
        var reply = await client.SayHelloAsync(new HelloRequest { Name = "gRPC" });

        // Assert
        Assert.Equal("Hello gRPC", reply.Message);
    }
}
