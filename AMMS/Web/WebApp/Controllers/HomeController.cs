using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {

        private string baseUrlApi = "http://localhost:5121/api/v1/";
        private string baseUrlKeyCloak = "http://localhost:8080/realms/amms/";
        public IActionResult Index()
        {
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        public IActionResult Login()
        {
            string urlGetToken = baseUrlKeyCloak + "protocol/openid-connect/token";


            return null;
        }


        public IActionResult SorgulaAsset()
        {
            string urlGetToken = baseUrlKeyCloak + "protocol/openid-connect/token";


            return null;
        }

        public IActionResult CreateUser()
        {

            string ulr = baseUrlApi + "user-management/Create";

            return null;
        }

    }
}
