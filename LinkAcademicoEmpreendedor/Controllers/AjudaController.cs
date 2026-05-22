using Microsoft.AspNetCore.Mvc;

namespace LinkAcademicoEmpreendedor.Controllers
{
    public class AjudaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}