using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AdminQRCodeMVC.ViewModels;

namespace AdminQRCodeMVC.Controllers;
public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login()
    {
        var model = new LoginViewModel(); // Используйте вашу модель LoginViewModel
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home"); // Перенаправление на главную страницу после успешного входа
            }
            else
            {
                ModelState.AddModelError("", "Неверный email или пароль");
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        var model = new RegisterViewModel(); // Используйте вашу модель RegisterViewModel
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("Index", "Home"); // Перенаправление на главную страницу после успешной регистрации
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
        }

        return View(model);
    }

}
