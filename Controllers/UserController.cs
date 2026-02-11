using Microsoft.AspNetCore.Mvc;
using PostHubAPI.Dtos.User;
using PostHubAPI.Services.Interfaces;

namespace PostHubAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost("Register")]
    public IActionResult Register([FromBody] RegisterUserDto dto)
    {
        var token = userService.Register(dto);
        return Ok(token);
    }

    [HttpPost("Login")]
    public IActionResult Login([FromBody] LoginUserDto dto)
    {
        var token = userService.Login(dto);
        return Ok(token);
    }
}
