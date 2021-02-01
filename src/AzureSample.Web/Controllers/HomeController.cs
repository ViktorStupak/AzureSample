using AzureSample.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AzureSample.Context.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace AzureSample.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IRepository _repository;
        private readonly BlobStorageService _blobStorage;
        private readonly QueuesService _queuesService;
        private readonly TablesService _tablesService;

        public HomeController(IRepository repository, BlobStorageService blobStorage, QueuesService queues, TablesService tablesService)
        {
            this._tablesService = tablesService;
            this._repository = repository;
            this._blobStorage = blobStorage;
            this._queuesService = queues;
        }

        public IActionResult Index()
        {
            using (LogContext.PushProperty("User", $"name: {this.User.Identity.Name}"))
                return View(_repository.GetPersons().ToList());
        }

        public async Task<IActionResult> IndexUploadResult()
        {
            using (LogContext.PushProperty("User", $"name: {this.User.Identity.Name}"))
            {
                var result = await this._tablesService.GetData();
                return View(result.ToList());
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IEnumerable<IFormFile> uploadedFile)
        {
            using (LogContext.PushProperty("User", $"name: {this.User.Identity.Name}"))
            {
                if (!ModelState.IsValid) return View("Index");
                foreach (var file in uploadedFile)
                {
                    var result = await this._blobStorage.SaveFile(file);
                    await this._queuesService.InsertMessage(result);
                }
                return RedirectToAction("Index");
            }
        }
    }
}
