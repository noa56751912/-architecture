using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using DTOs;

namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserServices _userServices;
        private readonly ILogger<UsersController> _logger;
        public UsersController(IUserServices userServices, ILogger<UsersController> logger)
        {
            _userServices = userServices;
            _logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> Get()
        {
            IEnumerable<UserDTO> users = await _userServices.GetUsers();
            if (users != null && users.Any())
                return Ok(users);
            return NoContent();
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(int id)
        {
            UserDTO? user = await _userServices.GetUserById(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
        private void AppendJwtCookie(string token)
        {
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<ActionResult<UserDTO>> Login([FromBody] ExistingUserDTO existingUser)
        {
            AuthResponseDTO? authResponse = await _userServices.Login(existingUser);
            if (authResponse == null)
                return Unauthorized("Invalid email or password");

            _logger.LogInformation($"login attempted id:{authResponse.User.UserId} email:{authResponse.User.Email} first name:{authResponse.User.FirstName} last name:{authResponse.User.LastName}");
            AppendJwtCookie(authResponse.Token);
            return Ok(authResponse.User);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UserDTO>> Register([FromBody] PostUserDTO newUser)
        {
            if (!await _userServices.UserWithSameEmail(newUser.Email))
                return BadRequest("The email already exists. Please try again.");
            if (!_userServices.IsPasswordStrong(newUser.Password))
                return BadRequest("The password is too weak. Please try again.");
            AuthResponseDTO? authResponse = await _userServices.Register(newUser);
            if (authResponse == null)
                return BadRequest();
            AppendJwtCookie(authResponse.Token);
            return CreatedAtAction(nameof(Get), new { id = authResponse.User.UserId }, authResponse.User);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PostUserDTO updateUser)
        {
            if (!_userServices.IsPasswordStrong(updateUser.Password))
                return BadRequest("The password is too weak. Please try again.");
            await _userServices.Update(id, updateUser);
            return NoContent();
        }
    }
}


