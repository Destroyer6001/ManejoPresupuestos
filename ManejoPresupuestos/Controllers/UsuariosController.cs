using DocumentFormat.OpenXml.Wordprocessing;
using ManejoPresupuestos.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace ManejoPresupuestos.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<Usuario> manager;
        private readonly SignInManager<Usuario> signIn;
        public UsuariosController(UserManager<Usuario> user, SignInManager<Usuario> signInManager)
        {
            manager = user;
            signIn = signInManager;
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Registro(RegistroViewModel registro)
        {
            if(!ModelState.IsValid)
            {
                return View(registro);
            }

            var usuario = new Usuario() { Email = registro.Email};
            var resultado = await manager.CreateAsync(usuario, password: registro.Password);

            if(resultado.Succeeded)
            {
                await signIn.SignInAsync(usuario, isPersistent: true);
                return RedirectToAction("Index", "Transacciones");
            }
            else
            {
                foreach(var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(registro);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Transacciones");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }

            var resultado = await signIn.PasswordSignInAsync(login.Email, login.Password, login.Recuerdame, lockoutOnFailure: false);

            if(resultado.Succeeded)
            {
                return RedirectToAction("Index", "Transacciones");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nombre de usuario o Contraseña incorrecta");
                return View(login);
            }
        }
    }
}
