# Prompt: Add a New Endpoint to WebApiShop

Use this prompt as a starting point when asking Copilot (or another AI assistant) to implement a complete new endpoint in this project. Fill in every `<placeholder>` before submitting.

---

## Request Template

```
Add a new endpoint to the WebApiShop project.

### What to build
- Resource name : <ResourceName>           (e.g. "Review")
- HTTP method   : <GET | POST | PUT | DELETE>
- Route         : api/<ResourceName>[/<sub-path>]
- Description   : <one-sentence description of what the endpoint does>

### Input  (skip if not applicable)
- DTO name      : <PostXxxDTO | XxxDTO>
- Fields        : <field name> : <type>, ...

### Output
- DTO name      : <XxxDTO>
- Fields        : <field name> : <type>, ...

### Business rules / special logic
- <list any filtering, validation, authorization, or caching requirements>
- Authorization required: <yes – role: Admin | User | none>
- Redis caching needed  : <yes | no>

### Files to create or modify
Follow the standard checklist below.
```

---

## Implementation Checklist

Work through every step in order. Do **not** skip steps.

### 1. Entity (if new table is needed)
- File: `Entities/<ResourceName>.cs`
- Use a `partial class` to avoid overwriting auto-generated code.
- Add the matching `DbSet<<ResourceName>>` in a separate `partial class ApiShopContext` file inside `Repositories/`.
- Run: `dotnet ef migrations add Add<ResourceName> --project Repositories`

### 2. DTO(s)
- File(s): `DTOs/<ResourceName>DTO.cs`, `DTOs/Post<ResourceName>DTO.cs`
- Use immutable `record` types.
- Example:
  ```csharp
  namespace DTOs
  {
      public record ResourceNameDTO(int ResourceNameId, string Name);
      public record PostResourceNameDTO(string Name);
  }
  ```

### 3. AutoMapper mapping
- File: `WebApiShop/AutoMapper.cs`
- Add: `CreateMap<ResourceName, ResourceNameDTO>().ReverseMap();`

### 4. Repository interface
- File: `Repositories/I<ResourceName>Repository.cs`
- All methods must be `async Task<T>`.
- Template:
  ```csharp
  namespace Repositories
  {
      public interface IResourceNameRepository
      {
          Task<IEnumerable<ResourceName>> GetAll();
          Task<ResourceName?> GetById(int id);
          Task<ResourceName> Create(ResourceName entity);
          Task Update(ResourceName entity);
          Task Delete(int id);
      }
  }
  ```

### 5. Repository implementation
- File: `Repositories/<ResourceName>Repository.cs`
- Inject `ApiShopContext` via constructor.
- Use EF Core async methods (`ToListAsync`, `FindAsync`, `SaveChangesAsync`).
- Never expose entities outside this layer.

### 6. Service interface
- File: `Services/I<ResourceName>Services.cs`
- Mirror the repository but accept/return DTOs, not entities.
- Template:
  ```csharp
  namespace Services
  {
      public interface IResourceNameServices
      {
          Task<IEnumerable<ResourceNameDTO>> GetAll();
          Task<ResourceNameDTO?> GetById(int id);
          Task<ResourceNameDTO> Create(PostResourceNameDTO dto);
          Task Update(int id, PostResourceNameDTO dto);
          Task Delete(int id);
      }
  }
  ```

### 7. Service implementation
- File: `Services/<ResourceName>Services.cs`
- Inject `I<ResourceName>Repository` and `IMapper`.
- If Redis caching is needed, also inject `IConnectionMultiplexer` and `IConfiguration` — follow the `ProductsServices` pattern.
- Call the private `ClearCache()` helper in every write method.
- Do **not** add try/catch — `ErrorHandlingMiddleware` handles all exceptions globally.

