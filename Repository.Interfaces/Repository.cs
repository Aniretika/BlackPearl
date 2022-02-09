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
        private SqlConnection Context;
        private SqlTransaction Transaction;

        private SqlCommandBuilder<T> sqlCommandBuilder = new SqlCommandBuilder<T>();

        public ReposiroryPattern()
        {
        }
        public ReposiroryPattern(SqlConnection context, SqlTransaction transaction)
        {
            this.Context = context;
            this.Transaction = transaction;
        }

        //Use Task-based asynchronous pattern

        //Task.Yield() — планирует продолжение.Как упоминалось выше,
        //асинхронный метод может закончится и синхронно. В случае, если вызван этот метод, его продолжение будет выполнено асинхронно
        //Task.WhenAll(..) — комбинатор, принимает IEnumerable/params объектов задач
        //и возвращает объект задачи, который завершиться по завершении всех переданных задач
        //public async Task<int> AddAsync(T item)
        //{
        //    SqlCommand query = new(sqlCommandBuilder.Insert(item), Context);
           
        //    int number = await query.ExecuteNonQueryAsync();
        //    Console.WriteLine($"Insert {number} rows");
        //    return number;
        //}

        public int Add(T item)
        {
            SqlCommand query = new(sqlCommandBuilder.Insert(item), Context);

            //int number = query.ExecuteNonQuery();
            object number1 = query.ExecuteScalar();

            Console.WriteLine($"ID {number1}");
            return (int)(decimal)number1;
        }

        public int Delete(int id)
        {
            SqlCommand query = new SqlCommand(sqlCommandBuilder.Remove(id), Context);

            int number = query.ExecuteNonQuery();
            Console.WriteLine($"Insert {number} rows");
            return number;
        }

        public int Update(T item)
        {

            SqlCommand query = new SqlCommand(sqlCommandBuilder.Update(item), Context);

            int number = query.ExecuteNonQuery();
            Console.WriteLine($"Update {number} rows");
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
            SqlCommand query = new(sqlCommandBuilder.FindById(id), Context);

            using (SqlDataReader reader = query.ExecuteReader())
            {
                var y = reader.GetType();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string typeInheritanceClass = "";
                        try
                        {
                            typeInheritanceClass = reader.GetValue(reader.GetOrdinal("Discriptor")).ToString();
                            return MapDataToBusinessEntity(reader, typeInheritanceClass);
                        }
                        catch
                        {
                            return MapDataToBusinessEntity(reader, "");
                        }
                    }
                }
            }
            //return not excecced value
            return (T)Activator.CreateInstance(typeof(T));
        }

        private static string ConvertDBNamingToBusinessEntity(string dbFieldNameByIndex, T item)
        {
            if (item.GetType().GetCustomAttributes().Any(currentAttribute=> currentAttribute
            .GetType()==typeof(PKRelationshipAttribute)))
            {

            }
            else if(item.GetType().GetCustomAttributes().Any(currentAttribute => currentAttribute
            .GetType() == typeof(FKRelationshipAttribute)))
            {

            }
            else if (item.GetType().GetCustomAttributes().Any(currentAttribute => currentAttribute
             .GetType() == typeof(ColumnDefinition)))
            {

            }
            return "";
        }

        private static T MapDataToBusinessEntity(IDataReader dr, string typeInheritanceClass)
        {
            Type businessEntityType = typeof(T);
            string assemblyName = businessEntityType.Assembly.GetName().Name;
            string fullNameInheritanceClass = $"{businessEntityType.Namespace.ToString()}.{typeInheritanceClass}";
            T newObject;
            // T newObject = CreateNewEntity(Type businessEntityType);
            if (businessEntityType.IsAbstract)
            {
                var newGeneratedObjectReference = Activator.CreateInstance(assemblyName, fullNameInheritanceClass);
                newObject = newGeneratedObjectReference.Unwrap() as T;
            }
            else
            {
                var newGeneratedObjectReference = Activator.CreateInstance(assemblyName, businessEntityType.FullName.ToString());
                newObject = newGeneratedObjectReference.Unwrap() as T;
            }
          
            Hashtable hashtable = new Hashtable();
            PropertyInfo[] properties = businessEntityType.GetProperties();
            foreach (PropertyInfo info in properties)
            {
                hashtable[info.Name.ToUpper()] = info;
            }
            for (int indexField = 0; indexField < dr.FieldCount; indexField++)
            {
                //column title matching with data layer

                var t = dr.GetName(indexField);
                ConvertDBNamingToBusinessEntity(t, newObject);
                PropertyInfo info = (PropertyInfo)
                                    hashtable[dr.GetName(indexField).ToUpper()];
                if ((info != null) && info.CanWrite)
                {
                    info.SetValue(newObject, dr.GetValue(indexField), null);
                }
                else if ((info != null) && info.CustomAttributes.Any(u => u.AttributeType == typeof(PKRelationshipAttribute)))
                {
                    info.SetValue(newObject, dr.GetValue(indexField));
                }
            }
            return newObject;
        }

        //public async Task<int> UpdateAsync(T item)
        //{
        //    SqlCommand query = new SqlCommand(sqlCommandBuilder.Update(item));

        //    int number = await query.ExecuteNonQueryAsync();
        //    return number;
        //}


        //public async Task<int> DeleteAsync(int id)
        //{
        //    SqlCommand query = new SqlCommand(sqlCommandBuilder.Remove(id));

        //    int number = await query.ExecuteNonQueryAsync();
        //    return number;
        //}

    }

}
