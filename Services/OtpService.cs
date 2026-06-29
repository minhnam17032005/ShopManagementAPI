using Microsoft.EntityFrameworkCore;
using ShopManagementAPI.Models.Enum;
using ShopManagementAPI.Data;
using ShopManagementAPI.Models;
using Microsoft.Extensions.Configuration.UserSecrets;
using ShopManagementAPI.Exceptions;
using ShopManagementAPI.Jwt;
using ShopManagementAPI.Helpers;
using ShopManagementAPI.Configurations;
using Microsoft.Extensions.Options;
using ShopManagementAPI.DTOs.request.Auth;
using ShopManagementAPI.DTOs.response.Auth;
using ShopManagementAPI.Repositories;
using ShopManagementAPI.BusinessHelpers;

namespace ShopManagementAPI.Services
{
    public class OtpService
    {
        private readonly CurrentUserService _currentUser;
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly OtpSettings _otpSettings;
        private readonly UserRepository _repoUser;
        private readonly EmailOtpRepository _repoEmailOtp;
        private readonly OtpVerificationRepository _repoOtpVerification;
        private readonly UnitOfWork _unitOfWork;

        public OtpService(CurrentUserService currentUser,AppDbContext context, EmailOtpRepository repoEmailOtp, OtpVerificationRepository repoOtpVerification,
            EmailService emailService,IOptions<OtpSettings> otpSettings, UserRepository repoUser, UnitOfWork unitOfWork)
        {
            _currentUser = currentUser;
            _context = context;
            _emailService = emailService;
            _otpSettings = otpSettings.Value;
            _repoUser = repoUser;
            _repoEmailOtp = repoEmailOtp;
            _repoOtpVerification = repoOtpVerification;
            _unitOfWork = unitOfWork;
        }

        // Tạo và gửi OTP để đổi mật khẩu cho người dùng đang đăng nhập
        public async Task SendChangePasswordOtpAsync()
        {
            // Lấy thông tin người dùng hiện tại
            var userId = _currentUser.UserId;

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new NotFoundException("Không tìm thấy user.");

            // Tạo OTP mới và gửi qua email
            await CreateAndSendOtpAsync(
                user,
                OtpType.ChangePassword,
                "Change Password OTP",
                "Bạn vừa yêu cầu thay đổi mật khẩu. Vui lòng sử dụng mã OTP bên dưới để tiếp tục.");
        }

        public async Task<VerifyOtpResponseDTO> VerifyChangePasswordOtpAsync(
        VerifyChangePasswordOtpRequest request)
        {
            // Lấy user hiện tại
            var userId = _currentUser.UserId;

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new NotFoundException("Không tìm thấy user.");

            // Lấy OTP còn hiệu lực
            var emailOtp = await _context.EmailOtps
                .Where(x =>
                    x.UserId == userId &&
                    x.Type == OtpType.ChangePassword &&
                    x.UsedAt == null &&
                    x.RevokedAt == null &&
                    x.ExpiredAt > DateTime.UtcNow)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (emailOtp == null)
                throw new BadRequestException("OTP không tồn tại hoặc đã hết hạn.");

            // Kiểm tra OTP
            if (!HashHelper.Verify(request.Otp, emailOtp.TokenHash))
            {
                throw new BadRequestException("OTP không chính xác.");
            }

            // Đánh dấu OTP đã sử dụng
            emailOtp.UsedAt = DateTime.UtcNow;

            // Thu hồi Verification Token cũ
            var activeTokens = await _context.OtpVerifications
                .Where(x =>
                    x.UserId == userId &&
                    x.Type == OtpType.ChangePassword &&
                    x.UsedAt == null &&
                    x.RevokedAt == null &&
                    x.ExpiredAt > DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            // Sinh Verification Token mới
            var verificationToken = TokenGenerator.GenerateSecureToken();

            var expiredAt = DateTime.UtcNow.AddMinutes(_otpSettings.ExpiredMinutes);

            // Lưu Hash của Verification Token
            _context.OtpVerifications.Add(new OtpVerification
            {
                UserId = user.Id,
                Email = user.Email,
                Type = OtpType.ChangePassword,
                TokenHash = HashHelper.Hash(verificationToken),
                ExpiredAt = expiredAt
            });

            await _context.SaveChangesAsync();

            // Trả token gốc cho client
            return new VerifyOtpResponseDTO
            {
                VerificationToken = verificationToken,
                ExpiredAt = expiredAt
            };
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest request)
        {
            int userId = _currentUser.UserId;

            User? user = await _repoUser.GetByIdAsync(userId);

            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng.");
            }

            // Kiểm tra mật khẩu hiện tại
            bool isCurrentPasswordValid =
                    PasswordHelper.Verify(
                        request.CurrentPassword,
                        user.PasswordHash);
            if (!isCurrentPasswordValid)
            {
                throw new BadRequestException("Mật khẩu hiện tại không đúng.");
            }

            // Không cho đổi sang mật khẩu cũ
            bool isSamePassword =
                    PasswordHelper.Verify(
                        request.NewPassword,
                        user.PasswordHash);
            if (isSamePassword)
            {
                throw new BadRequestException("Mật khẩu mới phải khác mật khẩu hiện tại.");
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                throw new BadRequestException("Mật khẩu xác nhận không trùng với mật khẩu mới.");
            }
            // Hash verification token
            string tokenHash = HashHelper.Hash(request.VerificationToken);

            // Lấy verification token hợp lệ
            OtpVerification? verification = await _repoOtpVerification.GetValidAsync(
                userId,
                tokenHash,
                OtpType.ChangePassword);

            if (verification == null)
            {
                throw new BadRequestException("verification token không hợp lệ.");
            }

            // Kiểm tra hết hạn
            if (verification.ExpiredAt <= DateTime.UtcNow)
            {
                throw new BadRequestException("Verification token đã hết hạn.");
            }

            // Đổi mật khẩu
            user.PasswordHash =PasswordHelper.Hash(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            // Đánh dấu verification token đã sử dụng
            verification.UsedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        // Tạo và gửi OTP để đặt lại mật khẩu
        public async Task SendForgotPasswordOtpAsync(string email)
        {
            // Tìm người dùng theo email
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                throw new NotFoundException("Không tìm thấy user.");

            // Tạo OTP mới và gửi qua email
            await CreateAndSendOtpAsync(
                user,
                OtpType.ForgotPassword,
                "Forgot Password OTP",
                "Bạn vừa yêu cầu đặt lại mật khẩu. Vui lòng sử dụng mã OTP bên dưới để tiếp tục.");
        }

        public async Task<VerifyOtpResponseDTO> VerifyForgotPasswordOtpAsync(
        VerifyForgotPasswordOtpRequest request)
        {
            // Tìm user theo email
            User? user = await _repoUser.GetByEmailAsync(request.Email);

            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng.");
            }

            // Lấy OTP hợp lệ
            EmailOtp? emailOtp =
                await _repoEmailOtp.GetValidAsync(
                    user.Id,
                    OtpType.ForgotPassword);

            if (emailOtp == null)
            {
                throw new BadRequestException("OTP không tồn tại hoặc đã hết hạn.");
            }

            // Verify OTP
            bool isOtpValid =
                HashHelper.Verify(
                    request.Otp,
                    emailOtp.TokenHash);

            if (!isOtpValid)
            {
                throw new BadRequestException("OTP không chính xác.");
            }

            // Đánh dấu OTP đã dùng
            emailOtp.UsedAt = DateTime.UtcNow;

            // Thu hồi Verification Token cũ
            var activeTokens =
                await _repoOtpVerification.GetActiveTokensAsync(
                    user.Id,
                    OtpType.ForgotPassword);

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            // Sinh Verification Token mới
            string verificationToken =
                TokenGenerator.GenerateSecureToken();

            DateTime expiredAt =
                DateTime.UtcNow.AddMinutes(_otpSettings.ExpiredMinutes);

            await _repoOtpVerification.AddAsync(
                new OtpVerification
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Type = OtpType.ForgotPassword,
                    TokenHash = HashHelper.Hash(verificationToken),
                    ExpiredAt = expiredAt
                });

            await _unitOfWork.SaveChangesAsync();

            return new VerifyOtpResponseDTO
            {
                VerificationToken = verificationToken,
                ExpiredAt = expiredAt
            };
        }

