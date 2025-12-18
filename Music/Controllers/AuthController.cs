using Microsoft.AspNetCore.Mvc;
using Music.Data;
using Music.Models;
using Music.Helpers;

namespace Music.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly MusicDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(MusicDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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

    }
}
