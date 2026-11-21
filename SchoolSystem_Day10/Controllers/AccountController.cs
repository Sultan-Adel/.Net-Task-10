using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolSystem.Models;
using SchoolSystem.ViewModels;

namespace SchoolSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        private List<SelectListItem> GetAdminAssignableRoles(string? selectedRole = null)
        {
            // Admin can assign Admin or Moderator roles
            var allowedRoles = new[] { "Admin", "Moderator" };
            return allowedRoles.Select(r => new SelectListItem
            {
                Value = r,
                Text = r,
                Selected = r == selectedRole
            }).ToList();
        }

        // 1. Normal user registration endpoint (no role selection; automatically assigned "User" role)
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View(new NormalRegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(NormalRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                FullName = model.FullName.Trim()
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (await _roleManager.RoleExistsAsync("User"))
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Success"] = "Registration completed successfully!";
                return RedirectToAction("Index", "Instructor");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // 2. Admin-only register endpoint to add user with role selected from dropdown (Admin or Moderator)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AdminRegister()
        {
            var vm = new AdminRegisterViewModel
            {
                AvailableRoles = GetAdminAssignableRoles()
            };
            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminRegister(AdminRegisterViewModel model)
        {
            var allowedRoles = new[] { "Admin", "Moderator" };
            if (!allowedRoles.Contains(model.SelectedRole))
            {
                ModelState.AddModelError(nameof(model.SelectedRole), "Please select either Admin or Moderator.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableRoles = GetAdminAssignableRoles(model.SelectedRole);
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                FullName = model.FullName.Trim(),
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(model.SelectedRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.SelectedRole));
                }

                await _userManager.AddToRoleAsync(user, model.SelectedRole);
                TempData["Success"] = $"User '{user.Email}' created with role '{model.SelectedRole}'.";
                return RedirectToAction(nameof(AdminRegister));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.AvailableRoles = GetAdminAssignableRoles(model.SelectedRole);
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = model.EmailOrUsername.Contains("@")
                ? await _userManager.FindByEmailAsync(model.EmailOrUsername.Trim())
                : await _userManager.FindByNameAsync(model.EmailOrUsername.Trim());

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Instructor");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
