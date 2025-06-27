// Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Operations.ServiceDefaults.Api;

var builder = WebApplication.CreateBuilder(args);

// <snippet_AddApiServiceDefaults>
builder.AddApiServiceDefaults();
// </snippet_AddApiServiceDefaults>

var app = builder.Build();

// <snippet_ConfigureApiUsingDefaults>
app.ConfigureApiUsingDefaults();
// </snippet_ConfigureApiUsingDefaults>

app.Run();