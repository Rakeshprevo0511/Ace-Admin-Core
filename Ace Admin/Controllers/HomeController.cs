using Ace_Admin.Dto;
using Ace_Admin.Models;
using dotnet_core_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore; // ✅ Needed for FirstOrDefaultAsync



namespace Ace_Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PracticeDbContext _context;
        private readonly IConfiguration _config;
        public HomeController(ILogger<HomeController> logger, PracticeDbContext context, IConfiguration config)
        {
            _logger = logger;
            _context = context;
            _config = config;
        }
        #region*****View Pages ****
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion

        [AllowAnonymous]
        public IActionResult List(int? id, string? search, int page = 1, int pageSize = 5)
        {
            ViewBag.Title = "Employee List";
            ViewBag.PageTitle = "Employees";
            ViewBag.PageSubtitle = "overview & stats";

            IQueryable<Employee> query = _context.Employees;

            if (id.HasValue)
            {
                // Filter by Id
                query = query.Where(e => e.Id == id.Value);
                ViewBag.DisableViewButton = true;
            }
            else
            {
                ViewBag.DisableViewButton = false;

                // Search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(e =>
                        e.EmpName.ToLower().Contains(search) ||
                        e.Email.ToLower().Contains(search) ||
                        e.PhoneNumber.Contains(search));

                }
            }

            // Total count after filters
            int totalEmployees = query.Count();

            // Apply pagination
            var employees = query
                            .OrderBy(e => e.Id)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            // ViewBag for pagination
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalEmployees / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;
            ViewBag.PageSize = pageSize;

            return View(employees);
        }

        [AllowAnonymous]
        [Route("emp-profile")]
        public IActionResult EmpProfile()
        {
           // or redirect with error message

            return View(); // strongly-typed view
        }

        [HttpPost]
        public IActionResult Edit(Employee model)
        {
            if (ModelState.IsValid)
            {
                var emp = _context.Employees.Find(model.Id);
                if (emp != null)
                {
                    emp.EmpName = model.EmpName;
                    emp.Email = model.Email;
                    _context.SaveChanges();
                }
                TempData["Message"] = "Employee Update successfully!";
                TempData["AlertType"] = "success";
                return RedirectToAction("List");
            }
            TempData["Message"] = "Error while adding employee!";
            TempData["AlertType"] = "error";
            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult Delete(int Id)
        {
            var emp = _context.Employees.Find(Id);
            if (emp != null)
            {
                _context.Employees.Remove(emp);
                _context.SaveChanges();
            }
            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Employees.Add(employee);
                _context.SaveChanges();
                TempData["Message"] = "Employee added successfully!";
                TempData["AlertType"] = "success";
                return RedirectToAction("List");
            }
            TempData["Message"] = "Error while adding employee!";
            TempData["AlertType"] = "error";
            return View("List");
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login([FromBody] LoginView model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid input." });

            var employee = _context.Employees
                .FirstOrDefault(e => e.Username == model.Username && e.Password == model.Password);

            if (employee != null)
            {
                var tokenService = new TokenService(_config);
                var token = tokenService.GenerateJwtToken(employee.Username, employee.Id);

                return Json(new
                {
                    success = true,
                    token = token,
                    redirectUrl = Url.Action("Index", "Home"),
                    message = "Login successful!"
                });
            }

            return Json(new { success = false, message = "Invalid Username or Password" });
        }

        [HttpPost]
        [Route("token-check")]
        public ActionResult loginCheck()
        {
            int _empid = 0;
            _empid = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var Userdetails = _context.Employees.FirstOrDefault(e => e.Id == _empid);

            return Json(new { success = true, message = "Login Succesfull", data = Userdetails });
        }

        [HttpGet]
        [Route("profile")]
        public async Task<IActionResult> GetProfile()
        {
            int empId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (empId == 0)
                return Unauthorized(new { success = false, message = "User not logged in" });

            var user = await _context.Employees
                .Where(e => e.Id == empId)
                .Select(e => new EmpProfileDto
                {
                    Id = e.Id,
                    Username= e.Username,
                    FullName = e.EmpName,
                    Email = e.Email,
                    Phone = e.PhoneNumber,
                    Location = e.Location,
                    AvatarUrl = e.FilePathPic,
                    Designation =e.Position,
                })
                .FirstOrDefaultAsync(); // ✅ works after adding Microsoft.EntityFrameworkCore

            if (user == null)
                return NotFound(new { success = false, message = "Employee not found" });

            return Ok(new { success = true, data = user });
        }

        [HttpPost]
        [Route("profile/update")]
        public async Task<IActionResult> UpdateProfile([FromBody] EmpProfileDto model)
        {
            int empId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (empId == 0)
                return Unauthorized(new { success = false, message = "User not logged in" });

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == empId);
            if (employee == null)
                return NotFound(new { success = false, message = "Employee not found" });

            // ✅ Update allowed fields
            employee.EmpName = model.FullName ?? employee.EmpName;
            employee.Email = model.Email ?? employee.Email;
            employee.PhoneNumber = model.Phone ?? employee.PhoneNumber;
            employee.Location = model.Location ?? employee.Location;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Profile updated successfully." });
        }
    }
}
