using System.IdentityModel.Tokens.Jwt;

using ShopManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ShopManagementAPI.Jwt;
using ShopManagementAPI.Exceptions;
using ShopManagementAPI.DTOs.Common;
using ShopManagementAPI.Extensions;
using Microsoft.Extensions.Options;
using ShopManagementAPI.Configurations;
using ShopManagementAPI.DTOs.request.Auth;
using ShopManagementAPI.DTOs.response.Auth;
using ShopManagementAPI.DTOs.response.User;

namespace ShopManagementAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly JwtSettings _jwtSettings;
        private readonly JwtService _jwtService;
        private readonly OtpService _otpService;

        public AuthController(AuthService authService, IOptions<JwtSettings> jwtOptions, JwtService jwtService, OtpService otpService)
        {
            _authService = authService;
            _jwtSettings = jwtOptions.Value;
            _jwtService = jwtService;
            _otpService = otpService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> Login(LoginRequestDTO dto)
        {
            var (response, refreshToken) =
                await _authService.LoginAsync(dto);

            // set cookie
            Response.Cookies.Append(
                "refreshToken",
                refreshToken,
                _jwtService.GetRefreshTokenCookieOptions()
            );

            return this.ApiOk(
                response,
                "Đăng nhập thành công"
            );
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<RefreshResponseDTO>>> Refresh()
        {
            // lấy refresh token từ cookie
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedException("Refresh token không tồn tại hoặc đã bị xóa");

            // gọi service
            var (response, newRefreshToken) =
                await _authService.RefreshTokenAsync(refreshToken);

            // set refresh token mới vào cookie 
            Response.Cookies.Append(
                "refreshToken",
                refreshToken,
                _jwtService.GetRefreshTokenCookieOptions()
            );

            return this.ApiOk(
                response,
                "Làm mới token thành công"
            );
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<RegisterResponseDTO>>> Register(
            RegisterRequestDTO dto)
        {
            var result = await _authService.RegisterAsync(dto);

            return this.ApiCreated(
                result,
                "Đăng ký tài khoản thành công"
            );
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserProfileResponseDTO>>> GetProfile()
        {
            var result = await _authService.GetProfileAsync();

            return this.ApiOk(
                result,
                "Lấy thông tin cá nhân thành công"
            );
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<object>>> Logout()
        {
            // lấy refresh token từ cookie
            var refreshToken = Request.Cookies["refreshToken"];

            // gọi service logout
            await _authService.LogoutAsync(refreshToken);

            // xóa refresh token trên cookie
            Response.Cookies.Delete("refreshToken");

            return this.ApiOk<object>(
                null,
                "Đăng xuất thành công"
            );
        }

        //===Change Password===//
        [Authorize]
        [HttpPost("change-password/send-otp")]
        public async Task<ActionResult<ApiResponse<object>>> SendChangePasswordOtp()
        {
            await _otpService.SendChangePasswordOtpAsync();

            return this.ApiOk<object>(
                null,
                "OTP đã được gửi đến email của bạn."
            );
        }

        [Authorize]
        [HttpPost("change-password/verify-otp")]
        public async Task<ActionResult<ApiResponse<VerifyOtpResponseDTO>>> VerifyChangePasswordOtp(
        VerifyChangePasswordOtpRequest request)
        {
            var response = await _otpService.VerifyChangePasswordOtpAsync(request);

            return this.ApiOk(
                response,
                "Xác thực OTP thành công."
            );
        }

        [Authorize]
        [HttpPost("change-password/change")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
        [FromBody] ChangePasswordRequest request)
        {
            await _otpService.ChangePasswordAsync(request);

            return this.ApiOk<object>(
                null,
                "Mật khẩu thay đổi thành công."
            );
        }

        //===Forget Password===//
        [HttpPost("forgot-password/send-otp")]
        public async Task<ActionResult<ApiResponse<object>>> SendForgotPasswordOtp(
            [FromBody] ForgotPasswordOtpRequest request)
        {
            await _otpService.SendForgotPasswordOtpAsync(request.Email);

            return this.ApiOk<object>(
                null,
                "OTP đã được gửi đến email của bạn."
            );
        }

        [HttpPost("forgot-password/verify-otp")]
        public async Task<ActionResult<ApiResponse<VerifyOtpResponseDTO>>> VerifyForgotPasswordOtp(
        [FromBody] VerifyForgotPasswordOtpRequest request)
        {
            var response =
                await _otpService.VerifyForgotPasswordOtpAsync(request);

            return this.ApiOk(
                response,
                "Xác thực OTP thành công.");
        }

        [HttpPost("forgot-password/reset")]
        public async Task<ActionResult<ApiResponse<object>>> ResetForgotPassword(
        [FromBody] ForgotPasswordRequest request)
        {
            await _otpService.ForgotPasswordAsync(request);

            return this.ApiOk<object>(
                null,
                "Đặt lại mật khẩu thành công.");
        }
        /*[Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
            ChangePasswordRequestDTO request)
        {
            await _authService.ChangePasswordAsync(request);

            return this.ApiOk<object>(
                null,
                "Đổi mật khẩu thành công. Vui lòng đăng nhập lại."
            );
        }*/
    }
}
