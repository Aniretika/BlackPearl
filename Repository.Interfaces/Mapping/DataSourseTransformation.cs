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
            string dataQueryDictionaryKey = "";

            foreach (var propertyInfo in item.GetType().GetProperties())
            {
                // need in switch case
                if ((propertyInfo.GetCustomAttribute(typeof(MyColumn)) as MyColumn) != null)
                {
                    var column = propertyInfo.GetCustomAttribute(typeof(MyColumn)) as MyColumn;
                    dataQueryDictionaryKey = column.ColumnTitle.ToString();

                    if (propertyInfo.PropertyType.Name == "String" || propertyInfo.PropertyType.Name == "Boolean")
                    {
                        dataQuery.Add(dataQueryDictionaryKey, "\"" + propertyInfo.GetValue(item).ToString() + "\"");
                    }
                    else if (propertyInfo.PropertyType.Name == "DateTime")
                    {
                        var dateTime = (DateTime)propertyInfo.GetValue(item);
                        dataQuery.Add(dataQueryDictionaryKey, "\"" + dateTime.ToString("yyyy-MM-dd") + "\"");
                    }
                    else if ((propertyInfo.GetCustomAttribute(typeof(MyForeignKeyAttribute)) as MyForeignKeyAttribute) != null)
                    {
                        var foreignKeyAttribute = propertyInfo.GetCustomAttribute(typeof(MyForeignKeyAttribute)) as MyForeignKeyAttribute;
                        var typeOfJoinedTable = foreignKeyAttribute.ForeignKeyType;
                        object foreignkeyValue = "";
                        foreach (var primaryKeyInJoinedTable in typeOfJoinedTable.GetType().GetProperties())
                        {
                            if ((primaryKeyInJoinedTable.GetCustomAttribute(typeof(MyPrimaryKeyAttribute)) as MyPrimaryKeyAttribute) != null)
                            {
                                var primaryKeyOfJoinedTable= primaryKeyInJoinedTable.GetCustomAttribute(typeof(MyPrimaryKeyAttribute)) as MyPrimaryKeyAttribute;
                                foreignkeyValue = primaryKeyInJoinedTable.GetValue(typeOfJoinedTable);
                            }
                        }

                        dataQuery.Add(foreignKeyAttribute.ColumnTitle, "\"" + foreignkeyValue);
                    }
                    else
                    {
                        dataQuery.Add(dataQueryDictionaryKey, propertyInfo.GetValue(item).ToString());
                    }
                }
            }
            return dataQuery;
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
            var tablename = t.BaseType.GetCustomAttribute(typeof(MyTable)) as MyTable;
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
