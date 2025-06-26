// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Operations.ServiceDefaults.Api.OpenApi;

public class AutoProducesResponseTypeConvention : IActionModelConvention
{
    /// <summary>
    ///     This is a special status code that is used to indicate that the operation has an auto-generated response type.
    ///     Which should be overridden by the OpenAPI XML operation transformer.
    /// </summary>
    internal static readonly int StatusCode = -299;

    public void Apply(ActionModel action)
    {
        var hasSuccessResponseCode = action.Filters
            .OfType<ProducesResponseTypeAttribute>()
            .Any(attr => attr.StatusCode is >= 200 and < 300);

        if (hasSuccessResponseCode)
        {
            return;
        }

        var returnType = action.ActionMethod.ReturnType;

        if (returnType.IsGenericType)
        {
            var genericType = returnType.GetGenericTypeDefinition();

            if (genericType == typeof(Task<>) || genericType == typeof(ValueTask<>))
            {
                returnType = returnType.GetGenericArguments()[0];
            }
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ActionResult<>))
        {
            var responseType = returnType.GetGenericArguments()[0];

            var producesAttr = new ProducesResponseTypeAttribute(responseType, StatusCode);
            action.Filters.Add(producesAttr);
        }
    }
}
