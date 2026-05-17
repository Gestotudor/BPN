# MoneyBee

MoneyBee is a .NET 8 microservices sample for a branch-based money transfer system. The solution splits the original monolith into three services:

- `AuthService`: API key management, authentication, authorization, and rate limiting
- `CustomerService`: customer registration, validation, KYC integration, and status management
- `TransferService`: transfer creation, fraud checks, exchange-rate support, idempotency, receiving, and cancellation

The implementation follows a Clean Architecture style with separate `API`, `Application`, `Domain`, and `Infrastructure` projects for each service.

## Tech Stack

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- MediatR
- FluentValidation
- Serilog
- Swagger / OpenAPI
- Docker and Docker Compose

## Solution Structure

```text
AuthService.API / Application / Domain / Infrastructure
CustomerService.API / Application / Domain / Infrastructure
TransferService.API / Application / Domain / Infrastructure
BuildingBlocks
docker-compose.yml
MoneyBee.slnx
```

## Implemented Capabilities

### AuthService

- Creates and revokes API keys
- Stores API keys hashed in PostgreSQL
- Validates API keys for other services through `POST /api/auth/validate`
- Enforces a memory-based rate limit of `100 requests/minute` per API key
- Supports scope-based authorization

Default scopes:

- `customer.read`
- `customer.write`
- `transfer.read`
- `transfer.write`

### CustomerService

- Creates customers after KYC verification
- Validates Turkish national ID numbers
- Rejects customers younger than 18
- Requires tax number for corporate customers
- Supports lookup by customer ID or national ID
- Changes customer status and notifies TransferService when status changes
- Exposes a validation endpoint used by TransferService

### TransferService

- Creates transfers between verified sender and receiver customers
- Requires fraud checks before each transfer
- Uses exchange rates for non-TRY transfers
- Applies a daily sender limit of `10,000 TRY`
- Adds a `5 minute` waiting period for transfers above `1,000 TRY`
- Supports idempotent transfer creation with the `Idempotency-Key` header
- Supports transfer receiving by transaction code
- Cancels pending transfers and records fee refunds
- Automatically cancels pending transfers when a customer becomes blocked

## Architecture Notes

- Each service has its own database context and migration set.
- Cross-service authentication is done with the `X-API-Key` header.
- API endpoints declare required scopes through attributes, and middleware validates them against `AuthService`.
- Health checks are exposed at `/health`.
- Swagger UI is enabled in development.

## External Dependencies

The project uses the following mandatory external services:

- Fraud service: `bpnpay/fraud-service:latest`
- KYC service: `bpnpay/kyc-service:latest`
- Exchange rate service: `bpnpay/exchange-rate-service:latest`

## Running the Project

### Option 1: Docker Compose

```bash
docker compose up --build
```

Current `docker-compose.yml` includes:

- `auth-db`
- `auth-api`
- `customer-db`
- `customer-api`
- `transfer-db`
- `transfer-api`
- `fraud-service`
- `kyc-service`
- `exchange-rate-service`

### Option 2: Run Services Locally

1. Start PostgreSQL and create the required databases.
2. Update connection strings in the service `appsettings.json` files if needed.
3. Start external services with Docker Compose.
4. Run the APIs individually:

```bash
dotnet run --project AuthService.API
dotnet run --project CustomerService.API
dotnet run --project TransferService.API
```

Default local URLs:

- AuthService: `https://localhost:5001` or `http://localhost:5101`
- CustomerService: `https://localhost:5002` or `http://localhost:5102`
- TransferService: `https://localhost:5003` or `http://localhost:5103`

Swagger endpoints:

- `https://localhost:5001/swagger`
- `https://localhost:5002/swagger`
- `https://localhost:5003/swagger`

## Configuration

### AuthService

- `ConnectionStrings:AuthDb`
- `Seed:AdminClientName`
- `Seed:AdminApiKey`

`AuthService` seeds a default admin client and admin API key from configuration.

### CustomerService

- `ConnectionStrings:CustomerDb`
- `ExternalServices:AuthService:BaseUrl`
- `ExternalServices:KycService:BaseUrl`
- `ExternalServices:TransferService:BaseUrl`

### TransferService

- `ConnectionStrings:TransferDb`
- `ExternalServices:FraudService:BaseUrl`
- `ExternalServices:CustomerService:BaseUrl`
- `ExternalServices:ExchangeService:BaseUrl`
- `ExternalServices:AuthService:BaseUrl`

## API Overview

### AuthService

- `POST /api/auth/validate`
- `POST /api/api-keys`
- `GET /api/api-keys`
- `DELETE /api/api-keys/{id}`

Example request to create an API key:

```json
{
  "clientName": "Branch App",
  "scopes": ["customer.read", "customer.write", "transfer.read", "transfer.write"],
  "expiresAt": null
}
```

### CustomerService

- `POST /api/customers`
- `GET /api/customers/{id}`
- `GET /api/customers/by-national-id/{nationalIdNumber}`
- `PUT /api/customers/{id}`
- `PATCH /api/customers/{id}/status`
- `POST /api/customers/validate`

Example request to create a customer:

```json
{
  "name": "Ayse",
  "surname": "Yilmaz",
  "nationalIdNumber": "10000000146",
  "taxNumber": null,
  "phoneNumber": "+905551112233",
  "dateOfBirth": "1995-01-15T00:00:00Z",
  "type": 0
}
```

### TransferService

- `POST /api/transfers`
- `GET /api/transfers/{id}`
- `GET /api/transfers/code/{transactionCode}`
- `POST /api/transfers/receive`
- `POST /api/transfers/{id}/cancel`
- `POST /api/internal/customer-status-changed`

Example request to create a transfer:

Headers:

- `X-API-Key: <api-key>`
- `Idempotency-Key: <unique-value>`

Body:

  ```json
  {
    "senderCustomer": {
      "name": "Ali",
      "surname": "Yilmaz",
      "nationalIdNumber": "12345678901",
      "taxNumber": null,
      "phoneNumber": "5551234567",
      "dateOfBirth": "1990-01-01T00:00:00Z",
      "type": 1
    },
    "receiverCustomer": {
      "name": "Ayse",
      "surname": "Demir",
      "nationalIdNumber": "10987654321",
      "taxNumber": null,
      "phoneNumber": "5559876543",
      "dateOfBirth": "1992-05-10T00:00:00Z",
      "type": 1
    },
    "amount": 1500,
    "currency": "TRY"
  }
  ```

## Development Notes

- `AuthService` seeds data on startup.
- `CustomerService` and `TransferService` apply EF Core migrations on startup.
- The repository currently contains ongoing local changes in `TransferService`; document and test behavior accordingly before production use.

## Build

```bash
dotnet build MoneyBee.slnx
```
