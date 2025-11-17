using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web.Http;
using TodoListMVC.App_Start;
using TodoListMVC.DTOs;
using TodoListMVC.Models;
using TodoListMVC.Repositories;

namespace TodoListMVC.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuthController(
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userRepository = _unitOfWork.Users; // Lấy repo từ UoW để dùng chung connection/transaction
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _userRepository.GetByEmail(model.Email);
            if (user == null)
                return Unauthorized();

            // TODO: so sánh mật khẩu (hash + salt). Ví dụ đơn giản:
            if (!VerifyPassword(model.Password, user.PasswordHash))
                return Unauthorized();

            var token = CreateJwtToken(user);

            return Ok(new
            {
                id = user.Id,
                email = user.Email,

                access_token = token,
                token_type = "bearer",
                expires_in = (int)JwtConfig.TokenLifetime.TotalSeconds
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check email existed
            var existing = _userRepository.GetByEmail(model.Email);
            if (existing != null)
            {
                return Content(System.Net.HttpStatusCode.Conflict, new { message = "Email đã tồn tại" });
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var user = new UserModel
            {
                Email = model.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            // Create user
            var created = _userRepository.Create(user);
            _unitOfWork.SaveChanges();

            // Return JWT immediately or201 with basic info
            var token = CreateJwtToken(created);
            return Content(System.Net.HttpStatusCode.Created, new
            {
                id = created.Id,
                email = created.Email,
                access_token = token,
                token_type = "bearer",
                expires_in = (int)JwtConfig.TokenLifetime.TotalSeconds
            });
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            // Tối thiểu là so sánh hash. Tùy cách bạn lưu và hash mật khẩu.
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        private string CreateJwtToken(UserModel user)
        {
            var key = Encoding.UTF8.GetBytes(JwtConfig.Secret);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: JwtConfig.Issuer,
                audience: JwtConfig.Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(JwtConfig.TokenLifetime),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}