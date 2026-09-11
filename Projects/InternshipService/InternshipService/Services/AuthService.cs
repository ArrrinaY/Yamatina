using InternshipService.Auth;
using InternshipService.Auth.models;
using InternshipService.Models.DTO;
using InternshipService.Models.Entities;
using InternshipService.Repositories;
using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace InternshipService.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJWTHelper _jwtHelper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, IJWTHelper jwtHelper, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtHelper = jwtHelper;
        _logger = logger;
    }

    public async Task<LoginResponseModel> LoginAsync(LoginRequestModel loginRequest)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(loginRequest.Username);
            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent username: {username}", loginRequest.Username);
                throw new ValidationException("Incorrect username or password");
            }

            var hashedPassword = HashPassword(loginRequest.Password);
            if (user.Password != hashedPassword && user.Password != loginRequest.Password)
            {
                _logger.LogWarning("Login attempt with incorrect password for username: {username}", loginRequest.Username);
                throw new ValidationException("Incorrect username or password");
            }

            var role = user.Role?.Trim() ?? string.Empty;
            
            role = role switch
            {
                "Admin" or "admin" => Roles.Admin,
                "Company" or "company" => Roles.Company,
                "Candidate" or "candidate" => Roles.Candidate,
                _ => Roles.Candidate
            };
            
            var token = _jwtHelper.GenerateToken(user.Id, role);
            
            _logger.LogInformation("User with username {username} logged in successfully", loginRequest.Username);
            
            return new LoginResponseModel
            {
                Token = token,
                Role = role,
                UserId = user.Id
            };
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {username}", loginRequest.Username);
            throw new ValidationException("An error occurred during login. Please try again.");
        }
    }

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}

