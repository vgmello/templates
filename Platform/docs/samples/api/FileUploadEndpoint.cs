using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

public static class FileUploadEndpoint
{
    // <FileUploadEndpoint>
    public static async Task<Results<Ok, BadRequest>> UploadAsync(HttpRequest request)
    {
        if (request.Form.Files.Count == 0)
        {
            return TypedResults.BadRequest();
        }

        var file = request.Form.Files[0];
        // Process the uploaded file
        return TypedResults.Ok();
    }
    // </FileUploadEndpoint>
}
