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

namespace Repository.Interfaces.SqlCommandBuilder
{
    public class SqlCommandBuilder<T> where T : class
    {
        private DataSourceTransormation<T> dataSource = new DataSourceTransormation<T>();

        //rewrite insert!!!!!!! + method ForeignKeysChecker 
        public string Insert(T item)
        {
            var insertionQueryPreparer = dataSource.GetDataForInsertQuery(item);
            string stringQuery = $"SELECT {dataSource.GetTableName(item.GetType())} ({ string.Join(", ", insertionQueryPreparer.Keys)}) " +
                $"VALUES ({ string.Join(", ", insertionQueryPreparer.Values)})";

            return stringQuery;
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
            string stringQuery = $"SELECT * FROM {dataSource.GetTableName()} WHERE {dataSource.GetPkField()} IN {id}";

            return stringQuery;
        }

        public string Remove(int id)
        {
            string stringQuery = $"DELETE FROM {dataSource.GetTableName()} WHERE {dataSource.GetPkField()} = {id}";

            return stringQuery;
        }

        public string Update(T item)
        {
            string updateQueryPreparer = "";
            foreach (var insertionQueryPreparer in dataSource.GetDataForInsertQuery(item))
            {
                updateQueryPreparer += $"({string.Join(",", insertionQueryPreparer.Key)} = {insertionQueryPreparer.Value}";
            }
           //throw new Exception("Insert Error: 
           // No Data Source was provided in the " + dataObject.GetType().Name + 
           //". Kindly review the class definition or the data mapper definition.");
            string stringQuery = $"UPDATE {dataSource.GetTableName()} " +
                $"SET {updateQueryPreparer}" +
                $"WHERE {dataSource.GetPkField()} = {dataSource.GetObjectId(item)}";
            return stringQuery;
        }

        private bool JoinRelationChecker(T item, Type joinedType)
        {
            var properties = item.GetType().GetProperties();
            MyForeignKeyAttribute keyAttribute = new MyForeignKeyAttribute(joinedType);
            foreach (var propertyInfo in properties)
            {
                keyAttribute = propertyInfo.GetCustomAttribute(typeof(MyForeignKeyAttribute)) as MyForeignKeyAttribute;

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
