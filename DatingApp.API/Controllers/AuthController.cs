using System;
using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController:ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            userDto.UserName = userDto.UserName.ToLower();

            if (await _repo.UserExists(userDto.UserName) )
            return BadRequest();

            var userToCreate = new User
            {
                UserName= userDto.UserName
            };

            var createUser = await _repo.Register(userToCreate, userDto.Password);
            return  StatusCode(201);

        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
		// var keyRsa = new RsaSecurityKey(System.Security.Cryptography.RSA.Create(2048));
		
		//  var publicKey = Convert.ToBase64String(keyRsa.Rsa.ExportRSAPublicKey());
        //  var privateKey = Convert.ToBase64String(keyRsa.Rsa.ExportRSAPrivateKey());

               //throw new Exception("Error server");

                var  userFromRepo = await _repo.Login(userLoginDto.UserName.ToLower(), userLoginDto.Password);

                if(userFromRepo == null)
                return new UnauthorizedResult();

                var claims = new []
                {
                    new Claim(ClaimTypes.NameIdentifier,userFromRepo.Id.ToString()),
                    new Claim(ClaimTypes.Name, userFromRepo.UserName)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

                var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);

                var tokemDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.Now.AddDays(1),
                    SigningCredentials = creds
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokemDescriptor);

                return Ok(new {
                    token = tokenHandler.WriteToken(token)
                });
        }
    }
}