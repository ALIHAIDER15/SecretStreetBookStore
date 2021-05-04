using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.Repository
{
    public class DbTesting
    {
        private readonly ApplicationDbContext _db;

        public DbTesting(ApplicationDbContext db)
        {
            _db = db;
        }


        public  void AddCategory(Category category)
        {
            _db.Categories.Add(category);
        }
    }
}
