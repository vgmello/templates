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

#### Update Cashier
```http
PUT /Cashiers/{id}
```

**Parameters:**
- `id` (UUID): The unique identifier of the cashier

**Request Body:**
```json
{
  "name": "John Smith",
  "email": "john.smith@example.com",
  "version": 1
}
```

**Response:**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "John Smith",
  "email": "john.smith@example.com",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T11:00:00Z",
  "version": 2
}
```

#### Delete Cashier
```http
DELETE /Cashiers/{id}
```

**Parameters:**
- `id` (UUID): The unique identifier of the cashier

**Response:**
- `204 No Content`: Cashier successfully deleted
- `404 Not Found`: Cashier not found

### Invoices Controller

Base URL: `/Invoices`

#### Get Invoice
```http
GET /Invoices/{id}
```

**Parameters:**
- `id` (UUID): The unique identifier of the invoice

**Response:**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "cashierId": "456e7890-e89b-12d3-a456-426614174000",
  "status": "Pending",
  "amount": 100.00,
  "currency": "USD",
  "description": "Invoice for services",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T10:00:00Z",
  "version": 1
}
```

#### Get Invoices (Filtered)
```http
GET /Invoices?status=Pending&cashierId=456e7890-e89b-12d3-a456-426614174000&pageNumber=1&pageSize=20
```

**Query Parameters:**
- `status` (string, optional): Filter by status (Pending, Paid, Cancelled)
- `cashierId` (UUID, optional): Filter by cashier
- `fromDate` (DateTime, optional): Filter invoices created after this date
- `toDate` (DateTime, optional): Filter invoices created before this date
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 20)

**Response:**
```json
{
  "items": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "cashierId": "456e7890-e89b-12d3-a456-426614174000",
      "status": "Pending",
      "amount": 100.00,
      "currency": "USD",
      "createdAt": "2024-01-01T10:00:00Z"
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 20
}
```

#### Create Invoice
```http
POST /Invoices
```

**Request Body:**
```json
{
  "cashierId": "456e7890-e89b-12d3-a456-426614174000",
  "amount": 100.00,
  "currency": "USD",
  "description": "Invoice for services"
}
```

**Response:**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "cashierId": "456e7890-e89b-12d3-a456-426614174000",
  "status": "Pending",
  "amount": 100.00,
  "currency": "USD",
  "description": "Invoice for services",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T10:00:00Z",
  "version": 1
}
```

#### Cancel Invoice
```http
PUT /Invoices/{id}/cancel
```

**Parameters:**
- `id` (UUID): The unique identifier of the invoice

**Response:**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "status": "Cancelled",
  "updatedAt": "2024-01-01T11:00:00Z",
  "version": 2
}
```

#### Mark Invoice as Paid
```http
PUT /Invoices/{id}/mark-paid
```

**Parameters:**
- `id` (UUID): The unique identifier of the invoice

**Request Body:**
```json
{
  "paymentReference": "PAY-123456",
  "paymentDate": "2024-01-01T11:00:00Z",
  "paymentMethod": "CreditCard"
}
```

**Response:**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "status": "Paid",
  "paymentReference": "PAY-123456",
  "paymentDate": "2024-01-01T11:00:00Z",
  "updatedAt": "2024-01-01T11:00:00Z",
  "version": 2
}
```

#### Simulate Payment (Testing Only)
```http
POST /Invoices/{id}/simulate-payment
```

**Parameters:**
- `id` (UUID): The unique identifier of the invoice

**Request Body:**
```json
{
  "paymentAmount": 100.00,
  "paymentMethod": "CreditCard"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Payment simulation completed",
  "invoiceId": "123e4567-e89b-12d3-a456-426614174000",
  "newStatus": "Paid"
}
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

#### UpdateCashier
```protobuf
rpc UpdateCashier (UpdateCashierRequest) returns (Cashier);
```

**Request:**
```protobuf
message UpdateCashierRequest {
  string id = 1;
  string name = 2;
  string email = 3;
  int32 version = 4;
}
```

#### DeleteCashier
```protobuf
rpc DeleteCashier (DeleteCashierRequest) returns (google.protobuf.Empty);
```

**Request:**
```protobuf
message DeleteCashierRequest {
  string id = 1;
}
```

### Invoices Service

Service definition: `InvoicesService`

#### GetInvoice
```protobuf
rpc GetInvoice (GetInvoiceRequest) returns (Invoice);
```

**Request:**
```protobuf
message GetInvoiceRequest {
  string id = 1;
}
```

**Response:**
```protobuf
message Invoice {
  string id = 1;
  string cashier_id = 2;
  string status = 3;
  double amount = 4;
  string currency = 5;
  string description = 6;
  google.protobuf.Timestamp created_at = 7;
  google.protobuf.Timestamp updated_at = 8;
  int32 version = 9;
}
```

#### GetInvoices
```protobuf
rpc GetInvoices (GetInvoicesRequest) returns (GetInvoicesResponse);
```

**Request:**
```protobuf
message GetInvoicesRequest {
  string status = 1;
  string cashier_id = 2;
  google.protobuf.Timestamp from_date = 3;
  google.protobuf.Timestamp to_date = 4;
  int32 page_number = 5;
  int32 page_size = 6;
}
```

**Response:**
```protobuf
message GetInvoicesResponse {
  repeated Invoice invoices = 1;
  int32 total_count = 2;
  int32 page_number = 3;
  int32 page_size = 4;
}
```

#### CreateInvoice
```protobuf
rpc CreateInvoice (CreateInvoiceRequest) returns (Invoice);
```

**Request:**
```protobuf
message CreateInvoiceRequest {
  string cashier_id = 1;
  double amount = 2;
  string currency = 3;
  string description = 4;
}
```

#### CancelInvoice
```protobuf
rpc CancelInvoice (CancelInvoiceRequest) returns (Invoice);
```

**Request:**
```protobuf
message CancelInvoiceRequest {
  string id = 1;
  string reason = 2;
}
```

#### MarkInvoiceAsPaid
```protobuf
rpc MarkInvoiceAsPaid (MarkInvoiceAsPaidRequest) returns (Invoice);
```

**Request:**
```protobuf
message MarkInvoiceAsPaidRequest {
  string id = 1;
  string payment_reference = 2;
  google.protobuf.Timestamp payment_date = 3;
  string payment_method = 4;
}

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