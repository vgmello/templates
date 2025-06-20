---
_layout: landing
---

# Billing Service Documentation

Welcome to the Billing Service documentation. This service is part of a .NET 9 microservices system built using Domain-Driven Design principles.

## What is the Billing Service?

The Billing Service manages cashiers, invoices, and payment processing within the broader Operations platform. It provides both REST and gRPC APIs for managing billing operations and integrates with other services through event-driven messaging.

## Key Features

- **Cashier Management**: Create and manage cashiers with multi-currency support
- **Invoice Processing**: Handle invoice lifecycle with Orleans-based stateful processing
- **Payment Integration**: Process payments and emit integration events
- **Cross-Service Integration**: React to business events from other services like Accounting

# Getting Started

This guide will help you set up and run the Billing service locally.

## Prerequisites

- .NET 9 SDK
- PostgreSQL running on localhost:5432
- Docker (optional, for containerized deployment)

## Database Setup

The Billing service requires PostgreSQL with specific databases and schemas. Run these commands from the `Billing/infra/Billing.Database/` directory:

```bash
cd Billing/infra/Billing.Database/

# Step 1: Setup databases
liquibase update --defaults-file liquibase.setup.properties

# Step 2: Service bus schema  
liquibase update --defaults-file liquibase.servicebus.properties

# Step 3: Domain schema
liquibase update
```

## Running the Service

### Local Development with .NET Aspire

The recommended way to run the service locally is using the .NET Aspire AppHost:

```bash
# Run the entire Billing service stack
dotnet run --project Billing/src/Billing.AppHost
```

This will start:
- Billing.Api (REST/gRPC endpoints)
- Billing.BackOffice (background processing)
- Billing.BackOffice.Orleans (stateful invoice processing)
- All necessary dependencies

### Individual Services

You can also run individual services:

```bash
# API service
dotnet run --project Billing/src/Billing.Api

# Background service
dotnet run --project Billing/src/Billing.BackOffice

# Orleans service
dotnet run --project Billing/src/Billing.BackOffice.Orleans
```

### Docker Compose

For containerized deployment:

```bash
docker-compose up --build
```

## Verifying the Setup

1. **Check API Health**: Navigate to `https://localhost:7001/health` (or the configured port)
2. **Test gRPC**: Use a gRPC client to connect to the Cashiers service
3. **Database Verification**: Check that the `billing` and `service_bus` databases exist

## Next Steps

- Explore the [Architecture Overview](content/architecture.md) to understand the service design
- Review the [API Reference](content/api-reference.md) for available endpoints
- Check the [Database Schema](content/database.md) for data structure details