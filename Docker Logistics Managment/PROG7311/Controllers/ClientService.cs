using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PROG7311.Controllers
{
    public class ClientService : Controller
    {
        // GET: ClientService
        public ActionResult Index()
        {
            return View();
        }

        // GET: ClientService/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ClientService/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ClientService/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ClientService/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ClientService/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ClientService/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ClientService/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
