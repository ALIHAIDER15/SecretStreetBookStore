using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Repository.IRepository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBook.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ProductController(IUnitOfWork unitOfWork , IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
    }


        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Upsert(int? id)
        {
          

            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),

                // Getting dropdown list using Querry syntax
                //var List = _unitOfWork.Category.GetAll();
                //var DropDownList = from i in List select new SelectListItem { Text = i.Name, Value = i.Id.ToString() };
                //CategoryList = DropDownList;

                 CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                 {
                    Text = i.Name,
                    Value = i.Id.ToString()
                 }),

                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            
        };

            //code for create
            if(id == null)
            {
                return View(productVM);
            }

            productVM.Product = _unitOfWork.Product.Get(id.GetValueOrDefault());
            //code for update
            if (productVM.Product == null)
            {
                return NotFound();
            }

            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {

                //GETTING THE ROOT PATH
                string webRootPath = _hostEnvironment.WebRootPath;

                //GETTING THE UPLOADED FILES USING BELOW METHOD, IN HOW TO PAKISTAN METHOD WE USE HttpFileBase IN MODEL
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    string FileName = Guid.NewGuid().ToString();
                    var Extenstion = Path.GetExtension(files[0].FileName);
                    string FullName = FileName + Extenstion;

                    // GETTING UPLOAD FOLDER FULL PATH
                    var Uploads = Path.Combine(webRootPath, @"images\products");

                    //UPDATE = IF SOMEONE WATNS TO CHANGE OLD IMAGE THEN BEOLOW CONDITON WILL EXECUTE
                    if (productVM.Product.ImageUrl != null)
                    {

                        //GETTING OLD IMAGE FULL PATH
                        var imagePath = Path.Combine(webRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            //DELETING 
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    //SAVING NEW IMAGE IN FOLDER
                    using (var filesStreams = new FileStream(Path.Combine(Uploads, FullName), FileMode.Create))
                    {
                        files[0].CopyTo(filesStreams);
                    }

                    //SAVINF URL FOR DATABASE
                    productVM.Product.ImageUrl= @"\images\products\" + FullName;

                }

                // ADDING NEW PRODUCT
                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                }
                // UPDATING NEW PRODUCT
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
                //return RedirectToAction("index"); <= Magic String
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                productVM.CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
       
            }
            return View(productVM);

        }




        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = allObj });
        }

       [HttpDelete]
       public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Product.Get(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message ="Error while deleting" });
            }

            //Deleting OLD IMAGE FULL PATH
            string webRootPath = _hostEnvironment.WebRootPath;
            var imagePath = Path.Combine(webRootPath, objFromDb.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            _unitOfWork.Product.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
