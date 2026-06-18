# WebApiShop - Microservices Distribution Plan

## Overview

The current monolith exposes four functional domains over a single SQL Server database and a shared EF Core context. This plan describes how to decompose it into independently deployable microservices, the communication strategy between them, and the migration path.

---

## Proposed Services

### 1. Catalog Service
**Owns:** `Products`, `Categories`
**Endpoints:**
- `GET /api/Products` - all products (Redis-cached)
- `GET /api/Products/{id}`
- `GET /api/Products/Filter` - pagination + filtering
- `GET /api/Categories`

**Data store:** Dedicated SQL Server DB (tables: `Products`, `Categories`)
**Cache:** Redis (`products:all`, TTL-driven)
**Notes:** Read-heavy; no write endpoints exist yet. Straightforward to extract - no runtime dependencies on other services.

---

### 2. Identity Service
**Owns:** `Users`, password scoring
**Endpoints:**
- `GET /api/Users`, `GET /api/Users/{id}`
- `POST /api/Users` (register)
- `POST /api/Users/Login`
- `PUT /api/Users/{id}`
- `POST /api/Password/PasswordScore`

**Data store:** Dedicated SQL Server DB (table: `Users`)
**Notes:** Fully self-contained. `PasswordServices` has zero external dependencies. Future work: issue JWT tokens here; all other services validate tokens.

---

### 3. Order Service
**Owns:** `Orders`, `OrderItems`
**Endpoints:**
- `GET /api/Orders/{id}`
- `POST /api/Orders`

**Data store:** Dedicated SQL Server DB (tables: `Orders`, `OrderItems`)
**Inter-service dependency:** Needs product prices to validate `OrderSum` - resolved by a synchronous HTTP call to **Catalog Service** (`GET /api/Products/{id}`) at order-creation time.
**Notes:** `UserDTO` currently embeds `ICollection<OrderDTO>`; after decomposition, the API Gateway or BFF layer assembles this by calling both Identity and Order services.

---

### 4. Audit Service (Observability)
**Owns:** `Ratings`
**Current role:** `RatingMiddleware` writes a row per request synchronously.
**Target role:** Publish a lightweight event (e.g., via RabbitMQ / Azure Service Bus) from each service's middleware; this service consumes and persists them asynchronously.
**Data store:** Dedicated SQL Server DB (table: `Ratings`) or a time-series store (e.g., InfluxDB).
**Notes:** Decoupling this from the request path removes latency and removes the shared-DB coupling.

---

## API Gateway
Add a single entry point (e.g., YARP, Ocelot, or Azure API Management) that:
- Routes `/api/Products*` and `/api/Categories*` to Catalog Service
- Routes `/api/Users*` and `/api/Password*` to Identity Service
- Routes `/api/Orders*` to Order Service
- Forwards JWT tokens for downstream auth (once Identity issues them)
- Can aggregate Identity + Order responses for the `UserDTO` with embedded orders

---

## Shared Concerns

| Concern | Approach |
|---|---|
| **Auth** | Identity Service issues JWT; all services validate with shared secret / JWKS endpoint |
| **Error handling** | Each service keeps its own `ErrorHandlingMiddleware` |
| **Logging** | Each service runs its own NLog; ship logs to a central sink (Seq, ELK, Application Insights) |
| **Caching** | Catalog keeps Redis; other services adopt Redis as needed per traffic patterns |
| **DTOs / contracts** | Replace shared `DTOs.csproj` with per-service contract packages or OpenAPI specs |
| **Entities** | Break `Entities.csproj` into per-service models; no cross-service entity references |
| **AutoMapper** | Each service owns its own `AutoMapper.cs` profile |

---

## Migration Path

1. **Phase 0 - Prepare monolith**
   - Add integration-test coverage for all endpoints (`DatabaseFixture` pattern already in place).
   - Wire up JWT authentication in the monolith first (so Identity token contract is defined).

2. **Phase 1 - Extract Catalog Service**
   - Lowest risk: no inter-service calls needed.
   - Create new ASP.NET Core project, move `Products`/`Categories` stack, point at new DB.
   - Proxy `/api/Products*` and `/api/Categories*` through gateway.

3. **Phase 2 - Extract Identity Service**
   - Move `Users`/`PasswordServices` stack to new project.
   - Implement JWT issuance on `Login`.
   - Update all remaining services to validate tokens.

4. **Phase 3 - Extract Order Service**
   - Replace `OrdersServices.ValidateOrderSum` direct-DB lookup with HTTP call to Catalog Service.
   - Decouple `UserDTO` embeds - gateway aggregates responses.

5. **Phase 4 - Decouple Audit Service**
   - Introduce message broker (RabbitMQ via docker-compose).
   - Replace synchronous `RatingMiddleware` DB writes with event publishing.
   - Deploy Audit Service as independent consumer.

---

## Docker Compose (Target)

```yaml
services:
  gateway:         # YARP / Ocelot
  catalog-api:     # Catalog Service
  identity-api:    # Identity Service
  order-api:       # Order Service
  audit-api:       # Audit Service
  redis:           # existing
  rabbitmq:        # new - for audit events
  sql-catalog:     # dedicated SQL Server instance (or schema)
  sql-identity:
  sql-orders:
  sql-audit:
```

---

## Key Decomposition Risks

| Risk | Mitigation |
|---|---|
| `OrderSum` validation crosses Order to Catalog | HTTP call to Catalog at order creation; use circuit breaker (Polly) |
| `UserDTO` embeds order history (cross-domain join) | Aggregate at gateway/BFF layer |
| Shared `ApiShopContext` | Migrate each domain tables to its own context before splitting repos |
| RTL Unicode prefixes in some filenames | Always verify filenames with `Get-ChildItem` before editing (per existing guidelines) |
| No authentication today | Implement JWT in monolith first (Phase 0) to avoid retrofitting auth across five services |
