using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulkyBook.Repository
{
    public class ProductRepository : Repository<Product> , IProductyRepository
    {
        private readonly ApplicationDbContext _db;

        // JITNA MUJE SAMAJ AI HAI
        //The purpose of using base keyword here is that we are inheriting all the methods and fields from Repository<Product>
        //class means jab kuch hmray pass Repository<Product> class mai hai wo  ProductRepository class mai ajye ga BUT hamne dbSet ko
        // paretn class k constructor mai intialize  keaya hai so parenrt class ka construcotr  ham child class m inherit ni kr skty
        // to jahn jahn dbSet use huwa hoga wo child class ko ni milay ga so is leya ham Base(bd) likh kr child class m paraent class
        // ka constructor call krty han ta k dbset intialize ho jaye or child class mai jahn chayw wahn mil jaye

        public ProductRepository(ApplicationDbContext db) :base(db)
        {
            _db = db;
      
        }

     

        public void Update(Product product)
        {
            var objFromDb = _db.Products.FirstOrDefault(s => s.Id == product.Id);
            if(objFromDb != null)
            {

                if(product.ImageUrl != null)
                {
                    objFromDb.ImageUrl = product.ImageUrl;
                }

                objFromDb.ISBN = product.ISBN;
                objFromDb.Price = product.Price;
                objFromDb.Price50 = product.Price50;
                objFromDb.ListPrice = product.ListPrice;
                objFromDb.Price100 = product.Price100;
                objFromDb.Title = product.Title;
                objFromDb.Description = product.Description;
                objFromDb.CategoryId = product.CategoryId;
                objFromDb.Author = product.Author;
                objFromDb.CoverTypeId = product.CoverTypeId;


            }
        }

     
    }
}
