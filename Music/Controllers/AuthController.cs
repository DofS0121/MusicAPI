using AppointmentsAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Music.Data;
using Music.Helpers;
using Music.Models;
using Music.Services;

namespace Music.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly MusicDbContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public AuthController(MusicDbContext context, IConfiguration config, EmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        // ==========================
        // POST: api/auth/register
        // ==========================
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            if (string.IsNullOrEmpty(model.Email) ||
                string.IsNullOrEmpty(model.Password) ||
                string.IsNullOrEmpty(model.FullName))
            {
                return BadRequest(new { message = "Thiếu thông tin đăng ký" });
            }

            // Check email tồn tại
            bool exists = _context.Users.Any(u => u.Email == model.Email);
            if (exists)
            {
                return BadRequest(new { message = "Email đã tồn tại" });
            }

            var user = new User
            {
                Email = model.Email,
                PasswordHash = PasswordHelper.Hash(model.Password),
                FullName = model.FullName,
                AvatarUrl = "default.png",
                Role = "user",
                Status = "active"
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Đăng ký thành công",
                id = user.Id,
                email = user.Email,
                fullName = user.FullName
            });
        }

        // ==========================
        // POST: api/auth/login
        // ==========================
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (string.IsNullOrEmpty(model.Email) ||
                string.IsNullOrEmpty(model.Password))
            {
                return BadRequest(new { message = "Thiếu email hoặc mật khẩu" });
            }

            string passwordHash = PasswordHelper.Hash(model.Password);

            var user = _context.Users.FirstOrDefault(u =>
                u.Email == model.Email &&
                u.PasswordHash == passwordHash &&
                u.Status == "active"
            );

            if (user == null)
            {
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });
            }

            // 🔐 TẠO JWT TOKEN
            string token = JwtHelper.GenerateToken(user, _config);

            return Ok(new
            {
                message = "Đăng nhập thành công",
                token,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    fullName = user.FullName,
                    avatar = user.AvatarUrl,
                    role = user.Role
                }
            });
        }

        // ==========================
        // PUT: api/auth/update/{userId}
        // ==========================
        [HttpPut("update/{userId}")]
        public IActionResult UpdateUser(int userId, [FromBody] UpdateUserModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound(new { message = "User không tồn tại!" });

            if (!string.IsNullOrEmpty(model.FullName))
                user.FullName = model.FullName;

            if (!string.IsNullOrEmpty(model.Email))
            {
                bool exists = _context.Users.Any(x => x.Email == model.Email && x.Id != userId);
                if (exists) return BadRequest(new { message = "Email này đã được sử dụng!" });

                user.Email = model.Email;
            }

            _context.SaveChanges();

            return Ok(new
            {
                message = "Cập nhật thông tin thành công",
                user = new
                {
                    id = user.Id,
                    fullName = user.FullName,
                    email = user.Email,
                    avatar = user.AvatarUrl
                }
            });
        }

        // =============================================
        // POST: api/auth/update-avatar/{userId}
        // =============================================
        [HttpPost("update-avatar/{userId}")]
        public async Task<IActionResult> UpdateAvatar(int userId, IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
                return BadRequest(new { message = "File không hợp lệ!" });

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound(new { message = "User không tồn tại!" });

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(avatar.FileName).ToLower();
            if (!allowed.Contains(ext))
                return BadRequest(new { message = "Chỉ nhận JPG / PNG / WEBP" });

            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            // Tên file mới
            string fileName = $"user_{userId}_{Guid.NewGuid()}{ext}";
            string filePath = Path.Combine(folder, fileName);

            // Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            // Nếu có avatar cũ → xóa file cũ (optional)
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                string oldFile = Path.Combine(folder, user.AvatarUrl);
                if (System.IO.File.Exists(oldFile)) System.IO.File.Delete(oldFile);
            }

            user.AvatarUrl = fileName;
            _context.SaveChanges();

            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            string avatarUrl = $"{baseUrl}/uploads/avatars/{fileName}";

            return Ok(new
            {
                message = "Cập nhật avatar thành công",
                avatar = avatarUrl
            });
        }



        // =============================================
        // POST: api/auth/upload-avatar/{userId}
        // =============================================
        [HttpPost("upload-avatar/{userId}")]
        public async Task<IActionResult> UploadAvatar(int userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "File không hợp lệ!" });
            }

            // Check user tồn tại
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new { message = "User không tồn tại!" });
            }

            try
            {
                // Tạo thư mục uploads/avatars
                string folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "avatars"
                );

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Validate extension (khuyến nghị)
                var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                string ext = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExt.Contains(ext))
                {
                    return BadRequest(new { message = "Chỉ cho phép file ảnh (.jpg, .png, .webp)" });
                }

                // Tạo tên file
                string fileName = $"avatar_{userId}_{Guid.NewGuid()}{ext}";
                string fullPath = Path.Combine(folderPath, fileName);

                // Lưu file
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Cập nhật DB
                user.AvatarUrl = fileName;
                _context.SaveChanges();

                // URL trả về cho Flutter
                string baseUrl = $"{Request.Scheme}://{Request.Host}";
                string avatarUrl = $"{baseUrl}/uploads/avatars/{fileName}";

                return Ok(new
                {
                    message = "Upload avatar thành công!",
                    avatar = avatarUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Lỗi upload avatar",
                    error = ex.Message
                });
            }
        }

        [HttpPost("verify-password")]
        public IActionResult VerifyPassword([FromBody] VerifyPasswordRequest req)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == req.UserId);
            if (user == null) return NotFound(new { success = false, message = "Không tìm thấy user!" });

            if (user.PasswordHash != PasswordHelper.Hash(req.CurrentPassword))
                return Ok(new { success = false, message = "Mật khẩu hiện tại không đúng!" });

            return Ok(new { success = true, message = "Xác thực thành công!" });
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest req, [FromServices] EmailService emailService)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == req.UserId);
            if (user == null) return NotFound(new { success = false, message = "Không tìm thấy user!" });

            string otp = new Random().Next(100000, 999999).ToString();

            _context.Add(new PasswordOtp
            {
                UserId = req.UserId,
                OTP = otp,
                ExpireAt = DateTime.Now.AddMinutes(5),
                IsUsed = false
            });

            await _context.SaveChangesAsync();

            await emailService.SendEmailAsync(
                user.Email,
                "🔐 Mã xác minh đổi mật khẩu",
                $"<h2>Mã OTP của bạn là <b>{otp}</b></h2><p>Có hiệu lực 5 phút.</p>"
            );

            return Ok(new { success = true, message = "Đã gửi OTP qua email!" });
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest req)
        {
            var otp = _context.Set<PasswordOtp>()
                .Where(x => x.UserId == req.UserId && x.OTP == req.OTP && x.IsUsed == false && x.ExpireAt > DateTime.Now)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();

            if (otp == null)
                return Ok(new { success = false, message = "OTP không hợp lệ hoặc đã hết hạn!" });

            otp.IsUsed = true;
            _context.SaveChanges();

            return Ok(new { success = true, message = "OTP hợp lệ, tiếp tục đổi mật khẩu." });
        }

        [HttpPut("change-password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest req)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == req.UserId);
            if (user == null) return NotFound(new { success = false, message = "User không tồn tại!" });

            string newPassHash = PasswordHelper.Hash(req.NewPassword);
            string oldPass = user.PasswordHash;

            if (oldPass == newPassHash)
                return BadRequest(new { success = false, message = "Mật khẩu mới không được giống mật khẩu cũ!" });

            user.PasswordHash = newPassHash;
            _context.SaveChanges();

            return Ok(new { success = true, message = "Đổi mật khẩu thành công!" });
        }

        [HttpPost("forgot-password/send-otp")]
        public async Task<IActionResult> ForgotPasswordSendOtp([FromBody] ForgotPasswordEmail req)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == req.Email);
            if (user == null)
                return Ok(new { success = false, message = "Email không tồn tại!" });

            string otp = new Random().Next(100000, 999999).ToString();

            _context.Add(new PasswordOtp
            {
                UserId = user.Id,
                OTP = otp,
                ExpireAt = DateTime.Now.AddMinutes(5),
                IsUsed = false
            });
            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                user.Email,
                "🔐 OTP đặt lại mật khẩu",
                $"<h2>Mã OTP: <b>{otp}</b></h2><p>Có hiệu lực 5 phút.</p>"
            );

            return Ok(new { success = true, userId = user.Id, message = "OTP đã gửi!" });
        }

        public class ForgotPasswordEmail
        {
            public string Email { get; set; }
        }

        [HttpPost("get-by-email")]
        public IActionResult GetUserByEmail([FromBody] EmailRequest req)
        {
            if (string.IsNullOrEmpty(req.Email))
                return BadRequest(new { message = "Email không hợp lệ" });

            var user = _context.Users.FirstOrDefault(u => u.Email == req.Email);

            if (user == null)
                return NotFound(new { message = "Không tìm thấy user" });

            return Ok(new { user.Id, user.Email });
        }
    }
}
