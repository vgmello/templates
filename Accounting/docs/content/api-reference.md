# API Reference

The Billing service provides both REST and gRPC APIs for managing cashiers and invoices.

## REST API Endpoints

### Cashiers Controller

Base URL: `/Cashiers`

#### Get Cashier
```http
GET /Cashiers/{id}
```

**Parameters:**
- `id` (UUID): The unique identifier of the cashier

**Response:**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "John Doe",
  "email": "john.doe@example.com",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T10:00:00Z",
  "version": 1
}
```

#### Get Cashiers (Paginated)
```http
GET /Cashiers?pageNumber=1&pageSize=10
```

**Query Parameters:**
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10)

**Response:**
```json
{
  "items": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "name": "John Doe",
      "email": "john.doe@example.com",
      "createdAt": "2024-01-01T10:00:00Z",
      "updatedAt": "2024-01-01T10:00:00Z",
      "version": 1
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10
}
```

#### Create Cashier
```http
POST /Cashiers
```

**Request Body:**
```json
{
  "name": "John Doe",
  "email": "john.doe@example.com"
}
```

**Response:**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "John Doe",
  "email": "john.doe@example.com",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T10:00:00Z",
  "version": 1
}
```

### Invoices Controller

Base URL: `/Invoices`

#### Get Invoices
```http
GET /Invoices
```

**Response:**
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "status": "Pending",
    "amount": 100.00,
    "currency": "USD",
    "createdAt": "2024-01-01T10:00:00Z"
  }
]
```

## gRPC API

### Cashiers Service

Service definition: `CashiersService`

#### GetCashier
```protobuf
rpc GetCashier (GetCashierRequest) returns (Cashier);
```

**Request:**
```protobuf
message GetCashierRequest {
  string id = 1;
}
```

**Response:**
```protobuf
message Cashier {
  string id = 1;
  string name = 2;
  string email = 3;
  google.protobuf.Timestamp created_at = 4;
  google.protobuf.Timestamp updated_at = 5;
  int32 version = 6;
}
```

#### GetCashiers
```protobuf
rpc GetCashiers (GetCashiersRequest) returns (GetCashiersResponse);
```

**Request:**
```protobuf
message GetCashiersRequest {
  int32 page_number = 1;
  int32 page_size = 2;
}
```

**Response:**
```protobuf
message GetCashiersResponse {
  repeated Cashier cashiers = 1;
  int32 total_count = 2;
  int32 page_number = 3;
  int32 page_size = 4;
}
```

#### CreateCashier
```protobuf
rpc CreateCashier (CreateCashierRequest) returns (Cashier);
```

**Request:**
```protobuf
message CreateCashierRequest {
  string name = 1;
  string email = 2;
}
```

## Error Handling

### HTTP Status Codes

- `200 OK`: Request successful
- `201 Created`: Resource created successfully
- `400 Bad Request`: Invalid request data
- `404 Not Found`: Resource not found
- `409 Conflict`: Resource already exists
- `500 Internal Server Error`: Server error

### Error Response Format

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "The provided data is invalid",
    "details": [
      {
        "field": "email",
        "message": "Email is required"
      }
    ]
  }
}
```

### gRPC Status Codes

- `OK`: Request successful
- `INVALID_ARGUMENT`: Invalid request parameters
- `NOT_FOUND`: Resource not found
- `ALREADY_EXISTS`: Resource already exists
- `INTERNAL`: Internal server error

## Authentication & Authorization

The Billing service implements standard authentication patterns:

- **API Keys**: For service-to-service communication
- **JWT Tokens**: For user authentication
- **Role-Based Access**: Different permissions for different operations

## Rate Limiting

API endpoints are rate-limited to prevent abuse:

- **Global Limit**: 1000 requests per minute per IP
- **Per-User Limit**: 100 requests per minute per authenticated user
- **Burst Limit**: Up to 50 requests in a 10-second window

## Webhooks

The service can send webhooks for important events:

### Cashier Events
- `cashier.created`: When a new cashier is created
- `cashier.updated`: When cashier information is modified

### Invoice Events
- `invoice.created`: When a new invoice is generated
- `invoice.paid`: When an invoice payment is processed
- `invoice.cancelled`: When an invoice is cancelled

**Webhook Payload Example:**
```json
{
  "event": "cashier.created",
  "timestamp": "2024-01-01T10:00:00Z",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "John Doe",
    "email": "john.doe@example.com"
  }
}
```

## Client SDKs

### .NET Client
```csharp
var client = new CashiersServiceClient(channel);
var cashier = await client.GetCashierAsync(new GetCashierRequest 
{ 
    Id = "123e4567-e89b-12d3-a456-426614174000" 
});
```

### HTTP Client Examples

#### cURL
```bash
# Get a cashier
curl -X GET "https://api.billing.local/Cashiers/123e4567-e89b-12d3-a456-426614174000" \
     -H "Authorization: Bearer YOUR_TOKEN"

# Create a cashier
curl -X POST "https://api.billing.local/Cashiers" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer YOUR_TOKEN" \
     -d '{"name":"John Doe","email":"john.doe@example.com"}'
```