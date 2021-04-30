using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }






        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {

            var UserList = _db.ApplicationUser.Include(model => model.Company).ToList();
            var UserRole = _db.UserRoles.ToList();
            var Roles = _db.Roles.ToList();

            foreach (var user in UserList)
            {
                var RoleId = UserRole.Where(u => u.UserId == user.Id).Select(u => u.RoleId).FirstOrDefault() ?? null;
                if (RoleId != null)
                {
                    user.Role = Roles.FirstOrDefault(u => u.Id == RoleId).Name;

                }
             

                if (user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }

            return Json(new { data = UserList });
        }



        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var objFromDb = _db.ApplicationUser.FirstOrDefault(u=>u.Id ==id);
            if(objFromDb == null)
            {
                return Json (new { success = false, message = "Error while Locking/Unlocking" });
            }
            if(objFromDb.LockoutEnd > DateTime.Now)
            {
                //user is currently locked, we will unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _db.SaveChanges();
            return Json(new { success = true, message = "Operation Successful." });
        }


        #endregion
    }
}



