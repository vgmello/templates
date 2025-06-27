using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseKestrel(options =>
        {
            options.AddServerHeader = false;
        });
        webBuilder.UseStartup<Startup>();
    });
