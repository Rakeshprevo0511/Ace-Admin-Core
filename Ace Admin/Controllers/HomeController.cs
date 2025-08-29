using Ace_Admin.Dto;
using Ace_Admin.Models;
using Ace_Admin.Models;
using dotnet_core_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

    

namespace Ace_Admin.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PracticeDbContext _context;
        private readonly IConfiguration _config;
        public HomeController(ILogger<HomeController> logger,PracticeDbContext context, IConfiguration config)
        {
            _logger = logger;
            _context = context;
            _config = config;
        }
        #region*****View Pages ****
       
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

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

        public IActionResult List(int? id, string search, int page = 1, int pageSize = 5)
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

            return View(employees);
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
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginView model)
        {
            if (ModelState.IsValid)
            {
                var employee = _context.Employees
                    .FirstOrDefault(e => e.Username == model.Username && e.Password == model.Password);

                if (employee != null)
                {
                    var tokenService = new TokenService(_config);
                    var token = tokenService.GenerateJwtToken(employee.Username);

                    Response.Cookies.Append("AuthToken", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(60)
                    });

                    TempData["Message"] = "Login Successful! Welcome " + employee.EmpName;
                    TempData["AlertType"] = "success"; 
                    return RedirectToAction("Index");
                }

                ViewBag.Message = "Invalid Username or Password";
                ViewBag.AlertType = "error";
            }
            return View(model);
        }
    }
}
