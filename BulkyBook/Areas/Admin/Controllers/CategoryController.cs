using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Repository;
using BulkyBook.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        //Just for Testing
        private readonly ApplicationDbContext _db;

        public CategoryController(IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;

        }


        [HttpGet]
        public IActionResult Index()
        {



            return View();
        }


        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            Category category = new Category();

            //code for create
            if(id == null)
            {
                return View(category);
            }

            category = _unitOfWork.Category.Get(id.GetValueOrDefault());
            //code for update
            if (id == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.Id == 0)
                {
                    //DbTesting testing = new DbTesting(_db);
                    //testing.AddCategory(category);

                    _unitOfWork.Category.Add(category);
                }
                else
                {
                    _unitOfWork.Category.Update(category);
                }
                _unitOfWork.Save();

                return RedirectToAction(nameof(Index));
                //return RedirectToAction("index"); <= Magic String
            }

            return View(category);

        }




        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Category.GetAll();
            return Json(new { data = allObj });
        }

       [HttpDelete]
       public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Category.Get(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message ="Error while deleting" });
            }

            _unitOfWork.Category.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
