using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Features.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
    {
        private readonly UserDbContext _context;
        private readonly IJwtTokenGenerator _tokenGenerator;

        public LoginCommandHandler(UserDbContext context, IJwtTokenGenerator tokenGenerator)
        {
            _context = context;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Find user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

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
