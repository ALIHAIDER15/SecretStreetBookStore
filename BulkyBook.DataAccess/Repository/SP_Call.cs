using BulkyBook.DataAccess.Data;
using BulkyBook.Repository.IRepository;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulkyBook.Repository
{
    class SP_Call : ISP_Call
    {

        private readonly ApplicationDbContext _db;
        private static string ConnectionString = "";

        public SP_Call(ApplicationDbContext db)
        {
            _db = db;
            ConnectionString = db.Database.GetDbConnection().ConnectionString;
            
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        // USED TO PERFORM INSERT UPDATE DELETE FOR SINGLE OR MULTIPLE TIMES
        public void Execute(string procedureName, DynamicParameters param = null)
        {
            using( SqlConnection sqlcon = new SqlConnection(ConnectionString) )
            {
                sqlcon.Open();
                sqlcon.Execute(procedureName, param, commandType: System.Data.CommandType.StoredProcedure);
            }
        }


        //SELECTING DATA FROM SINGLE OR MULTIPLE TABLES (SINGEL RESUTL SET)
        public IEnumerable<T> List<T>(string procedureName, DynamicParameters param = null)
        {
            using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                sqlCon.Open();
                return sqlCon.Query<T>(procedureName, param, commandType: System.Data.CommandType.StoredProcedure);
            }
        }


        //SELECTING DATA FROM DATA SET , HAVE TO ADD MORE ITEM BELOW DEPENDING ON DATA SETS IN DATABASE
        public Tuple<IEnumerable<T1>, IEnumerable<T2>> List<T1, T2>(string procedureName, DynamicParameters param = null)
        {
            using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                sqlCon.Open();
                var result = SqlMapper.QueryMultiple(sqlCon, procedureName, param, commandType: System.Data.CommandType.StoredProcedure);
                var item1 = result.Read<T1>().ToList();
                var item2 = result.Read<T2>().ToList();


                if (item1 != null && item2 != null)
                {
                    return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(item1, item2);
                }

            }

            return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(new List<T1>(), new List<T2>());
        }


        //SELECTING ONE COMPLETE SINGLE ROW 
        public T OneRecord<T>(string procedureName, DynamicParameters Param = null)
        {

            using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                sqlCon.Open();
                var value = sqlCon.Query<T>(procedureName, Param, commandType: System.Data.CommandType.StoredProcedure);
                return (T)Convert.ChangeType(value.FirstOrDefault(), typeof(T));
            }
        }

        //USED TO PERFORM INSERT UPDATE DELETE AND GET FIRSR COLUMN OF FISRT ROW : 1 OR "HELLO"
        public T Single<T>(string procedureName, DynamicParameters Param = null)
        {
            using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                sqlCon.Open();
                return (T)Convert.ChangeType(sqlCon.ExecuteScalar<T>(procedureName, Param, commandType: System.Data.CommandType.StoredProcedure), typeof(T));
            }
        }

        //OTHER THAN THIS WE CAN USE BUILD IN DAPPER METHOD TO PERFORM CRUD
    }
}
