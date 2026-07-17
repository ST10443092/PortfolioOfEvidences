using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PROG7311.Controllers
{
    public class AdminView : Controller
    {
        // GET: AdminView
        public ActionResult Index()
        {
            return View();
        }

        // GET: AdminView/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AdminView/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AdminView/Create
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

        // GET: AdminView/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AdminView/Edit/5
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

        // GET: AdminView/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AdminView/Delete/5
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
