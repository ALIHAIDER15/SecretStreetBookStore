using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.Repository.IRepository
{
    public interface ISP_Call : IDisposable
    {

        //For inserting data
        void Execute(string procedureName, DynamicParameters param = null);

        //getting single value like total count or column  name
        T Single<T>(string procedureName, DynamicParameters Param = null);

        //getting one record
        T OneRecord<T>(string procedureName, DynamicParameters Param = null);

        //getting multiple records from same table
        IEnumerable<T> List<T>(string procedureName, DynamicParameters param = null);


        //getting multiple records from multiple tables
        Tuple<IEnumerable<T1>, IEnumerable<T2>> List<T1,T2> (String procedureName, DynamicParameters param = null);
    }
}
