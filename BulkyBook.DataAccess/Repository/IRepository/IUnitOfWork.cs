using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.Repository.IRepository
{
    public interface IUnitOfWork: IDisposable
    {
        ICategoryRepository Category { get; }

        ICoverTypeRepository CoverType { get; }

        IProductyRepository Product { get; }

        ICompanyRepository Company { get; }

        IApplicationUserRepository ApplicationUser { get; }

        ISP_Call SP_Call { get; }


        void Save();

    }
}
