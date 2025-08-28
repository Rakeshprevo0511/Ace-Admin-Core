using Ace_Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Ace_Admin.Models;

namespace Ace_Admin.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PracticeDbContext _context;
        public HomeController(ILogger<HomeController> logger,PracticeDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult List (int? id, int page = 1, int pageSize = 5)
        {
            ViewBag.Title = "Employee List";
            ViewBag.PageTitle = "Employees";           // <-- must set
            ViewBag.PageSubtitle = "overview & stats"; // <-- must set
            List<Employee> employees;
            int totalEmployees;

            if (id.HasValue)
            {
                employees = _context.Employees.Where(e => e.Id == id.Value).ToList();
                ViewBag.Title = "Employee List";
                ViewBag.DisableViewButton = true;
                ViewBag.TotalPages = 1;
                ViewBag.CurrentPage = 1;
            }
            else
            {
                ViewBag.DisableViewButton = false;
                ViewBag.Title = "Employee List";
                totalEmployees = _context.Employees.Count();
                employees = _context.Employees
                                    .OrderBy(e => e.Id)
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();

                ViewBag.TotalPages = (int)Math.Ceiling((double)totalEmployees / pageSize);
                ViewBag.CurrentPage = page;
            }

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
            }
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
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
