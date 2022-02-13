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
using RepositoryInterfaces.Enum;

namespace Repository.Interfaces
{
    public class ReposiroryPattern<T> : IRepository<T> where T : IEntityBase
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
            Transaction.Commit();
            Console.WriteLine($"ID {number1}");
            return (int)(decimal)number1;
        }

        public int Delete(int id)
        {
            SqlCommand query = new SqlCommand(sqlCommandBuilder.Remove(id), Context);

            int number = query.ExecuteNonQuery();
            Transaction.Commit();
            Console.WriteLine($"Insert {number} rows");
            return number;
        }

        public int Update(T item)
        {
            SqlCommand query = new SqlCommand(sqlCommandBuilder.Update(item), Context);

            int number1 = query.ExecuteNonQuery();
            Transaction.Commit();
            Console.WriteLine($"ID {number1}");
            return number1;
        }


        public T Include(T item, Type joinedType)
        {
            SqlCommand query = new SqlCommand(sqlCommandBuilder.Include(item), Context);

            object createdInstance = null;
            using (SqlDataReader includeReader = query.ExecuteReader())
            {
                var y = includeReader.GetType();
                if (includeReader.HasRows)
                {
                    while (includeReader.Read())
                    {  
                        var mappedInstance = MapDataToBusinessEntity();
                        createdInstance = GetNewObject(mappedInstance, includeReader);
                    }
                }
            }
            try
            {
                 return GetAllObject(createdInstance, joinedType);
            }
            catch
            {
                return MapDataToBusinessEntity();
            }
            finally
            {
                throw new Exception("The class is Abstract Or Interface. Cannot create the instance.");
            }
        }

        private T GetAllObject(object createdInstance, Type joinedType)
        {
            
            foreach (var propertyInfo in createdInstance.GetType().GetProperties())
            {
                if (propertyInfo.PropertyType == joinedType
                    || joinedType.IsAssignableTo(propertyInfo.PropertyType)
                    || propertyInfo.PropertyType.GetElementType() == joinedType)
                {
                    object generatedInstance = null;
                    object relatedObject = null;

                    if (joinedType.IsAssignableTo(propertyInfo.PropertyType))
                    {
                        generatedInstance = DynamicCreationRepository(joinedType, EntityType.IsAbstractOrInterface);
                    }
                    generatedInstance = DynamicCreationRepository(joinedType);
                   
                   
                    var method = generatedInstance.GetType().GetMethod("GetById");

                    var relatedEntityId = GetIdOfRelatedEntity(createdInstance, propertyInfo, joinedType);
                    if (relatedEntityId == null)
                    {
                        object joinedTypeExemplar = System.Runtime.Serialization.FormatterServices
          .GetUninitializedObject(joinedType);
                       
                        relatedEntityId = GetIdOfRelatedEntity(joinedTypeExemplar, propertyInfo, createdInstance.GetType());

                        relatedObject = method.Invoke(generatedInstance, new object[] { relatedEntityId });
                        foreach (var property in createdInstance.GetType().GetProperties())
                        {
                            if ((property.PropertyType == relatedObject.GetType() 
                                || property.PropertyType == relatedObject.GetType().BaseType)
                                && !property.GetIndexParameters().Any())
                            {
                                property.SetValue(createdInstance, relatedObject);
                            }
                        }
                    }

                    relatedObject = method.Invoke(generatedInstance, new object[] { relatedEntityId });
                    foreach (var property in createdInstance.GetType().GetProperties())
                    {
                        if (property.PropertyType == relatedObject.GetType() || property.PropertyType == relatedObject.GetType().BaseType)
                        {
                            property.SetValue(createdInstance, relatedObject);
                        }
                    }
                    
                }
            }
            return (T)createdInstance;
        }

        private int? GetIdOfRelatedEntity(object mainInstance, PropertyInfo parentPropertyInfo, Type joinedType, EntityRelationship relationship = EntityRelationship.Backward)
        {
            object relatedEntityId = null;
            if(relationship == EntityRelationship.Direct)
            {

            }
            foreach (var property in mainInstance.GetType().GetProperties())
            {
                FKRelationshipAttribute foreignKeyAttributeForProperty = CurrentPropertyFkAttribute(property);
                switch(relationship)
                {
                    case EntityRelationship.Direct:
                        {
                            if (foreignKeyAttributeForProperty != null)
                            {
                                if ((parentPropertyInfo.PropertyType == joinedType || joinedType.IsAssignableTo(parentPropertyInfo.PropertyType)) &&
                                        (joinedType == foreignKeyAttributeForProperty.ForeignKeyType ||
                                        joinedType.BaseType == foreignKeyAttributeForProperty.ForeignKeyType))
                                {
                                    relatedEntityId = property.GetValue(mainInstance);
                                }
                            }
                        }
                        break;
                    case EntityRelationship.Backward:
                        {
                            if (foreignKeyAttributeForProperty != null)
                            {
                                if (joinedType == foreignKeyAttributeForProperty.ForeignKeyType ||
                                        joinedType.BaseType == foreignKeyAttributeForProperty.ForeignKeyType)
                                {
                                    relatedEntityId = property.GetValue(mainInstance);
                                }
                            }
                            break;
                        }
                }
            }

            return (int?)relatedEntityId;
        }


        private object DynamicCreationRepository(Type repositoryType, EntityType entityType = EntityType.Regular)
        {
            Type constructedType;
            if (entityType == EntityType.IsAbstractOrInterface)
            {
                constructedType = typeof(ReposiroryPattern<>).MakeGenericType(repositoryType.BaseType);
            }
            else
            {
                constructedType = typeof(ReposiroryPattern<>).MakeGenericType(repositoryType);
            }
            

            object objectWithConstructor = Activator.CreateInstance(constructedType /*, new object[] { }*/);
            ConstructorInfo ctorInstance = objectWithConstructor.GetType().GetConstructor(new[] { Context.GetType(), Transaction.GetType() });
            object generatedInstance = ctorInstance.Invoke(new object[] { Context });
            return generatedInstance;
        }

        public T GetById(int id)
        {
            SqlCommand query = new(sqlCommandBuilder.FindById(id), Context);

            SqlDataReader reader = query.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    string typeInheritanceClass = "";
                    int? indexOfDicriptor = reader.GetOrdinal("Discriptor");
                    if (indexOfDicriptor != null)
                    { 
                        typeInheritanceClass = reader.GetValue((int)indexOfDicriptor).ToString();

                        var newEntity = MapDataToBusinessEntity(typeInheritanceClass);
                        return GetNewObject(newEntity, reader);
                    }
                    else
                    {
                        var newEntity = MapDataToBusinessEntity();
                        return GetNewObject(newEntity, reader);
                    }
                }
            }  

            reader.Close();
            return (T)Activator.CreateInstance(typeof(T));
        }

        public T GetByForeignKey(int id)
        {
            SqlCommand query = new(sqlCommandBuilder.FindById(id), Context);

            SqlDataReader reader = query.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    string typeInheritanceClass = "";
                    try
                    {
                        var dicriptor = reader.GetOrdinal("Discriptor");
                        typeInheritanceClass = reader.GetValue(dicriptor).ToString();

                        var newEntity = MapDataToBusinessEntity(typeInheritanceClass);
                        return GetNewObject(newEntity, reader);
                    }
                    catch
                    {
                        var newEntity = MapDataToBusinessEntity();
                        return GetNewObject(newEntity, reader);
                    }
                }
            }
            Console.WriteLine();
            reader.Close();
            return (T)Activator.CreateInstance(typeof(T));
        }

        private static T MapDataToBusinessEntity(string typeInheritanceClass = "")
        {
            Type businessEntityType = typeof(T);
            string assemblyName = businessEntityType.Assembly.GetName().Name;
            string fullNameInheritanceClass = $"{businessEntityType.Namespace.ToString()}.{typeInheritanceClass}";
            T newObject;

            if (businessEntityType.IsAbstract)
            {
                var newGeneratedObjectReference = Activator.CreateInstance(assemblyName, fullNameInheritanceClass);
                newObject = (T)newGeneratedObjectReference.Unwrap();
            }
            else
            {
                var newGeneratedObjectReference = Activator.CreateInstance(assemblyName, businessEntityType.FullName.ToString());
                newObject = (T)newGeneratedObjectReference.Unwrap();
            }
            return newObject;

        }

        private static T GetNewObject(object newObject, IDataReader dr)
        {
            PropertyInfo[] properties = newObject.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                string dbFieldName = ConvertBLLDataToDb(property);
                if (dbFieldName == null)
                {
                    continue;
                }
                else
                {
                    var propertyValue = dr[dbFieldName];
                    if (propertyValue != null)
                        property.SetValue(newObject, propertyValue);
                }
            }
            return (T)newObject;
        }

        private static string ConvertBLLDataToDb(PropertyInfo property)
        {
            if ((property.GetCustomAttribute(typeof(PKRelationshipAttribute)) as PKRelationshipAttribute) != null)
            {
                var pkAtrtribute = property.GetCustomAttribute(typeof(PKRelationshipAttribute)) as PKRelationshipAttribute;

                return pkAtrtribute.ColumnTitle;
            }
            else if ((property.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute) != null)
            {
                var fkAtrtribute = property.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute;

                return fkAtrtribute.ColumnTitle;
            }
            else if ((property.GetCustomAttribute(typeof(ColumnDefinition)) as ColumnDefinition) != null)
            {
                var columnAtrtribute = property.GetCustomAttribute(typeof(ColumnDefinition)) as ColumnDefinition;

                return columnAtrtribute.ColumnTitle;
            }
            return null;
        }

        private FKRelationshipAttribute CurrentPropertyFkAttribute(PropertyInfo property)
        {
            return property.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute;
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
