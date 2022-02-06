using MyAttriubutes;
using System;
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
        public Dictionary<string, object> GetDataForInsertQuery(T item)
        {
            var dataQuery = new Dictionary<string, object>();
            var dataQuery1 = new Dictionary<string, object>();
            string dataQueryDictionaryKey = "";

            foreach (var propertyInfo in item.GetType().GetProperties())
            {
                if ((propertyInfo.GetCustomAttribute(typeof(MyColumn)) as MyColumn) != null)
                {
                    var column = propertyInfo.GetCustomAttribute(typeof(MyColumn)) as MyColumn;
                    dataQueryDictionaryKey = column.ColumnTitle.ToString();
                    switch (propertyInfo.PropertyType.Name.ToString())
                    {
                        case "String" or "Boolean":
                            dataQuery.Add(dataQueryDictionaryKey, $"'{propertyInfo.GetValue(item).ToString()}'");
                            break;
                        case "DateTime":
                            var dateTime = (DateTime)propertyInfo.GetValue(item);
                            dataQuery.Add(dataQueryDictionaryKey, $"'{dateTime.ToString("yyyy-MM-dd")}'");
                            break;
                        default:
                            dataQuery.Add(dataQueryDictionaryKey, propertyInfo.GetValue(item).ToString());
                            break;
                    }
                }
                else if (propertyInfo.CustomAttributes == null && IsPropertyIsFK(propertyInfo, item))
                {
                    GetInsertQueryTree(item);
                }
                else if (item.GetType().BaseType.IsAbstract)
                {
                    dataQuery.Add("Discriptor", $"'{item.GetType().Name}'");
                }

                //checking if fkattribute value excists
                //propertyInfo.GetFkAttribute if true === GetInsertQueryTree? Can we make sense about not linked fk`s?
            }

            return dataQuery;
        }

        private static bool IsPropertyIsFK(PropertyInfo propertyInfo, T item)
        {
            MyForeignKeyAttribute[] MyAttributes =
                              (MyForeignKeyAttribute[])Attribute.GetCustomAttributes(item.GetType(), typeof(MyForeignKeyAttribute));

            if (MyAttributes.Any(connectedEntityType => connectedEntityType.ForeignKeyType == propertyInfo.PropertyType))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetInsertQueryTree(T item)
        {
            var dataQuery = new Dictionary<string, object>();
            string dataQueryDictionaryKey = "";

            dataQuery = GetDataForInsertQuery(item);
         
            foreach (var attributeData in item.GetType().CustomAttributes)
            {
                if (attributeData.AttributeType == typeof(MyForeignKeyAttribute))
                {
                    MyForeignKeyAttribute[] MyAttributes =
                    (MyForeignKeyAttribute[])Attribute.GetCustomAttributes(item.GetType(), typeof(MyForeignKeyAttribute));

                    for (int i = 0; i < MyAttributes.Length; i++)
                    {
                        Type includedType = MyAttributes[i].ForeignKeyType;

                    }
                }
            }
            return dataQueryDictionaryKey;
        }

        public string GetTableName(Type type)
        {
            MyTable MyAttribute =
              (MyTable)Attribute.GetCustomAttribute(type, typeof(MyTable));
            return MyAttribute.ColumnTitle;
        }
        public string GetTableName()
        {
            var t = typeof(T);

            var tablename = t.GetCustomAttribute(typeof(MyTable)) as MyTable;
            return tablename.ColumnTitle;
        }
      
        public string GetPkField()
        {
            var itemType = typeof(T);
            foreach (var propertyInfo in itemType.GetProperties())
            {
                if ((propertyInfo.GetCustomAttribute(typeof(MyPrimaryKeyAttribute)) as MyPrimaryKeyAttribute) != null)
                {
                    var pkAttribute = propertyInfo.GetCustomAttribute(typeof(MyPrimaryKeyAttribute)) as MyPrimaryKeyAttribute;

                    return pkAttribute.ColumnTitle;
                }
            }
            return "";
        }
        public int GetObjectId(T item)
        {
            var itemType = item.GetType();
            foreach (var propertyInfo in itemType.GetProperties())
            {
                if ((propertyInfo.GetCustomAttribute(typeof(MyPrimaryKeyAttribute)) as MyPrimaryKeyAttribute) != null)
                {
                    var propertyId = propertyInfo.GetValue(item);
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
                if ((propertyInfo.GetCustomAttribute(typeof(MyForeignKeyAttribute)) as MyForeignKeyAttribute) != null
                    && ((propertyInfo.GetCustomAttribute(typeof(MyForeignKeyAttribute)) as MyForeignKeyAttribute).ForeignKeyType == joinedType))
                {
                    MyForeignKeyAttribute keyAttribute = propertyInfo.GetCustomAttribute(typeof(MyForeignKeyAttribute)) as MyForeignKeyAttribute;
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
