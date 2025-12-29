using Ace_Admin.Dto;
using Ace_Admin.Models;
using Ace_Admin.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Ace_Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        #region ---STATIC VARIABLE----
        private readonly ILogger<HomeController> _logger;
        private readonly PracticeDbContext _context;
        private readonly IConfiguration _config;
        private readonly Upservecies _upstox;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly RedisService _redis;

        #endregion

        #region ---CONTRUCTOR---
        public HomeController(ILogger<HomeController> logger, PracticeDbContext context, IConfiguration config, IMapper mapper, IHttpClientFactory httpClientFactory, RedisService redis
        )
        {
            _logger = logger;
            _context = context;
            _config = config;
            _upstox = new Upservecies(context, config);
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
            _redis = redis;
        }
        #endregion

        #region ---- View Pages -----

        [ServiceFilter(typeof(SeoActionFilter)), AllowAnonymous, Route("home")] public IActionResult Index() => View();
        [ServiceFilter(typeof(SeoActionFilter)), AllowAnonymous, Route("set-seo")] public IActionResult SetSeo() => View();
        [ServiceFilter(typeof(SeoActionFilter)), AllowAnonymous, Route("view-seo")] public IActionResult ViewSeo() => View();
        public IActionResult Privacy() => View();
        [ServiceFilter(typeof(SeoActionFilter)), AllowAnonymous, Route("login")] public IActionResult Login() => View();
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        [ServiceFilter(typeof(SeoActionFilter)), AllowAnonymous, Route("charts-show")] public IActionResult Charts() => View();

        [ServiceFilter(typeof(SeoActionFilter)), Route("/this-is-test")] public ActionResult Test() => View();
        [ServiceFilter(typeof(SeoActionFilter)), AllowAnonymous, Route("emp-profile")] public IActionResult EmpProfile() => View();


        #endregion

        #region ---  Employee Management -----
        [AllowAnonymous]
        public async Task<IActionResult> List(int? id, string? search, int page = 1, int pageSize = 5)
        {
            ViewBag.Title = "Employee List";
            ViewBag.PageTitle = "Employees";
            ViewBag.PageSubtitle = "overview & stats";
            IQueryable<Employee> query = _context.Employees.AsNoTracking();
            if (id.HasValue)
            {
                query = query.Where(e => e.Id == id.Value);
                ViewBag.DisableViewButton = true;
            }
            else
            {
                ViewBag.DisableViewButton = false;
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(e =>
                        (e.EmpName != null && e.EmpName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (e.Email != null && e.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (e.PhoneNumber != null && e.PhoneNumber.Contains(search))
                    );
                }
            }
            int totalEmployees = await query.CountAsync();
            var employees = await query.OrderBy(e => e.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalEmployees / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;
            ViewBag.PageSize = pageSize;
            return View(employees);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Employee model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Invalid data provided!";
                TempData["AlertType"] = "error";
                return RedirectToAction("List");
            }
            var emp = await _context.Employees.FindAsync(model.Id);
            if (emp == null)
            {
                TempData["Message"] = "Employee not found!";
                TempData["AlertType"] = "error";
                return RedirectToAction("List");
            }
            emp.EmpName = model.EmpName;
            emp.Email = model.Email;
            await _context.SaveChangesAsync();
            TempData["Message"] = "Employee updated successfully!";
            TempData["AlertType"] = "success";
            return RedirectToAction("List");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp != null)
            {
                _context.Employees.Remove(emp);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Employee deleted successfully!";
                TempData["AlertType"] = "success";
            }
            else
            {
                TempData["Message"] = "Employee not found!";
                TempData["AlertType"] = "error";
            }
            return RedirectToAction("List");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Error while adding employee!";
                TempData["AlertType"] = "error";
                return View("List");
            }
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Employee added successfully!";
            TempData["AlertType"] = "success";
            return RedirectToAction("List");
        }
        #endregion

        #region  --- API Endpoints -----
        [HttpPost]
        public ActionResult LoginHandler(string username, string password)
        {
            if (username == "admin" && password == "pass")
            {
                return Json(new { success = true, message = "success" });
            }
            return Json(new { sucess = false, message = "falied" });
        }
        [HttpGet("api/users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Employees.AsNoTracking().Select(u => new { id = u.Id, name = u.EmpName }).ToListAsync();
            return Ok(users);
        }

        [AllowAnonymous, ValidateAntiForgeryToken, HttpPost("api/login")]
        public async Task<IActionResult> Login([FromBody] LoginView model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(ApiResponse<object>.BadRequest("Invalid input."));

                var secretHeader = Request.Headers["X-Frontend-Secret"].FirstOrDefault();
                if (secretHeader != _config["CorsSettings:FrontendSecret"])
                    return Unauthorized("Unauthorized client");

                string origin = Request.Headers["Origin"].FirstOrDefault() ?? "";
                if (!string.IsNullOrEmpty(origin) &&
                    !origin.Contains(_config["CorsSettings:AllowedOrigin"] ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized("Invalid request origin");
                }

                var employee = await _context.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Username == model.Username);

                if (employee == null)
                    return Json(ApiResponse<object>.Unauthorized("Invalid username or password."));

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, employee.Password);
                if (!isPasswordValid)
                    return Json(ApiResponse<object>.Unauthorized("Invalid username or password."));

                string machineId = GetMachineUniqueId();

                // Generate access token
                var tokenService = new assymmetricTokenGenerate(_config);
                // Ensure 'employee.Username' is not null before calling GenerateJwtToken
                if (string.IsNullOrEmpty(employee.Username))
                {
                    _logger.LogError("Employee username is null or empty for employee ID {EmployeeId}", employee.Id);
                    return Json(ApiResponse<object>.BadRequest("Invalid employee data."));
                }

                var accessToken = tokenService.GenerateJwtToken(employee.Username, employee.Id, machineId);
                // Generate refresh token
                string refreshToken = Guid.NewGuid().ToString();

                // Save refresh token in Redis
                await _redis.SetRefreshTokenAsync(employee.Id, refreshToken);

                // Set HttpOnly Cookie for Refresh Token
                Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(30)
                });

                // Set HttpOnly Cookie for Access Token
                Response.Cookies.Append("AuthToken", accessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddMinutes(30)
                });

                string csrf = Guid.NewGuid().ToString();
                Response.Cookies.Append("X-CSRF-TOKEN", csrf, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.None
                });

                return Json(ApiResponse<object>.Ok(new
                {
                    employee = new
                    {
                        employee.Id,
                        employee.Username,
                        employee.EmpName,
                        employee.Email,
                        employee.PhoneNumber,
                        employee.Position
                    },
                    csrf
                }, "Login successful."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", model.Username);
                return Json(ApiResponse<object>.InternalServerError("An unexpected error occurred."));
            }
        }

        [HttpPost("/logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("access_token");
            return Json(ApiResponse<object>.Ok(new object(), "Logged out"));
        }
        [HttpPost, Route("token-check")]
        public async Task<ActionResult> LoginCheck()
        {
            try
            {
                var empIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


                if (string.IsNullOrEmpty(empIdClaim) || !int.TryParse(empIdClaim, out int empId) || empId <= 0)
                    return Json(ApiResponse<object>.Unauthorized("Invalid or expired token"));
                var storedRefreshToken = await _redis.GetRefreshTokenAsync(empId);
                var userDetails = await _context.Employees.Include(e => e.Wallet).Include(e => e.EmployeeCourses).ThenInclude(ec => ec.Course).AsNoTracking().FirstOrDefaultAsync(e => e.Id == empId);
                if (userDetails == null)
                    return Json(ApiResponse<object>.Unauthorized("Employee not found or unauthorized"));
                var data = _mapper.Map<EmpProfileDto>(userDetails);
                return Json(ApiResponse<object>.Ok(data, "Token Verified"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token verification");
                return Json(ApiResponse<object>.InternalServerError("An unexpected error occurred."));
            }
        }

        [HttpGet, Route("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var empIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(empIdClaim, out int empId) || empId == 0)
                return Unauthorized(new
                {
                    success = false,
                    message = "User not logged in"
                });
            var employee = await _context.Employees.Include(e => e.Wallet).Include(e => e.EmployeeCourses).ThenInclude(ec => ec.Course).AsNoTracking().FirstOrDefaultAsync(e => e.Id == empId);
            if (employee == null)
                return NotFound(new
                {
                    success = false,
                    message = "Employee not found"
                });
            var user = _mapper.Map<EmpProfileDto>(employee);
            var completedCourses = await _context.EmployeeCourses.Where(ec => ec.EmployeeId == empId && ec.Status == "Completed").Include(ec => ec.Course).AsNoTracking()
                .Select(ec => new
                {
                    ec.Course.CourseId,
                    ec.Course.CourseName,
                    ec.Status
                }).ToListAsync();
            var data = new
            {
                User = user,
                Courses = completedCourses
            };
            return Ok(new
            {
                success = true,
                data = new
                {
                    User = user,
                    Courses = completedCourses
                }
            });
        }

        [HttpPost, Route("profile/update")]
        public async Task<IActionResult> UpdateProfile([FromBody] EmpProfileDto model)
        {
            var empIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(empIdClaim, out int empId) || empId == 0)
                return Unauthorized(new
                {
                    success = false,
                    message = "User not logged in"
                });
            var employee = await _context.Employees.FindAsync(empId);
            if (employee == null)
                return NotFound(new
                {
                    success = false,
                    message = "Employee not found"
                });
            employee.EmpName = model.FullName ?? employee.EmpName;
            employee.Email = model.Email ?? employee.Email;
            employee.PhoneNumber = model.Phone ?? employee.PhoneNumber;
            employee.Location = model.Location ?? employee.Location;
            await _context.SaveChangesAsync();
            return Ok(new
            {
                success = true,
                message = "Profile updated successfully."
            });
        }
        [HttpPost, Route("get-files")]
        public async Task<IActionResult> DownloadZip()
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/image");
            string zipPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/zip/images.zip");

            // Delete old zip if exists
            if (System.IO.File.Exists(zipPath))
                System.IO.File.Delete(zipPath);

            // Create ZIP from folder
            ZipFile.CreateFromDirectory(folderPath, zipPath);

            // Read ZIP file into memory
            var memory = new MemoryStream();
            using (var stream = new FileStream(zipPath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            // Return file to user
            return File(memory,
                "application/zip",
                "files.zip");
        }
        #endregion

        #region -----Redis Integration-----
        [AllowAnonymous]
        [HttpPost("login-redis")]
        public async Task<IActionResult> Loginredis([FromBody] LoginView model)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Username == model.Username);

            if (employee == null || !BCrypt.Net.BCrypt.Verify(model.Password, employee.Password))
                return Unauthorized("Invalid username or password");

            string machineId = Guid.NewGuid().ToString();

            // ✔ Correct RSA token generation
            var tokenService = new assymmetricTokenGenerate(_config);
            string accessToken = tokenService.GenerateAccessToken(
                employee.Username,
                employee.Id,
                machineId
            );

            string refreshToken = Guid.NewGuid().ToString();

            await _redis.SetRefreshTokenAsync(employee.Id, refreshToken);
            await _redis.SetMachineIdAsync(employee.Id, machineId);

            Response.Cookies.Append("AuthToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(30)
            });

            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            Response.Cookies.Append("X-CSRF-TOKEN", Guid.NewGuid().ToString(), new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None
            });
            var user = _mapper.Map<EmpProfileDto>(employee);

            return Json(ApiResponse<object>.Ok(user, "Login Successful"));

        }

        // ---------------- TOKEN CHECK ----------------
        [HttpPost("token-check-redis")]
        public async Task<IActionResult> TokenCheckRedis()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return await Refresh(); 
                }
                var empIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(empIdClaim) || !int.TryParse(empIdClaim, out int empId))
                    return Unauthorized("Invalid or expired token");

                var storedToken = await _redis.GetRefreshTokenAsync(empId);
                if (string.IsNullOrEmpty(storedToken))
                    return Unauthorized("Session expired");

                return Json(ApiResponse<object?>.Ok(null, "Token Verified"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user ");
                return Json(ApiResponse<object>.InternalServerError("An unexpected error occurred."));
            }

        }
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            // 1️⃣ Check Refresh Token cookie exists
            if (!Request.Cookies.TryGetValue("RefreshToken", out string? refreshTokenFromCookie))
                return Unauthorized("Missing refresh token");

            int? userId = await _redis.GetUserIdByRefreshTokenAsync(refreshTokenFromCookie);

            if (userId is null || userId <= 0) { return Unauthorized("Invalid or expired refresh token"); }

            int realUserId = userId.Value;

            var storedRefreshToken = await _redis.GetRefreshTokenAsync(realUserId);
            if (storedRefreshToken == null || storedRefreshToken != refreshTokenFromCookie)
                return Unauthorized("Invalid or expired refresh token");

            // 3️⃣ Retrieve stored machine ID (device binding)
            var machineId = await _redis.GetMachineIdAsync(realUserId);
            if (string.IsNullOrEmpty(machineId))
                return Unauthorized("Machine ID missing");

            // 4️⃣ Load Employee
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == realUserId);
            if (employee == null)
                return Unauthorized("User not found");

            // 5️⃣ Generate new Access Token
            var tokenService = new assymmetricTokenGenerate(_config);
            string newAccessToken = tokenService.GenerateAccessToken(
                employee.Username,
                employee.Id,
                machineId
            );

            // 6️⃣ Generate new Refresh Token
            string newRefreshToken = Guid.NewGuid().ToString();
            await _redis.SetRefreshTokenAsync(realUserId, newRefreshToken);

            // 7️⃣ Write new cookies
            Response.Cookies.Append("AuthToken", newAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(30)
            });

            Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new { message = "Tokens refreshed successfully" });
        }

        [HttpPost("logout-redis")]
        public async Task<IActionResult> LogoutRedis()
        {
            if (!Request.Cookies.TryGetValue("RefreshToken", out string? refreshTokenFromCookie))
                return Unauthorized("Missing refresh token");

            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
                return Unauthorized("Invalid user id");


            await _redis.DeleteRefreshTokenAsync(userId);
            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("RefreshToken");
            Response.Cookies.Delete("X-CSRF-TOKEN");
            return Json(ApiResponse<object?>.Ok(null, "Logged out"));
        }
        #endregion

        #region ----- Instruments & Stocks -----
        [AllowAnonymous, HttpGet("instruments/import")]
        public async Task<IActionResult> Import()
        {
            var count = await _upstox.ImportInstrumentsAsync();
            return Ok($"{count} instruments imported.");
        }

        [ServiceFilter(typeof(SeoActionFilter)), AllowAnonymous, HttpGet("instruments")]
        public async Task<IActionResult> Stocks(string? search)
        {
            var query = _context.Instruments.AsNoTracking().AsQueryable();
            if (!string.IsNullOrEmpty(search)) query = query.Where(i => i.Symbol.StartsWith(search) || i.Name.StartsWith(search));
            var list = await query.Take(100).ToListAsync();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") return Json(list);
            return View(list);
        }
        [AllowAnonymous]
        [HttpGet("get-stocks")]
        public async Task<IActionResult> BankNiftyLive()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("NSE");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                client.DefaultRequestHeaders.Accept.ParseAdd("application/json,text/plain,*/*");

                var home = await client.GetAsync("https://www.nseindia.com");
                if (!home.IsSuccessStatusCode)
                    return Json(new { success = false, message = "Failed to load cookies" });

                // 2️⃣ Now call real API
                var apiResponse = await client.GetStringAsync("https://www.nseindia.com/api/equity-stockIndices?index=NIFTY%20BANK");

                var root = JsonSerializer.Deserialize<NiftyBankRoot>(apiResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var bankNifty = root?.data?.FirstOrDefault();
                if (bankNifty == null) return Json(new { success = false, message = "No data found" });

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        indexName = root.name,
                        symbol = bankNifty.symbol,
                        lastPrice = bankNifty.lastPrice,
                        change = bankNifty.change,
                        pChange = bankNifty.pChange,
                        open = bankNifty.open,
                        dayHigh = bankNifty.dayHigh,
                        dayLow = bankNifty.dayLow,
                        previousClose = bankNifty.previousClose,
                        totalTradedVolume = bankNifty.totalTradedVolume,
                        lastUpdateTime = bankNifty.lastUpdateTime
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [AllowAnonymous]
        [HttpGet("get-bankstock-list")]
        public async Task<IActionResult> GetBankNiftyList()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("NSE");
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                client.DefaultRequestHeaders.Accept.ParseAdd(
                    "application/json,text/plain,*/*");
                // 1️⃣ First request: Get cookies
                var home = await client.GetAsync("https://www.nseindia.com");
                if (!home.IsSuccessStatusCode)
                    return Json(new { success = false, message = "Failed to load cookies" });
                // 2️⃣ Now call real API
                var apiResponse = await client.GetStringAsync(
                    "https://www.nseindia.com/api/equity-stockIndices?index=NIFTY%20BANK");
                var root = JsonSerializer.Deserialize<NiftyBankRoot>(apiResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (root?.data == null || !root.data.Any())
                    return Json(new { success = false, message = "No data found" });
                var list = root.data.Select(item => new
                {
                    item.symbol,
                    item.lastPrice,
                    item.change,
                    item.pChange,
                    item.open,
                    item.dayHigh,
                    item.dayLow,
                    item.previousClose,
                    item.totalTradedVolume,
                    item.lastUpdateTime
                }).ToList();
                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [AllowAnonymous, HttpGet("get-index-names")]
        public async Task<IActionResult> GetIndexList()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("NSE");

                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                client.DefaultRequestHeaders.Accept.ParseAdd(
                    "application/json,text/plain,*/*");

                // Get cookies first
                var home = await client.GetAsync("https://www.nseindia.com");
                if (!home.IsSuccessStatusCode)
                    return Json(new { success = false, message = "Failed to load cookies" });

                // Real API
                var apiResponse = await client.GetStringAsync("https://www.nseindia.com/api/index-names");

                var root = JsonSerializer.Deserialize<IndexNamesResponse>(apiResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (root?.stn == null || !root.stn.Any())
                    return Json(new { success = false, message = "No data found" });

                // Convert each ["NIFTY 50","NIFTY 50"] → clean object
                var list = root.stn.Select(row => new
                {
                    indexName = row[0],
                    displayName = row[1]
                }).ToList();

                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [AllowAnonymous]
        [HttpGet("get-equity-quote")]
        public async Task<IActionResult> GetEquityQuote(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return Json(new { success = false, message = "Symbol required" });

            try
            {
                var client = _httpClientFactory.CreateClient("NSE");

                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                client.DefaultRequestHeaders.Accept.ParseAdd(
                    "application/json,text/plain,*/*");

                // Step 1: Load NSE cookies
                var home = await client.GetAsync("https://www.nseindia.com");
                if (!home.IsSuccessStatusCode)
                    return Json(new { success = false, message = "Failed to load cookies" });

                // Step 2: Call actual API
                string apiUrl = $"https://www.nseindia.com/api/quote-equity?symbol={symbol}";
                string json = await client.GetStringAsync(apiUrl);

                // Step 3: SAVE the JSON in server folder
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "NSE_Quotes");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string filePath = Path.Combine(folder, $"{symbol}_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                await System.IO.File.WriteAllTextAsync(filePath, json);

                // Step 4: Deserialize Response
                var result = JsonSerializer.Deserialize<QuoteResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region ---- Messages ----
        [HttpGet("messages/recent")]
        public async Task<IActionResult> GetRecentMessages()
        {

            try
            {
                var empIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var cutoff = DateTime.Now.AddDays(-7);
                var messages = await _context.UserMessages.Where(m => m.ReceiverId == empIdClaim && m.SentAt >= cutoff).OrderByDescending(m => m.SentAt).AsNoTracking().ToListAsync();
                var senderIds = messages.Select(m => int.TryParse(m.UserId, out int id) ? id : 0).Where(id => id > 0).Distinct().ToList();
                var employees = await _context.Employees.Where(e => senderIds.Contains(e.Id)).AsNoTracking().Select(e => new { e.Id, e.EmpName, e.FilePathPic }).ToDictionaryAsync(e => e.Id);


                var result = messages.Select(m =>
                {
                    int.TryParse(m.UserId, out int sid);
                    var emp = employees.ContainsKey(sid) ? employees[sid] : null;
                    return new
                    {
                        senderName = emp?.EmpName ?? "Unknown",
                        senderPic = string.IsNullOrEmpty(emp?.FilePathPic) ? "/assets/image/avatar/avatar.png" : emp.FilePathPic,
                        text = m.Message,
                        sentTime = m.SentAt.ToString("hh:mm tt")
                    };
                }).ToList();
                return Json(result);

            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = ex.Message });
            }

        }


        #endregion

        #region ---test---
        
        #endregion

        #region ----- Courses -----
        [HttpGet("get-courses")]
        public async Task<JsonResult> GetCourses()
        {
            var data = await _context.Courses.AsNoTracking().Select(e => new { e.CourseId, e.CourseName, e.CourseDescription }).ToListAsync();
            return Json(ApiResponse<object>.Ok(data, "Courses fetched successfully"));
        }

        [HttpGet("get-courses/{id}")]
        public async Task<JsonResult> GetCourse(int id)
        {
            var data = await _context.Courses.AsNoTracking().Where(e => e.CourseId == id).Select(e => new { e.CourseId, e.CourseName, e.CourseDescription }).FirstOrDefaultAsync();
            if (data == null)
                return Json(ApiResponse<object>.NotFound("Course not found"));
            return Json(ApiResponse<object>.Ok(data, "Course fetched successfully"));
        }

        [HttpGet("/instruments/chart")]
        public async Task<IActionResult> GetInstrumentChartData([FromQuery] string token, [FromQuery] string exchange)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(exchange))
                return BadRequest(new
                {
                    success = false,
                    message = "Token and Exchange are required"
                });
            var instrument = await _context.Instruments.AsNoTracking().FirstOrDefaultAsync(i => i.Token == token && i.Exchange == exchange);
            if (instrument == null)
                return NotFound(new
                {
                    success = false,
                    message = "Instrument not found"
                });
            var random = new Random();
            var basePrice = 100.0m + (decimal)(random.NextDouble() * 900);
            var prices = new List<decimal>();
            var timestamps = new List<DateTime>();
            for (int i = 9; i >= 0; i--)
            {
                var fluctuation = (decimal)(random.NextDouble() * 10 - 5);
                prices.Add(Math.Round(basePrice + fluctuation, 2));
                timestamps.Add(DateTime.Now.AddMinutes(-i));
            }
            var chartData = new
            {
                instrument.Token,
                instrument.Symbol,
                instrument.Exchange,
                Prices = prices,
                Timestamps = timestamps
            };
            return Ok(new
            {
                success = true,
                data = chartData
            });
        }

        #endregion

        #region--------SEO MANAGEMENT------
        [HttpPost("api/seo/save")]
        public async Task<IActionResult> SaveSeoSettings([FromBody] SeoSetting model)
        {
            var empIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Validation failed", errors = ModelState });
            }

            try
            {
                if (model.Id > 0)
                {
                    var existing = await _context.SeoSettings.FindAsync(model.Id);
                    if (existing != null)
                    {
                        existing.PageTitle = model.PageTitle;
                        existing.PageUrl = model.PageUrl;
                        existing.MetaDescription = model.MetaDescription;
                        existing.MetaKeywords = model.MetaKeywords;
                        existing.MetaAuthor = model.MetaAuthor;
                        existing.OgTitle = model.OgTitle;
                        existing.OgImage = model.OgImage;
                        existing.OgDescription = model.OgDescription;
                        existing.TwitterCard = model.TwitterCard;
                        existing.TwitterSite = model.TwitterSite;
                        existing.CanonicalUrl = model.CanonicalUrl;
                        existing.Robots = model.Robots;
                        existing.UpdatedDate = DateTime.Now;

                        _context.SeoSettings.Update(existing);
                    }
                }
                else
                {
                    model.CreatedDate = DateTime.Now;
                    model.UpdatedDate = DateTime.Now;
                    model.IsActive = true;
                    await _context.SeoSettings.AddAsync(model);
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "SEO settings saved successfully", id = model.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error saving SEO settings: " + ex.Message });
            }
        }

        [HttpGet("api/seo/get")]
        public async Task<IActionResult> GetSeoSettings([FromQuery] int id)
        {
            try
            {
                var seoSettings = await _context.SeoSettings.FindAsync(id);

                if (seoSettings == null)
                {
                    return NotFound(new { success = false, message = "SEO settings not found" });
                }

                return Ok(new { success = true, data = seoSettings });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("api/seo/list")]
        public async Task<IActionResult> GetAllSeoSettings()
        {
            try
            {
                var seoSettings = await _context.SeoSettings
                    .Where(s => s.IsActive == true)
                    .OrderByDescending(s => s.CreatedDate)
                    .ToListAsync();

                return Ok(new { success = true, data = seoSettings });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("api/seo/delete/{id}")]
        public async Task<IActionResult> DeleteSeoSettings(int id)
        {
            try
            {
                var seoSettings = await _context.SeoSettings.FindAsync(id);
                if (seoSettings == null)
                {
                    return NotFound(new { success = false, message = "SEO settings not found" });
                }

                seoSettings.IsActive = false;
                _context.SeoSettings.Update(seoSettings);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "SEO settings deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region  ----- helper methods ----
        public static string GetMachineUniqueId()
        {
            string cpu = GetCpuId() ?? "";
            string mb = GetMotherboardId() ?? "";
            string disk = GetDiskId() ?? "";

            string raw = cpu + mb + disk;

            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
        public static string? GetDiskId()
        {
            try
            {
                var searcher = new System.Management.ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive");
                foreach (var item in searcher.Get())
                {
                    return item["SerialNumber"]?.ToString()?.Trim();
                }
            }
            catch { }
            return null;
        }
        public static string? GetMotherboardId()
        {
            try
            {
                var searcher = new System.Management.ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                foreach (var item in searcher.Get())
                {
                    return item["SerialNumber"]?.ToString()?.Trim();
                }
            }
            catch { }
            return null;
        }
        public static string? GetCpuId()
        {
            try
            {
                var searcher = new System.Management.ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (var item in searcher.Get())
                {
                    return item["ProcessorId"]?.ToString()?.Trim();
                }
            }
            catch { }
            return null;
        }
        #endregion

    }
    #region ---- NSE Quote Response Models ----
    public class QuoteResponse
    {
        public Info info { get; set; }
        public Metadata metadata { get; set; }
        public PriceInfo priceInfo { get; set; }
    }

    public class Info
    {
        public string symbol { get; set; }
        public string companyName { get; set; }
        public string industry { get; set; }
    }

    public class Metadata
    {
        public string series { get; set; }
        public string status { get; set; }
        public string lastUpdateTime { get; set; }
    }

    public class PriceInfo
    {
        public decimal lastPrice { get; set; }
        public decimal change { get; set; }
        public decimal pChange { get; set; }
        public decimal previousClose { get; set; }
        public decimal open { get; set; }
        public IntraDayHighLow intraDayHighLow { get; set; }
    }

    public class IntraDayHighLow
    {
        public decimal min { get; set; }
        public decimal max { get; set; }
    }
    public class NiftyBankRoot
    {
        public string name { get; set; }
        public List<NiftyBankItem> data { get; set; }
    }
    public class IndexNamesResponse
    {
        public List<List<string>> stn { get; set; }
    }
    public class RefreshRequest
    {
        public int UserId { get; set; }
    }
    public class NiftyBankItem
    {
        public string symbol { get; set; }
        public double open { get; set; }
        public double dayHigh { get; set; }
        public double dayLow { get; set; }
        public double lastPrice { get; set; }
        public double previousClose { get; set; }
        public double change { get; set; }
        public double pChange { get; set; }
        public double yearHigh { get; set; }
        public double yearLow { get; set; }
        public long totalTradedVolume { get; set; }
        public double totalTradedValue { get; set; }
        public string lastUpdateTime { get; set; }
    }
    #endregion

}
