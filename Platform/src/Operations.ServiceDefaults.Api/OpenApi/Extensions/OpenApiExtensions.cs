// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Operations.ServiceDefaults.Api.OpenApi.Transformers;

namespace Operations.ServiceDefaults.Api.OpenApi.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiWithXmlDocSupport(this IServiceCollection services, Action<OpenApiOptions>? config = null)
    {
        services.AddSingleton<IXmlDocumentationService, XmlDocumentationService>();

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<XmlDocumentationDocumentTransformer>();
            options.AddOperationTransformer<XmlDocumentationOperationTransformer>();
            options.AddSchemaTransformer<XmlDocumentationSchemaTransformer>();

            config?.Invoke(options);
        });

        services.Configure<MvcOptions>(opt => opt.Conventions.Add(new AutoProducesResponseTypeConvention()));

        return services;
    }
}
