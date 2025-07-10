---
# https://vitepress.dev/reference/default-theme-home-page
layout: home

hero:
  name: "Billing Service"
  text: "Comprehensive Documentation"
  tagline: "Domain-driven billing service with REST/gRPC APIs, Orleans actors, and modern frontend"
  actions:
    - theme: brand
      text: Quick Start
      link: /architecture#quick-start
    - theme: alt
      text: API Reference
      link: /api-reference

features:
  - title: 🏗️ Architecture
    details: Domain-driven design with CQRS, microservices, and event-driven processing using Orleans actors
    link: /architecture
  - title: 🚀 APIs
    details: REST and gRPC endpoints for cashier management, invoice processing, and payment handling
    link: /api-reference
  - title: 🗄️ Database
    details: PostgreSQL with Liquibase migrations, Entity Framework, and source-generated commands
    link: /database
  - title: 💻 Frontend
    details: SvelteKit application with TypeScript, Tailwind CSS, and shadcn-svelte components
    link: /frontend
---

## Overview

The Billing Service is a comprehensive solution built with modern technologies and best practices:

- **🔧 Backend**: .NET 9 with Orleans, Wolverine, and Entity Framework
- **🌐 Frontend**: SvelteKit with TypeScript and Tailwind CSS
- **🔍 Observability**: OpenTelemetry integration and health checks
- **🐳 Deployment**: Docker Compose and .NET Aspire support
- **🧪 Testing**: Unit, integration, and E2E tests with Testcontainers

## Quick Navigation

### 🏗️ [Architecture Overview](/architecture)
Learn about the system design, service structure, and technology stack.

### 🚀 [API Reference](/api-reference)
Comprehensive REST and gRPC API documentation with examples.

### 🗄️ [Database Schema](/database)
Database structure, migrations, and Entity Framework configuration.

### 💻 [Frontend Development](/frontend)
SvelteKit application structure, components, and development workflow.

## Getting Started

### Prerequisites
- .NET 9 SDK
- Docker & Docker Compose
- Node.js 18+ (for frontend)
- PostgreSQL (or use Docker)

### Quick Start with .NET Aspire
```bash
cd Billing/src/Billing.AppHost
dotnet run
```

### Quick Start with Docker
```bash
docker compose -f Billing/compose.yml --profile api up -d
```

### Access Points
- **Aspire Dashboard**: [http://localhost:18100](http://localhost:18100)
- **Billing Web UI**: [http://localhost:8105](http://localhost:8105)  
- **Billing API**: [http://localhost:8101/scalar](http://localhost:8101/scalar)
- **Orleans Dashboard**: [http://localhost:8104/dashboard](http://localhost:8104/dashboard)

## Key Features

### 🎯 Core Functionality
- **Cashier Management**: Create, update, and manage cashiers with multi-currency support
- **Invoice Processing**: Complete invoice lifecycle with payment simulation
- **Event-Driven Architecture**: Integration events for system coordination
- **Real-time Processing**: Orleans actors for stateful operations

### 🛠️ Development Features
- **API-First Design**: OpenAPI/Swagger documentation
- **Type Safety**: Full TypeScript support across frontend and backend
- **Modern UI**: Responsive design with accessibility support
- **Comprehensive Testing**: Unit, integration, and E2E test coverage

### 📊 Observability
- **Distributed Tracing**: OpenTelemetry integration
- **Health Checks**: Comprehensive health monitoring
- **Metrics**: Application and business metrics
- **Logging**: Structured logging with correlation IDs

## Technology Stack

### Backend
- **.NET 9**: Modern C# with nullable reference types
- **Orleans**: Virtual actors for stateful processing
- **Wolverine**: Messaging and CQRS implementation
- **Entity Framework**: Database access and migrations
- **FluentValidation**: Input validation
- **OpenAPI**: API documentation

### Frontend
- **SvelteKit**: Full-stack web framework
- **TypeScript**: Type-safe JavaScript
- **Tailwind CSS**: Utility-first CSS framework
- **shadcn-svelte**: UI component library
- **Vitest**: Unit testing framework
- **Playwright**: E2E testing framework

### Infrastructure
- **PostgreSQL**: Primary database
- **Liquibase**: Database migrations
- **Docker**: Containerization
- **OpenTelemetry**: Observability
- **.NET Aspire**: Orchestration and observability

## Support

For questions, issues, or contributions:
- Review the [Architecture Overview](/architecture) for system understanding
- Check the [API Reference](/api-reference) for endpoint documentation
- Consult the [Database Schema](/database) for data model details
- See the [Frontend Development](/frontend) guide for UI development