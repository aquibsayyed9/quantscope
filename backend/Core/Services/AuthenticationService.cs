using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FixMessageAnalyzer.Data;
using FixMessageAnalyzer.Data.DTOs;
using FixMessageAnalyzer.Data.Entities;
using BC = BCrypt.Net.BCrypt;

namespace FixMessageAnalyzer.Core.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto model);
        Task<AuthResponseDto> LoginAsync(LoginDto model);
        Task<UserDto> GetUserByIdAsync(int userId);
    }

    public class AuthService : IAuthService
    {
        private readonly FixDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthService(FixDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
        {
            // Check if user exists
            if (await _dbContext.Users.AnyAsync(u => u.Email == model.Email))
                throw new Exception("User with this email already exists");

            // Hash password
            var passwordHash = BC.HashPassword(model.Password);

            // Create user
            var user = new User
            {
                Email = model.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Generate token
            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = new UserDto { Id = user.Id, Email = user.Email }
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto model)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
                throw new Exception("Invalid email or password");

            var isValid = BC.Verify(model.Password, user.PasswordHash);
            if (!isValid)
                throw new Exception("Invalid email or password");

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // Generate token
            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = new UserDto { Id = user.Id, Email = user.Email }
            };
        }

        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return null;

            return new UserDto { Id = user.Id, Email = user.Email };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"] ?? "your_default_secret_key_at_least_16_chars_long");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}