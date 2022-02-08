using System;
using Repository.Interfaces.Mapping;
using System.Data.Linq;
using GameLibrary;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using MyAttriubutes;
using System.Linq.Expressions;
using System.Collections;

namespace Repository.Interfaces.SqlCommandBuilder
{
    public class SqlCommandBuilder<T> where T : class
    {
        private DataSourceTransormation<T> dataSource = new DataSourceTransormation<T>();

        public string Insert(object item) => QueryPreparer(InsertWrapper, item);
        public string Update(object item) => QueryPreparer(UpdateWrapper, item);

        private string QueryPreparer(Func<object, string> queryWrapper, object obj)//func
        {
            StringBuilder stringQuery = new StringBuilder();
            stringQuery.Append(queryWrapper(obj));
            stringQuery.Append(GetNestedObjects(queryWrapper, obj));

            return stringQuery.ToString();
        }

        private string GetNestedObjects(Func<object, string> queryWrapper, object obj)
        {
            StringBuilder stringQuery = new StringBuilder();
            object nestedObject = GetObjectByFk(obj);
            if (nestedObject != null)
            {
                if ((nestedObject.GetType() is IEnumerable))
                {
                    var collectionOfNestedObject = nestedObject as IEnumerable;
                    foreach (var nestedObjectInstance in collectionOfNestedObject)
                    {
                        stringQuery.Append(queryWrapper(nestedObjectInstance));
                        if (GetObjectByFk(nestedObjectInstance) != null)
                        {
                            stringQuery.Append(GetNestedObjects(queryWrapper, nestedObjectInstance));
                        }
                    }
                }
                else if (nestedObject.GetType().IsArray)
                {
                    var array = (IEnumerable)nestedObject;
                    foreach (var nestedObjectExemplar in array)
                    {
                        stringQuery.Append(queryWrapper(nestedObjectExemplar));

                        if (GetObjectByFk(nestedObjectExemplar) != null)
                        {
                            stringQuery.Append(GetNestedObjects(queryWrapper, nestedObjectExemplar));
                        }
                    }
                }
                else
                {
                    stringQuery.Append(queryWrapper(nestedObject));
                }
            }
            return stringQuery.ToString();
        }
        private string InsertWrapper(object insertingInstance)
        {
            var queryPreparer = dataSource.GetDataForInsertQuery(insertingInstance);

            string stringQuery = $"INSERT INTO {dataSource.GetTableName(insertingInstance.GetType())} ({ string.Join(", ", queryPreparer.Keys)}) " +
    $"VALUES ({ string.Join(", ", queryPreparer.Values)}) ";
            return stringQuery;
        }

        private string UpdateWrapper(object updatingInstance)
        {
            var queryPreparer = dataSource.GetDataForInsertQuery(updatingInstance);
            string updateQueryPreparer = "";
            foreach (var insertionQueryPreparer in dataSource.GetDataForInsertQuery(updatingInstance))
            {
                updateQueryPreparer += $"({string.Join(",", insertionQueryPreparer.Key)} = {insertionQueryPreparer.Value}";
            }
            //throw new Exception("Insert Error: 
            // No Data Source was provided in the " + dataObject.GetType().Name + 
            //". Kindly review the class definition or the data mapper definition.");
            string stringQuery = $"UPDATE {dataSource.GetTableName()} " +
                $"SET {updateQueryPreparer}" +
                $"WHERE {dataSource.GetPkField()} = {dataSource.GetObjectId(queryPreparer)}";
            return stringQuery;
        }
        private object GetObjectByFk(object item)
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

        public string Include(T item, Type joinedType)
        {
            if(JoinRelationChecker(item, joinedType))
            {
                string itemJoinedTable = dataSource.GetTableName() + "_joined";
                string joinedTypeTable = dataSource.GetTableName(joinedType) + "_joined";
                string foreignKeyJoinedTable = dataSource.GetFkField(joinedType);
                string primaryKeyMainTable = dataSource.GetPkField();
                string stringQuery =
                    $"SELECT * FROM {dataSource.GetTableName()} AS {itemJoinedTable} " +
                    $"JOIN {dataSource.GetTableName(joinedType)} AS {joinedTypeTable} " +
                    $"ON {itemJoinedTable}.{primaryKeyMainTable} = {joinedTypeTable}.{foreignKeyJoinedTable} " +
                    $"WHERE {itemJoinedTable}.{primaryKeyMainTable} = {dataSource.GetObjectId(item)}";
                return stringQuery;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public string FindById(int id)
        {
            string stringQuery = $"SELECT * FROM {dataSource.GetTableName()} WHERE {dataSource.GetPkField()} = {id}";

            return stringQuery;
        }

        public string Remove(int id)
        {
            string stringQuery = $"DELETE FROM {dataSource.GetTableName()} WHERE {dataSource.GetPkField()} = {id}";

            return stringQuery;
        }

        private bool JoinRelationChecker(T item, Type joinedType)
        {
            var properties = item.GetType().GetProperties();
            FKRelationshipAttribute keyAttribute = new FKRelationshipAttribute(joinedType);
            foreach (var propertyInfo in properties)
            {
                keyAttribute = propertyInfo.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute;

            }
            return keyAttribute.ForeignKeyType == joinedType;
        }


        //public bool JoinRelationChecker(T item)
        //{
        //    Type type = item.GetType();
        //    var fkMainTableChecker = type.GetProperties().Where(
        //        prop => Attribute.IsDefined(prop, typeof(MyForeignKeyAttribute)));

        //    return fkMainTableChecker != null;
        //}

    }
}
