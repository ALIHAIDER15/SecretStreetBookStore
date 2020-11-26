using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.Repository.IRepository
{
    public interface ICoverTypeRepository :IRepository<CoverType>
    {
        void Update(CoverType coverType);
    }
}
