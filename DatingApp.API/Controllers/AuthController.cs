using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.Application.IRepositories;
using DatingApp.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DatingApp.API.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IAuthRepository _authRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository authRepository,
            IMapper mapper,
            IConfiguration config)
        {
            _authRepository = authRepository;
            _mapper = mapper;
            _config = config;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if (await _authRepository.UserExists(userForRegisterDto.Username))
            {
                return BadRequest("Tài khoản đã tồn tại.");
            }

            var userToCreate = new User() { Username = userForRegisterDto.Username };
            var createdUser = await _authRepository.Register(userToCreate, userForRegisterDto.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userForRepository = await _authRepository.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userForRepository == null)
            {
                return Unauthorized();
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userForRepository.Id.ToString()),
                new Claim(ClaimTypes.Name, userForRepository.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var user = _mapper.Map<UserForListDto>(userForRepository);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                user
            });
        }
    }
}