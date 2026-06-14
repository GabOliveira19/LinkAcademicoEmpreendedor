using Microsoft.AspNetCore.Mvc;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class DemoController : Controller
    {
        public IActionResult Aluno()
        {
            return View();
        }

        public IActionResult Empresa()
        {
            return View();
        }
    }
}
