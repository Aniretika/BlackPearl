using MyAttriubutes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Repository.Interfaces.Mapping
{
    //Checks and generate join relation
    public class DataSourceTransormation<T> where T : class
    {
        public DataSourceTransormation() { }
        public Dictionary<string, object> GetDataForInsertQuery(object objectInstance)
        {
            var dataQuery = new Dictionary<string, object>();
            string dataQueryDictionaryKey = "";

            foreach (var propertyInfo in objectInstance.GetType().GetProperties())
            {
                if ((propertyInfo.GetCustomAttribute(typeof(ColumnDefinition)) as ColumnDefinition) != null)
                {
                    var column = propertyInfo.GetCustomAttribute(typeof(ColumnDefinition)) as ColumnDefinition;
                    dataQueryDictionaryKey = column.ColumnTitle.ToString();
                    //сравнивать type
                    switch (propertyInfo.PropertyType.Name.ToString())
                    {
                        case "String" or "Boolean":
                            dataQuery.Add(dataQueryDictionaryKey, $"'{propertyInfo.GetValue(objectInstance).ToString()}'");
                            break;
                        case "DateTime":
                            var dateTime = (DateTime)propertyInfo.GetValue(objectInstance);
                            dataQuery.Add(dataQueryDictionaryKey, $"'{dateTime.ToString("yyyy-MM-dd")}'");
                            break;
                        default:
                            dataQuery.Add(dataQueryDictionaryKey, propertyInfo.GetValue(objectInstance).ToString());
                            break;
                    }
                }
                else if (objectInstance.GetType().BaseType.GetTypeInfo().IsAbstract)
                {
                    dataQuery.Add("Discriptor", $"'{objectInstance.GetType().Name}'");
                }
            }

            return dataQuery;
        }
        private object GetObjectByFk(T item)
        {
            foreach (var propertyInfo in item.GetType().GetProperties())
            {
                if ((propertyInfo.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute) != null)
                {
                    return propertyInfo.GetValue(item);
                }
            }

            return null;
        }

        private static bool IsPropertyIsFK(PropertyInfo propertyInfo, T item)
        {
            FKRelationshipAttribute[] MyAttributes =
                              (FKRelationshipAttribute[])Attribute.GetCustomAttributes(item.GetType(), typeof(FKRelationshipAttribute));

            if (MyAttributes.Any(connectedEntityType => connectedEntityType.ForeignKeyType == propertyInfo.PropertyType))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetTableName(Type type)
        {
            var t = type;
  
            TableDefinition MyAttribute =
              (TableDefinition)Attribute.GetCustomAttribute(type, typeof(TableDefinition));
            return MyAttribute.ColumnTitle;
        }
        public string GetTableName()
        {
            var t = typeof(T);

            var tablename = t.GetCustomAttribute(typeof(TableDefinition)) as TableDefinition;
            return tablename.ColumnTitle;
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
            return "";
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
        public string GetFkField(Type joinedType)
        {
            Type itemType = typeof(T);
            foreach (var propertyInfo in itemType.GetProperties())
            {
                if ((propertyInfo.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute) != null
                    && ((propertyInfo.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute).ForeignKeyType == joinedType))
                {
                    FKRelationshipAttribute keyAttribute = propertyInfo.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute;
                    return keyAttribute.ColumnTitle;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            return "";
        }
    }
}