        public async Task ForgotPasswordAsync(
        ForgotPasswordRequest request)
        {
            // Kiểm tra confirm password
            if (request.NewPassword != request.ConfirmPassword)
            {
                throw new BadRequestException(
                    "Mật khẩu xác nhận không trùng với mật khẩu mới.");
            }

            // Hash Verification Token
            string tokenHash = HashHelper.Hash(request.VerificationToken);

            // Lấy Verification Token hợp lệ
            OtpVerification? verification =
                await _repoOtpVerification.GetValidByTokenAsync(
                    tokenHash,
                    OtpType.ForgotPassword);

            if (verification == null)
            {
                throw new BadRequestException(
                    "Verification token không hợp lệ hoặc đã hết hạn.");
            }

            // Lấy User từ Verification
            User? user = await _repoUser.GetByIdAsync(
                verification.UserId);

            if (user == null)
            {
                throw new NotFoundException(
                    "Không tìm thấy người dùng.");
            }

            // Không cho dùng lại mật khẩu cũ
            bool isSamePassword =
                BCrypt.Net.BCrypt.Verify(
                    request.NewPassword,
                    user.PasswordHash);

            if (isSamePassword)
            {
                throw new BadRequestException(
                    "Mật khẩu mới phải khác mật khẩu hiện tại.");
            }

            // Đổi mật khẩu
            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    request.NewPassword);

            user.UpdatedAt = DateTime.UtcNow;

            // Đánh dấu Verification Token đã sử dụng
            verification.UsedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        // Tạo OTP mới, lưu vào database và gửi email
        private async Task CreateAndSendOtpAsync(User user,OtpType otpType,string subject,string message)
        {
            // Thu hồi OTP cũ còn hiệu lực
            var activeOtps = await _context.EmailOtps
                .Where(x =>
                    x.UserId == user.Id &&
                    x.Type == otpType &&
                    x.UsedAt == null &&
                    x.RevokedAt == null &&
                    x.ExpiredAt > DateTime.UtcNow)
                .ToListAsync();

            foreach (var otp in activeOtps)
            {
                otp.RevokedAt = DateTime.UtcNow;
            }

            // Sinh OTP mới
            var otpCode = OtpGenerator.GenerateOtp();

            _context.EmailOtps.Add(new EmailOtp
            {
                UserId = user.Id,
                Email = user.Email,
                TokenHash = HashHelper.Hash(otpCode),
                Type = otpType,
                ExpiredAt = DateTime.UtcNow.AddMinutes(_otpSettings.ExpiredMinutes)
            });

            await _context.SaveChangesAsync();

            // Gửi email
            await _emailService.SendOtpAsync(
                user.Email,
                subject,
                message,
                otpCode);
        }


    }
}