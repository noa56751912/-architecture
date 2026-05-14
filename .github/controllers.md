# Controllers — Patterns & Reference

All controllers live in `WebApiShop/Controllers/` and follow the same structure.

## Existing Controllers
| File | Route | Purpose |
|---|---|---|
| `UsersController.cs` | `api/Users` | Register, login, get, update users |
| `ProductsController .cs` | `api/Products` | List all products; filtered/paginated list |
| `CategoriesController.cs` | `api/Categories` | CRUD for categories |
| `OrdersController.cs` | `api/Orders` | Place and retrieve orders |
| `PasswordController.cs` | `api/Password` | Password utilities |

> **Note:** Some filenames begin with an invisible RTL Unicode character. When creating files in this folder, verify with `Get-ChildItem` in PowerShell before opening.

---

## Controller Template

```csharp
using Microsoft.AspNetCore.Mvc;
using Services;
using DTOs;

namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class XxxController : ControllerBase
    {
        private readonly IXxxServices _xxxServices;
        // Add ILogger<XxxController> only if you need to log user actions

        public XxxController(IXxxServices xxxServices)
        {
            _xxxServices = xxxServices;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<XxxDTO>>> Get()
        {
            var items = await _xxxServices.GetAll();
            if (items != null && items.Any())
                return Ok(items);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<XxxDTO>> GetById(int id)
        {
            var item = await _xxxServices.GetById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<XxxDTO>> Create([FromBody] PostXxxDTO dto)
        {
            var created = await _xxxServices.Create(dto);
            if (created == null) return BadRequest();
            return CreatedAtAction(nameof(GetById), new { id = created.XxxId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PostXxxDTO dto)
        {
            await _xxxServices.Update(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _xxxServices.Delete(id);
            return NoContent();
        }
    }
}
```

## Rules
- Inject only the service interface, never the repository directly.
- Never catch exceptions — `ErrorHandlingMiddleware` handles them globally.
- Use `NoContent()` for empty lists and successful PUT/DELETE.
- Use `CreatedAtAction` for successful POST responses.
- Return DTOs, never Entities.
- `[FromBody]` for POST/PUT payloads; `[FromQuery]` for filter params.
