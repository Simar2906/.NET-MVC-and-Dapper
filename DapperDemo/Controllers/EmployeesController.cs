using Microsoft.AspNetCore.Mvc;
using DapperDemo.Models;
using DapperDemo.Repository;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DapperDemo.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ICompanyRepository _compRepo;
        private readonly IEmployeeRepository _empRepo;
        private readonly IBonusRepository _bonRepo;
        [BindProperty]
        public Employee Employee { get; set; }

        public EmployeesController(IEmployeeRepository empRepo, ICompanyRepository compRepo, IBonusRepository bonRepo)
        {
            _compRepo = compRepo;
            _empRepo = empRepo;
            _bonRepo = bonRepo;
        }

        public async Task<IActionResult> Index(int companyId = 0)
        {
            //List<Employee> employees = _empRepo.GetAll();
            //foreach(Employee obj in employees)
            //{
            //    obj.Company = _compRepo.Find(obj.CompanyId);
            //}

            List<Employee> employees = _bonRepo.GetEmployeeWithCompany(companyId);
            return View(employees);
        }
        public IActionResult Create()
        {

            IEnumerable<SelectListItem> companyList = _compRepo.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.CompanyId.ToString()
            });
            ViewBag.CompanyList = companyList;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePOST()
        {
            if (ModelState.IsValid)
            {
                await _empRepo.AddAsync(Employee);
                return RedirectToAction(nameof(Index));
            }
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    await Console.Out.WriteLineAsync(error.ErrorMessage);
                }
            }
            return View(Employee);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Employee = _empRepo.Find(id.GetValueOrDefault());

            IEnumerable<SelectListItem> companyList = _compRepo.GetAll().Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.CompanyId.ToString()
            });
            ViewBag.CompanyList = companyList;
            if (Employee == null)
            {
                return NotFound();
            }
            return View(Employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            if (id != Employee.EmployeeId)
            {
                return NotFound();
            }
            await Console.Out.WriteLineAsync("Hello");
            if (ModelState.IsValid)
            {
                _empRepo.Update(Employee);
                await Console.Out.WriteLineAsync("Heasfasfafasllo");
                return RedirectToAction(nameof(Index));
            }
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    await Console.Out.WriteLineAsync(error.ErrorMessage);
                }
            }
            return View(Employee);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            _empRepo.Remove(id.GetValueOrDefault());
            return RedirectToAction(nameof(Index));
        }
    }
}
