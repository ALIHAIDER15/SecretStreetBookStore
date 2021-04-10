﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulkyBook.Models;
using BulkyBook.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            Company company = new Company();

            //code for create
            if(id == null)
            {
                return View(company);
            }

            company = _unitOfWork.Company.Get(id.GetValueOrDefault());
            //code for update
            if (id == null)
            {
                return NotFound();
            }

            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);
                }
                else
                {
                    _unitOfWork.Company.Update(company);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
                //return RedirectToAction("index"); <= Magic String
            }

            return View(company);

        }




        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Company.GetAll();
            return Json(new { data = allObj });
        }

       [HttpDelete]
       public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Company.Get(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message ="Error while deleting" });
            }

            _unitOfWork.Company.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
