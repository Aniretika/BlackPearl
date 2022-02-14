using GameLibrary;
using MyAttriubutes;
using RepositoryInterfaces.SqlCommandBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Repository.Interfaces.Mapping
{
    public class DataSourceTransormation<T> where T : IEntityBase
    {
        private Validations validations = new();
        public DataSourceTransormation() { }
        public Dictionary<string, object> GetDataForInsertQuery(object mainInstance)
        {
            var dataQuery = new Dictionary<string, object>();
           
            foreach (var propertyInfo in mainInstance.GetType().GetProperties())
            {
                var columnAttribute = validations.CurrentPropertyColumnAttribute(propertyInfo);
                if (columnAttribute != null)
                {   
                    string attributeTitle = columnAttribute.ColumnTitle.ToString();
                    dataQuery = dataQuery.Concat(ConvertBusinessLogicDataToDatabase(propertyInfo, mainInstance, attributeTitle))
                     .ToDictionary(x => x.Key, x => x.Value);
                }
                else if ((propertyInfo.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute) != null)
                {
                    var fkAttribute = propertyInfo.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute;
                    string attributeTitle = fkAttribute.ColumnTitle.ToString();
                    dataQuery = dataQuery.Concat(ConvertBusinessLogicDataToDatabase(propertyInfo, mainInstance, attributeTitle))
                       .ToDictionary(x => x.Key, x => x.Value);
                }
            }
            if (mainInstance.GetType().BaseType.GetTypeInfo().IsAbstract)
            {
                dataQuery.Add("Discriptor", $"'{mainInstance.GetType().Name}'");
            }
            return dataQuery;
        }

        private Dictionary<string, object> ConvertBusinessLogicDataToDatabase(PropertyInfo propertyInfo, object objectInstance, string attributeTitle)
        {
            var dataQuery = new Dictionary<string, object>();

            if (propertyInfo.GetValue(objectInstance) != null)
            {
                switch (propertyInfo.PropertyType.Name.ToString())
                {
                    case "String" or "Boolean":
                        dataQuery.Add(attributeTitle, $"'{propertyInfo.GetValue(objectInstance)}'");
                        break;
                    case "DateTime":
                        var dateTime = (DateTime)propertyInfo.GetValue(objectInstance);
                        dataQuery.Add(attributeTitle, $"'{dateTime.ToString("yyyy-MM-dd")}'");
                        break;
                    default:
                        dataQuery.Add(attributeTitle, propertyInfo.GetValue(objectInstance).ToString());
                        break;
                }
            }
            return dataQuery;
        }

        public string GetFkColumnTitleDependedEntity(object principalEntity, object dependedEntity)
        {
            FKRelationshipAttribute fkAttribute = null;
            foreach (var propertyInfo in dependedEntity.GetType().GetProperties())
            {
                object[] attributes = propertyInfo.GetCustomAttributes(true);
                foreach (object attribute in attributes)
                {
                    fkAttribute = attribute as FKRelationshipAttribute;
                    var t = principalEntity.GetType();
                    if (fkAttribute != null)
                    {
                        if (fkAttribute.ForeignKeyType == principalEntity.GetType())
                        {
                            return fkAttribute.ColumnTitle;
                        }
                        
                        else if (fkAttribute.ForeignKeyType.IsAssignableFrom(principalEntity.GetType()))
                        {
                            return fkAttribute.ColumnTitle;
                        }
                    }
                   
                }
            }
            return null;
        }

        public object GetPrimaryKeyValue(object objectInstance)
        {
            foreach (var propertyInfo in objectInstance.GetType().GetProperties())
            {
                if ((propertyInfo.GetCustomAttribute(typeof(PKRelationshipAttribute)) as PKRelationshipAttribute) != null)
                {
                    return propertyInfo.GetValue(objectInstance);
                }
            }
            return null;
        }

        public string GetTableName(Type type)
        {
            TableDefinition MyAttribute =
          (TableDefinition)Attribute.GetCustomAttribute(type, typeof(TableDefinition));

            if (MyAttribute != null)
            {
                return MyAttribute.ColumnTitle;
            }
            return null;
        }

        public string GetTableName()
        {
            var t = typeof(T);

            var tablename = t.GetCustomAttribute(typeof(TableDefinition)) as TableDefinition;
            return tablename.ColumnTitle;
        }
      
        public string GetPkField(Type itemType)
        {
            foreach (var propertyInfo in itemType.GetProperties())
            {
                if ((propertyInfo.GetCustomAttribute(typeof(PKRelationshipAttribute)) as PKRelationshipAttribute) != null)
                {
                    var pkAttribute = propertyInfo.GetCustomAttribute(typeof(PKRelationshipAttribute)) as PKRelationshipAttribute;

                    return pkAttribute.ColumnTitle;
                }
            }
            return null;
        }

        public string GetPkField()
        {
            var itemType = typeof(T);
            foreach (var propertyInfo in itemType.GetProperties())
            {
                if ((propertyInfo.GetCustomAttribute(typeof(PKRelationshipAttribute)) as PKRelationshipAttribute) != null)
                {
                    var pkAttribute = propertyInfo.GetCustomAttribute(typeof(PKRelationshipAttribute)) as PKRelationshipAttribute;

                    return pkAttribute.ColumnTitle;
                }
            }
            return null;
        }

        public int GetObjectId(object objectInstance)
        {
            var itemType = objectInstance.GetType();
            foreach (var propertyInfo in itemType.GetProperties())
            {
                if ((propertyInfo.GetCustomAttribute(typeof(PKRelationshipAttribute)) as PKRelationshipAttribute) != null)
                {
                    var propertyId = propertyInfo.GetValue(objectInstance);
                    return Convert.ToInt32(propertyId);
                }
            }
            return 0;
        }
    }
}
