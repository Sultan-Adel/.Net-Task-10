using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolSystem.ViewModels;

namespace SchoolSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.Select(r => r.Name!).ToList();
            return View(roles);
        }

        // Endpoint: /Role/AddRole (Authorized for authenticated user with role Admin)
        [HttpGet]
        public IActionResult AddRole()
        {
            return View(new AddRoleViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(AddRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var roleName = model.RoleName.Trim();
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                ModelState.AddModelError(nameof(model.RoleName), $"Role '{roleName}' already exists.");
                return View(model);
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                TempData["Success"] = $"Role '{roleName}' added successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}
