using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Repository.IRepository;
using BulkyBook.Utility;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace BulkyBook.Area.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, ApplicationDbContext db , IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _db=db;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
           
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return View(productList);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            //GETTING PRODUCT THAT CONTAIN COVERTYPE AND CATEGORY USING DIFFERENT METHODS........................

            //Using Sql Query
            //select p.Title,p.Description,p.ImageUrl,ct.Name,c.Name from Products As p
            //join CoverTypes AS CT on p.CoverTypeId = CT.Id
            //join Categories AS C on P.CategoryId = C.Id
            //where p.Id = 6



            //Using Dapper with Store Procedure
            //dynamic storePro;
            //using (SqlConnection sqlCon = new SqlConnection(_db.Database.GetDbConnection().ConnectionString))
            //{
            //    sqlCon.Open();
            //    storePro = sqlCon.Query<StoreProcedureTestingVM>("GetDetailOfProduct", new { id = id }, commandType: System.Data.CommandType.StoredProcedure);
            //}


 

            //Using Extension Method Linq
            //var ProductDetail2 = _db.Products.Include("Category,CoverType").Where((u) => u.Id == id).Select((x) => new
            //{
            //    Product_Name = x.Title,
            //    Product_Description = x.Description,
            //    ImageUrl = x.ImageUrl,
            //    CoverType = x.CoverType.Name,
            //    category = x.Category.Name
            //});



            //Using Query Like Syntax Method Linq
            //var ProductDetail = from ObjProduct in _db.Products
            //                  join ObjCovertyp in _db.CoverTypes
            //                  on ObjProduct.CoverTypeId equals ObjCovertyp.Id
            //                  join Objcategory in _db.Categories
            //                  on ObjProduct.CategoryId equals Objcategory.Id
            //                  select new
            //                  {
            //                      Product_Name = ObjProduct.Title,
            //                      Product_Description = ObjProduct.Description,
            //                      ImageUrl = ObjProduct.ImageUrl,
            //                      CoverType = ObjCovertyp.Name,
            //                      category = Objcategory.Name
            //                  };


            //Using Repository Pattern
            var productFromDb = _unitOfWork.Product.GetFirstOrDefault((u) => u.Id == id, includeProperties: "Category,CoverType");

            ShoppingCart carObj = new ShoppingCart()
            {
                Product = productFromDb,
                ProductId = productFromDb.Id

            };
       
            return View(carObj);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart CartObject)
        {

            

            CartObject.Id = 0;
            if (ModelState.IsValid)
            {
                //Getting User Logined User Id
                var UserID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                CartObject.ApplicationUserId = UserID;

                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault((u)=> u.ApplicationUserId == UserID && 
                                          u.ProductId == CartObject.ProductId);

                //no records exists in database for that product for that user
                if (cartFromDb == null)
                {
                    _unitOfWork.ShoppingCart.Add(CartObject);
                }
                else
                {
                    cartFromDb.Count  += CartObject.Count;

                    //If we Comment thr Update Method Then Entity FrameWork will automatically Update the Tables as we Get the record ,
                    //update it,and uses save changes so automatically it will be updated

                    //_unitOfWork.ShoppingCart.Update(cartFromDb);
                }

                _unitOfWork.Save();

                var count = _unitOfWork.ShoppingCart.GetAll((u) => u.ApplicationUserId == CartObject.ApplicationUserId).Count();

                //Storing complex object in Session varibale using Custom Implementation
                //_httpContextAccessor.HttpContext.Session.SetObject(SD.ssShoppingCart,count);


                //Storing int  in Session varibale using Provided Implementation
                _httpContextAccessor.HttpContext.Session.SetInt32(SD.ssShoppingCart, count);

                return RedirectToAction(nameof(Index));

            }
            else
            {
                var productFromDb = _unitOfWork.Product.
                                    GetFirstOrDefault((u) => u.Id == CartObject.ProductId, includeProperties: "Category,CoverType");

                ShoppingCart carObj = new ShoppingCart()
                {
                    Product = productFromDb,
                    ProductId = productFromDb.Id

                };

                return View(carObj);

            }
            
        }






        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

 
}
