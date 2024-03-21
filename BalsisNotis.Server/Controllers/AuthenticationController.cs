using BalsisNotis.Server.Models;
using BalsisNotis.Server.Models.DtoModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNotis.Server.Controllers
{
    [ApiController]
    [Route("Api/[controller]/[action]")]
    public class AuthenticationController(AppDbContext context, SignInManager<IdentityUser> _signInManager) : AppControllerBase(context)
    {
        [HttpPost(Name = "Login")]
        [AllowAnonymous]
        public AppResponse<UserDto> Login(User attemptedUser)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(attemptedUser.Email, attemptedUser.Password, isPersistent: true, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return new AppResponse<UserDto>(new UserDto(result.), true);
                }

                return new AppResponse<UserDto>(null, false, "Login Failed: Your Email Address or Password is incorrect");
            }

            var User = _context.Users.FirstOrDefault(user => user.Email == attemptedUser.Email && user.PasswordHash == attemptedUser.Password);



            
        }

        [HttpPost(Name = "Register")]
        public bool Register(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();

            return true;
        }
    }
}
