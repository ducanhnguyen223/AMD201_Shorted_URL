using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.DTOs;
using UserService.Entities;
using UserService.Services;

namespace UserService.Features.Commands
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
    {
        private readonly UserDbContext _context;
        private readonly IJwtTokenGenerator _tokenGenerator;

        public RegisterCommandHandler(UserDbContext context, IJwtTokenGenerator tokenGenerator)
        {
            _context = context;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create user
            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // Generate JWT token
            var token = _tokenGenerator.GenerateToken(user);

            return new AuthResponse
            {
                Token = token,
                Email = user.Email,
                FullName = user.FullName
            };
        }
    }
}
