# Housekeeping Service

## Overview

The Housekeeping service manages room status and housekeeping operations in a hotel management system. It provides functionality to track room cleanliness status, cleaning activities, maintenance requests, and mini fridge usage without creating or managing room entities themselves.

## Key Features

- **Room Status Management**: Track and update room cleanliness status (Clean, Dirty, Cleaning, Inspected, Maintenance, OutOfService)
- **Cleaning Operations**: Record cleaning activities and track completion times
- **Maintenance Requests**: Create and manage maintenance requests with priority levels
- **Mini Fridge Management**: Track mini fridge items and usage for billing purposes
- **Event-Driven Architecture**: Publishes integration events for other services to consume

## Quick Start

### Using .NET Aspire (Recommended)
```bash
cd Housekeeping/src/Housekeeping.AppHost
dotnet run
```

This automatically:
- Sets up PostgreSQL databases with Liquibase migrations
- Starts all services (API, BackOffice, Orleans)
- Configures service discovery and dependencies
- Provides observability dashboard

Access Points:
- **Aspire Dashboard**: http://localhost:18140
- **Housekeeping API**: http://localhost:8141/scalar
- **Orleans Dashboard**: http://localhost:8144/dashboard

### Using Docker Compose
```bash
# Backend services only
docker compose -f Housekeeping/compose.yml --profile api up -d

# All services
docker compose -f Housekeeping/compose.yml --profile api --profile backoffice --profile aspire up -d
```

## Architecture Overview

### Backend Services
- **Housekeeping.Api** - REST + gRPC API (ports 8141/8142)
  - Room status management endpoints
  - Cleaning operation endpoints
  - Maintenance request endpoints
  - Mini fridge management endpoints
- **Housekeeping.BackOffice** - Event-driven background processing (port 8143)
  - Wolverine-based message handling
  - Integration event processing
- **Housekeeping.BackOffice.Orleans** - Stateful room processing (port 8144)
  - Orleans actors with clustering
  - Persistent state management
- **Housekeeping** (Core) - Domain logic with DDD patterns
  - Commands, queries, entities
  - FluentValidation
  - Source-generated DB commands

## API Endpoints

### Room Status Management

**Get Room Status**
- `GET /rooms/{id}/status` - Get specific room status

**List Room Statuses**
- `GET /rooms/status?status={status}&floor={floor}&assignedCleanerId={cleanerId}` - List rooms with filtering

**Update Room Status**
- `PUT /rooms/{id}/status` - Update room status
  ```json
  {
    "status": "Clean",
    "notes": "Room inspected and ready",
    "updatedBy": "cleaner-guid"
  }
  ```

### Cleaning Operations

**Record Cleaning**
- `POST /rooms/{id}/cleaning` - Start or complete cleaning
  ```json
  {
    "cleanerId": "cleaner-guid",
    "isComplete": true,
    "notes": "Deep cleaned, restocked amenities"
  }
  ```

### Maintenance Requests

**Request Maintenance**
- `POST /rooms/{id}/maintenance` - Create maintenance request
  ```json
  {
    "issueType": "Plumbing",
    "description": "Leaky faucet in bathroom",
    "priority": "Medium",
    "reportedBy": "staff-guid"
  }
  ```

### Mini Fridge Management

**Update Mini Fridge**
- `PUT /rooms/{id}/mini-fridge` - Update mini fridge items
  ```json
  {
    "items": {
      "soda": 2,
      "water": 4,
      "snacks": 1
    },
    "updatedBy": "guest-guid"
  }
  ```

## Room Status Workflow

```
Dirty → Cleaning → Clean → Inspected
  ↓        ↓         ↓        ↓
OutOfService ← Maintenance ←──┘
```

- **Dirty**: Room needs cleaning after checkout
- **Cleaning**: Currently being cleaned
- **Clean**: Cleaning completed
- **Inspected**: Quality control passed
- **Maintenance**: Under repair/maintenance
- **OutOfService**: Temporarily unavailable

## Database

### Structure
- **housekeeping** database - Main application data
  - `rooms_status` - Room status information
  - `cleaning_history` - Cleaning activity records
  - `maintenance_requests` - Maintenance request tracking
- **service_bus** database - Wolverine messaging
- Managed by Liquibase migrations

### Connection String
```
Host=localhost;Port=54322;Database=housekeeping;Username=postgres;Password=password@
```

## Integration Events

### Published Events
- `RoomStatusChanged` - When room status changes
- `CleaningCompleted` - When cleaning is finished
- `MaintenanceRequested` - New maintenance request
- `MiniFridgeUpdated` - Mini fridge usage changed

### Event Examples
```csharp
// Room status changed
public record RoomStatusChanged(
    Guid RoomId,
    string RoomNumber,
    RoomStatus PreviousStatus,
    RoomStatus NewStatus,
    Guid? ChangedBy,
    DateTimeOffset ChangedDateUtc);

// Cleaning completed
public record CleaningCompleted(
    Guid RoomId,
    string RoomNumber,
    Guid CleanerId,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    TimeSpan Duration);
```

## Testing

### Backend Tests
```bash
# All tests
dotnet test

# Specific project
dotnet test Housekeeping/test/Housekeeping.Tests

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

Test Categories:
- **Unit** - Isolated component tests with mocks
- **Integration** - Real PostgreSQL via Testcontainers
- **Architecture** - NetArchTest rule enforcement

## Development Notes

- **Package Management**: Centralized via Directory.Packages.props
- **Code Quality**: SonarAnalyzer enabled, warnings as info
- **Environment**: .NET 9 with nullable reference types and implicit usings
- **Source Generation**: Custom generators reduce boilerplate for DB commands
- **Port Configuration**:
  - Database: 54322 (PostgreSQL)
  - API: 8141 (HTTP), 8142 (gRPC)
  - BackOffice: 8143
  - Orleans: 8144 (includes dashboard)
  - Aspire: 18140 (dashboard), 18892 (telemetry)

## Troubleshooting

### Common Issues

1. **Database connection failed**
   - Ensure PostgreSQL is running on port 54322
   - Check credentials: postgres/password@

2. **Port conflicts**
   - Check if ports 8141-8144, 18140, 18892, 54322 are available
   - Use `netstat -tulpn | grep <port>` to verify

3. **Test failures**
   - Docker must be running for Testcontainers
   - Run database migrations first

### Room Status Not Updating
- Check if room exists in `rooms_status` table
- Verify the room is not in a locked status (e.g., OutOfService)
- Check Orleans grain state for the room

### Integration Events Not Publishing
- Verify Wolverine message bus configuration
- Check service-bus database connectivity
- Review BackOffice service logs