### 8. Controller
- File: `WebApiShop/Controllers/<ResourceName>Controller.cs`
- Inject only `I<ResourceName>Services` (never the repository directly).
- Use `[AuthorizeRole("Admin")]` attribute for write operations when authorization is required.
- Return codes:
  | Situation | Return |
  |---|---|
  | List empty | `NoContent()` |
  | Item not found | `NotFound()` |
  | Invalid input | `BadRequest()` |
  | Successful POST | `CreatedAtAction(nameof(GetById), new { id = ... }, dto)` |
  | Successful PUT/DELETE | `NoContent()` |
- Template:
  ```csharp
  using Microsoft.AspNetCore.Mvc;
  using Services;
  using DTOs;

  namespace WebApiShop.Controllers
  {
      [Route("api/[controller]")]
      [ApiController]
      public class ResourceNameController : ControllerBase
      {
          private readonly IResourceNameServices _resourceNameServices;

          public ResourceNameController(IResourceNameServices resourceNameServices)
          {
              _resourceNameServices = resourceNameServices;
          }

          [HttpGet]
          public async Task<ActionResult<IEnumerable<ResourceNameDTO>>> Get()
          {
              var items = await _resourceNameServices.GetAll();
              if (items != null && items.Any()) return Ok(items);
              return NoContent();
          }

          [HttpGet("{id}")]
          public async Task<ActionResult<ResourceNameDTO>> GetById(int id)
          {
              var item = await _resourceNameServices.GetById(id);
              if (item == null) return NotFound();
              return Ok(item);
          }

          [HttpPost]
          public async Task<ActionResult<ResourceNameDTO>> Post([FromBody] PostResourceNameDTO dto)
          {
              var created = await _resourceNameServices.Create(dto);
              return CreatedAtAction(nameof(GetById), new { id = created.ResourceNameId }, created);
          }

          [HttpPut("{id}")]
          public async Task<IActionResult> Put(int id, [FromBody] PostResourceNameDTO dto)
          {
              await _resourceNameServices.Update(id, dto);
              return NoContent();
          }

          [HttpDelete("{id}")]
          public async Task<IActionResult> Delete(int id)
          {
              await _resourceNameServices.Delete(id);
              return NoContent();
          }
      }
  }
  ```

### 9. Dependency injection
- File: `WebApiShop/Program.cs`
- Add **after** the existing `AddScoped` registrations:
  ```csharp
  builder.Services.AddScoped<IResourceNameRepository, ResourceNameRepository>();
  builder.Services.AddScoped<IResourceNameServices, ResourceNameServices>();
  ```
- Only needed when creating a **new** repository/service pair.

### 10. Unit tests
- File: `TestProject/<ResourceName>RepositoryUnitTesting.cs`
- Use `Moq` + `Moq.EntityFrameworkCore` — no real DB.
- Mock `ApiShopContext` and assert service/repository behaviour.

---

## Key Constraints (always enforce)

| Rule | Detail |
|---|---|
| No try/catch | `ErrorHandlingMiddleware` is the global handler |
| No entities in controllers | Always map to/from DTOs |
| Async everywhere | Every service/repository method is `async Task<T>` |
| Interface-only injection | Controllers → `IXxxServices`; Services → `IXxxRepository` |
| Immutable DTOs | Use C# `record` types |
| No hardcoded secrets | Use `appsettings.json` / environment variables |
| File names with RTL chars | Run `Get-ChildItem` in PowerShell before reading/editing files in `Repositories/` and `Controllers/` |

---

## Example — Filled-in Prompt

```
Add a new endpoint to the WebApiShop project.

Resource name : Review
HTTP method   : POST, GET
Route         : api/Reviews
Description   : Allow authenticated users to submit a 1–5 star review for a product,
                and retrieve all reviews for a given product.

Input (POST)  : PostReviewDTO — ProductId: int, Stars: int, Comment: string?
Output        : ReviewDTO — ReviewId: int, ProductId: int, Stars: int, Comment: string?, CreatedAt: DateTime

Business rules:
- Stars must be between 1 and 5 (validate in service layer).
- Authorization required: yes – role: User
- Redis caching needed: no

Files to create or modify: follow the standard checklist.
```
