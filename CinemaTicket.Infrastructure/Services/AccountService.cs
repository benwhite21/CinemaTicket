using CinemaTicket.Application.DTOs;
using CinemaTicket.Application.Interfaces;
using CinemaTicket.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CinemaTicket.Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return (false, "Mật khẩu xác nhận không khớp.");

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return (false, "Email này đã được sử dụng.");

            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            await _userManager.AddToRoleAsync(user, "Customer");
            return (true, "Đăng ký thành công!");
        }

        public async Task<(bool Success, string Message, UserDto? User)> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return (false, "Email hoặc mật khẩu không đúng.", null);

            if (!user.IsActive)
                return (false, "Tài khoản đã bị khóa.", null);

            var result = await _signInManager.PasswordSignInAsync(
                user, dto.Password, dto.RememberMe, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return (false, "Tài khoản bị khóa tạm thời do đăng nhập sai quá nhiều lần.", null);

            if (!result.Succeeded)
                return (false, "Email hoặc mật khẩu không đúng.", null);

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                Avatar = user.Avatar,
                Roles = roles
            };

            return (true, "Đăng nhập thành công!", userDto);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<UserDto?> GetCurrentUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                Avatar = user.Avatar,
                Roles = roles
            };
        }
    }
}