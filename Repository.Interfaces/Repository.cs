using GameLibrary;
using Repository.Interfaces.SqlCommandBuilder;
using Repository.Interfaces.Mapping;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Reflection;
using System.Data;
using System.Linq;
using MyAttriubutes;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections;

namespace Repository.Interfaces
{
    public class ReposiroryPattern<T> : IRepository<T> where T : class
    {
        protected SqlConnection _context;
        protected SqlTransaction _transaction;

        public SqlCommandBuilder<T> sqlCommandBuilder = new SqlCommandBuilder<T>();

        public ReposiroryPattern()
        {
        }
        public ReposiroryPattern(SqlConnection context, SqlTransaction transaction)
        {
            this._context = context;
            this._transaction = transaction;
        }

        //Use Task-based asynchronous pattern

        //Task.Yield() — планирует продолжение.Как упоминалось выше,
        //асинхронный метод может закончится и синхронно. В случае, если вызван этот метод, его продолжение будет выполнено асинхронно
        //Task.WhenAll(..) — комбинатор, принимает IEnumerable/params объектов задач
        //и возвращает объект задачи, который завершиться по завершении всех переданных задач
        public async Task<int> AddAsync(T item)
        {
            SqlCommand query = new(sqlCommandBuilder.Insert(item));
           
            int number = await query.ExecuteNonQueryAsync();
            Console.WriteLine($"Insert {number} rows");
            return number;
        }

        public async Task<int> UpdateAsync(T item)
        {
            SqlCommand query = new SqlCommand(sqlCommandBuilder.Update(item));

            int number = await query.ExecuteNonQueryAsync();
            return number;
        }

        public async Task<int> DeleteAsync(int id)
        {
            SqlCommand query = new SqlCommand(sqlCommandBuilder.Remove(id));

            int number = await query.ExecuteNonQueryAsync();
            return number;
        }

        public T Include(T item, Type joinedType)
        {
            //WHERE DATAREADER IS IT???
            SqlCommand query = new SqlCommand(sqlCommandBuilder.Include(item,joinedType));
            
            int number = query.ExecuteNonQuery();
            return item;
        }

        public T GetItem(int id)
        {
            SqlCommand query = new(sqlCommandBuilder.FindById(id));

            SqlDataReader reader = query.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    return MapDataToBusinessEntity(reader);
                }
            }
            reader.Close();
            return (T)Activator.CreateInstance(typeof(T));
        }

        private static T MapDataToBusinessEntity(IDataReader dr)
        {
            Type businessEntityType = typeof(T);
            T newObject = (T)Activator.CreateInstance(typeof(T));
            Hashtable hashtable = new Hashtable();
            PropertyInfo[] properties = businessEntityType.GetProperties();
            foreach (PropertyInfo info in properties)
            {
                hashtable[info.Name.ToUpper()] = info;
            }
            while (dr.Read())
            {
                
                for (int index = 0; index < dr.FieldCount; index++)
                {
                    PropertyInfo info = (PropertyInfo)
                                        hashtable[dr.GetName(index).ToUpper()];
                    if ((info != null) && info.CanWrite)
                    {
                        info.SetValue(newObject, dr.GetValue(index), null);
                    }
                }
            }
            dr.Close();
            return newObject;
        }
    }
   
}
