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
using RepositoryInterfaces.Mapping;

namespace Repository.Interfaces
{
    public class ReposiroryPattern<T> : IRepository<T> where T : IEntityBase
    {
        private SqlConnection Context;
        private SqlCommandBuilder<T> sqlCommandBuilder = new();

        public ReposiroryPattern()
        {
        }
        public ReposiroryPattern(SqlConnection context)
        {
            this.Context = context;
        }
     
        public int Add(T item)
        {
            SqlCommand query = new(sqlCommandBuilder.Insert(item), Context);
       
            object number1 = query.ExecuteScalar();

            return (int)(decimal)number1;
        }

        public int Delete(int id)
        {
            SqlCommand query = new SqlCommand(sqlCommandBuilder.Remove(id), Context);

            int number = query.ExecuteNonQuery();
            
            return number;
        }

        public int Update(T item)
        {
            SqlCommand query = new SqlCommand(sqlCommandBuilder.Update(item), Context);

            int number = query.ExecuteNonQuery();

            return number;
        }


        public T Include(T item, Type joinedType)
        {
            SqlCommand query = new SqlCommand(sqlCommandBuilder.Include(item, joinedType), Context);

            object createdInstance = null;
            using (SqlDataReader reader = query.ExecuteReader())
            {
                var y = reader.GetType();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {  
                        var mappedInstance = ToDatabaseLayerConverter<T>.MapDataToBusinessEntity();
                        createdInstance = ToDatabaseLayerConverter<T>.FillObject(mappedInstance, reader);
                    }
                }
                reader.Close();
            }
            try
            {
                 return GetIncludedObject(createdInstance, joinedType);
            }
            catch
            {
                return ToDatabaseLayerConverter<T>.MapDataToBusinessEntity();
            }
           
        }

        public T GetById(int id)
        {
            SqlCommand query = new(sqlCommandBuilder.FindById(id), Context);

            SqlDataReader reader = query.ExecuteReader();
            object createdObjectById = null;
            object newEntity = null;
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    string typeInheritanceClass = "";
                    int? indexOfDicriptor = reader.GetOrdinal("Discriptor");
                    if (indexOfDicriptor != null)
                    {
                        typeInheritanceClass = reader.GetValue((int)indexOfDicriptor).ToString();
                        newEntity = ToDatabaseLayerConverter<T>.MapDataToBusinessEntity(typeInheritanceClass);
                    }
                    else
                    {
                        newEntity = ToDatabaseLayerConverter<T>.MapDataToBusinessEntity();
                    }
                    createdObjectById = ToDatabaseLayerConverter<T>.FillObject(newEntity, reader);
                    reader.Close();
                    return (T)createdObjectById;
                }
            }

            reader.Close();
            try
            {
                return ToDatabaseLayerConverter<T>.MapDataToBusinessEntity();
            }
            catch
            {

                throw new Exception("The data is not valid or cannot create an exemplar of interface or abstract class.");
            }

        }

        private T GetIncludedObject(object createdInstance, Type joinedType)
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

                    int? relatedEntityId = GetIdOfRelatedEntity(createdInstance, propertyInfo, joinedType);

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

        private int? GetIdOfRelatedEntity(object mainInstance, PropertyInfo parentPropertyInfo, Type joinedType)
        {
            object relatedEntityId = null;

            foreach (var property in mainInstance.GetType().GetProperties())
            {
                FKRelationshipAttribute foreignKeyAttributeForProperty = sqlCommandBuilder.validations.CurrentPropertyFkAttribute(property);

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

            object objectWithConstructor = Activator.CreateInstance(constructedType);
            ConstructorInfo ctorInstance = objectWithConstructor.GetType().GetConstructor(new[] { Context.GetType() });
            object generatedInstance = ctorInstance.Invoke(new object[] { Context });
            return generatedInstance;
        }
    }
}
