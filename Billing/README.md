# Billing Service Documentation

This directory contains the documentation for the Billing Service, built with [DocFX](https://dotnet.github.io/docfx/) using the [Material theme](https://ovasquez.github.io/docfx-material/).

## Quick Start

### Using Docker
Run from the **Billing** folder (not the docs folder):

```bash
# Build the image (from Billing folder)
docker build -f docs/Dockerfile -t billing-docfx .

# Run the container
docker run -d -p 8850:8850 --name billing-docs billing-docfx
```

### Using Local DocFX
Run from the **Billing** folder:

```bash
# Install DocFX (if not already installed)
dotnet tool install -g docfx

# Serve documentation (from Billing folder)
docfx docs/docfx.json --serve -p 8850 -n "*"
```

### Using .NET Aspire
The documentation service is automatically included when running the Billing AppHost:

```bash
cd src/Billing.AppHost
dotnet run
```

The documentation will be available in the Aspire dashboard with a direct link.

## Accessing the Documentation

Once running, the documentation is available at:
- **Local**: http://localhost:8850

## Important Notes

**Working Directory**: All DocFX commands should be run from the **Billing** folder, not the `docs` folder. This allows DocFX to properly access the source code for API documentation generation.

**Correct**:
```bash
cd Billing/
docfx docs/docfx.json --serve -p 8850 -n "*"
```

**Incorrect**:
```bash
cd Billing/docs/
docfx docfx.json --serve -p 8850 -n "*"  # Won't find source code
```

## Development

### Prerequisites
- .NET 9 SDK
- Docker
- DocFX (installed automatically in Docker)

### Project Structure
```
docs/
├── content/              # Markdown documentation files
│   ├── introduction.md
│   ├── getting-started.md
│   ├── architecture.md
│   ├── api-reference.md
│   ├── database.md
│   └── toc.yml          # Table of contents
├── templates/            # DocFX themes
│   └── material/         # Material theme files
├── api/                 # Auto-generated API documentation
├── images/              # Documentation images
├── docfx.json          # DocFX configuration
├── index.md            # Documentation homepage
├── toc.yml             # Root table of contents
└── Dockerfile          # Container configuration
```

### Building Locally
If you have DocFX installed locally, run from the **Billing** folder:

```bash
# Install DocFX (if not already installed)
dotnet tool install -g docfx

# Build documentation
docfx docs/docfx.json

# Serve locally
docfx docs/docfx.json --serve -p 8850 -n "*"
```

### Updating Documentation
1. Edit markdown files in the `content/` directory
2. Update `toc.yml` files if adding new pages
3. Rebuild the Docker image to see changes
4. The API documentation is auto-generated from code comments

### Customizing the Theme
The Material theme is included in `templates/material/` and supports various customization options:
- **Logo**: Replace `images/logo.svg` with your logo
- **Favicon**: Replace `images/favicon.ico` with your favicon
- **Colors**: Modify theme colors in `docfx.json` globalMetadata
- **Git Repository**: Update the `_gitContribute` settings for "Edit this page" links
- **Theme Updates**: Replace files in `templates/material/` to update the theme

### Configuration
The DocFX configuration is in `docfx.json`:
- **Port**: 8850 (configurable)
- **Theme**: Material theme for modern UI
- **Search**: Enabled for all content
- **API Generation**: Automatic from source code
- **Git Integration**: Links to contribute and edit pages

## Deployment Options

### Development (Aspire)
- Automatically included in Billing.AppHost
- Integrated with service discovery
- Available in Aspire dashboard

### Standalone (Docker)
- Use `docker-compose.yml` for standalone deployment
- Includes health checks and restart policies
- Can be deployed behind a reverse proxy

### Production
- Build image in CI/CD pipeline
- Deploy to container orchestration platform
- Configure ingress/load balancer for external access

## Monitoring
- Logs available via Docker logs
- DocFX built-in server provides basic request logging

## Troubleshooting

### Container won't start
```bash
# Check container logs
docker logs billing-docs

# Check if port is in use
netstat -tulpn | grep 8850
```

### Documentation not updating
- Rebuild the Docker image after making changes
- Clear browser cache
- Check that markdown files are valid

### API documentation missing
- Ensure source code has XML documentation comments
- Check that project references are correct in `docfx.json`
- Verify build succeeds without errors

### Theme not loading
- Ensure `templates/material/` directory exists
- Check that `docfx.json` references the correct template path
- Verify theme files are not corrupted

## Contributing
1. Add new markdown files to `content/` directory
2. Update table of contents in `toc.yml`
3. Test changes locally
4. Update this README if adding new features