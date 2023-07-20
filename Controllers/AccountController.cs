using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AdminQRCodeMVC.ViewModels;
using Microsoft.AspNetCore.Authentication;

namespace AdminQRCodeMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        // Конструктор контроллера с внедрением UserManager и SignInManager через Dependency Injection
        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]   // .../Account/Logout
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Выход пользователя из системы с помощью метода SignOutAsync
            await _signInManager.SignOutAsync();

            // Очистка сессии с помощью метода SignOutAsync из Microsoft.AspNetCore.Authentication
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home"); // Перенаправление на главную страницу после выхода
        }

        
        [HttpGet]   // .../Account/Login
        public IActionResult Login()
        {
            var model = new LoginViewModel(); // Создание модели LoginViewModel для передачи в представление
            return View(model); // Возврат представления, в котором будет отображаться форма для входа
        }

        
        [HttpPost]   // .../Account/Login
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid) // Проверка, что данные в модели валидны (email и пароль введены корректно)
            {
                // Попытка аутентификации пользователя с помощью метода PasswordSignInAsync
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded) // Если аутентификация успешна
                {
                    return RedirectToAction("Index", "Home"); // Перенаправление на главную страницу после успешного входа
                }
                else
                {
                    ModelState.AddModelError("", "Неверный email или пароль"); // Ошибка аутентификации, добавляем ошибку в ModelState
                }
            }

            return View(model); // Возврат представления с моделью для отображения ошибок
        }

        
        [HttpGet]  // .../Account/Register
        public IActionResult Register()
        {
            var model = new RegisterViewModel(); // Создание модели RegisterViewModel для передачи в представление
            return View(model); // Возврат представления, в котором будет отображаться форма для регистрации
        }

        
        [HttpPost]  // .../Account/Register
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid) // Проверка, что данные в модели валидны (email и пароль введены корректно)
            {
                // Создание нового пользователя с помощью класса IdentityUser и UserManager
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                // Попытка создания нового пользователя с помощью метода CreateAsync
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded) // Проверка создан ли пользователь 
                {
                    // Попытка аутентификации только что созданного пользователя с помощью метода SignInAsync
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home"); // Перенаправление на главную страницу после успешной регистрации
                }
                else
                {
                    // Если возникли ошибки при создании пользователя, добавляем их в ModelState
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(model); // Возврат представления с моделью для отображения ошибок
        }
    }
}
