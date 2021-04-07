using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.Repository.IRepository
{
    public interface IProductyRepository : IRepository<Product>
    {
        void Update(Product product);
    }
}
