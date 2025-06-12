using Grpc.Core;
using Accounting.Api.Protos.ResourceManagement;
using Microsoft.Extensions.Logging; // Added for ILogger
using System.Threading.Tasks; // Added for Task

namespace Accounting.Api.ResourceManagement; // Updated namespace

public class ResourceManagerServiceImpl : ResourceManager.ResourceManagerBase
{
    private readonly ILogger<ResourceManagerServiceImpl> _logger;

    public ResourceManagerServiceImpl(ILogger<ResourceManagerServiceImpl> logger)
    {
        _logger = logger;
    }

    public override Task<GetResourceResponse> GetResource(GetResourceRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Getting resource {ResourceId}", request.ResourceId);

        // Dummy implementation
        return Task.FromResult(new GetResourceResponse
        {
            ResourceId = request.ResourceId,
            Name = "Sample Resource",
            Description = "This is a sample resource description from ResourceManagerService."
        });
    }
}
