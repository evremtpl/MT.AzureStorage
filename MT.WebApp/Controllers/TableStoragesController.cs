using System;

using System.Linq;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using MT.AzureStorageLib.Entities;
using MT.AzureStorageLib.Services.Concrete;
using MT.AzureStorageLib.Services.Interfaces;

namespace MT.WebApp.Controllers
{
    public class TableStoragesController : Controller
    {
        private readonly TableStorage<UserPicture> _noSqlStorage;

        public TableStoragesController(TableStorage<UserPicture> noSqlStorage)
        {
            _noSqlStorage = noSqlStorage;
        }

        public IActionResult Index()
        {
            ViewBag.people = _noSqlStorage.All().ToList();
            ViewBag.IsUpdate = false;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserPicture person)
        {
        
            await _noSqlStorage.AddAsync(person);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Update(string rowKey, string partitionKey)
        {
            var product = await _noSqlStorage.GetAsync(rowKey, partitionKey);

            ViewBag.people = _noSqlStorage.All().ToList();
            ViewBag.IsUpdate = true;

            return View("Index", product);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UserPicture person)
        {
            ViewBag.IsUpdate = true;

            await _noSqlStorage.UpdateAsync(person);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string rowKey, string partitionKey)
        {
            await _noSqlStorage.DeleteAsync(rowKey, partitionKey);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Query(string name)
        {
            ViewBag.IsUpdate = false;
            ViewBag.people = _noSqlStorage.Query(x => x.Name == name).ToList();

            return View("Index");
        }
    }
}