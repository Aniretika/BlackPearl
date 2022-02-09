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
using Microsoft.VisualBasic.CompilerServices;

namespace Repository.Interfaces.SqlCommandBuilder
{
    public static class ArrayExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this Array target)
        {
            foreach (var item in target)
                yield return (T)item;
        }
    }

    public class SqlCommandBuilder<T> where T : class
    {
        private DataSourceTransormation<T> dataSource = new DataSourceTransormation<T>();
        // Dictionary where object 1 - pk type, object 2 - pk value
        private Dictionary<object, object> RelationKeyContainer { get; set; }
        public string Insert(object item) => InsertQueryPreparer(InsertWrapper, item);
        public string Update(object item) => UpdateQueryPreparer(UpdateWrapper, item);
       
        private string InsertQueryPreparer(Func<object, string> queryWrapper, object obj)//func
        {
            StringBuilder stringQuery = new StringBuilder();

            stringQuery.Append(queryWrapper(obj));
            stringQuery.Append(" SELECT SCOPE_IDENTITY() ");


            return stringQuery.ToString();
        }

        private string UpdateQueryPreparer(Func<object, string> queryWrapper, object obj)//func
        {
            StringBuilder stringQuery = new StringBuilder();

            stringQuery.Append(queryWrapper(obj));
            stringQuery.Append(GetNestedObjects(queryWrapper, obj));

            return stringQuery.ToString();
        }
       
         

        private string GetNestedObjects(Func<object, string> queryWrapper, object obj)
        {
            StringBuilder stringQuery = new();
            object nestedObject = GetObjectByFk(obj);

            //merge two dictionaries
            RelationKeyContainer = dataSource.GetPksDataQuery(obj).Select(dict => dict)
                         .ToDictionary(pair => pair.Key, pair => pair.Value);

            if (nestedObject != null)
            {
                if (nestedObject.GetType() is IEnumerable)
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

                    foreach (var t in array)
                    {
                        //array.
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
            //merge two dictionaries
            // RelationKeyContainer = dataSource.GetPksDataQuery(obj).Select(dict => dict)
            //   .ToDictionary(pair => pair.Key, pair => pair.Value);

            
            queryPreparer = queryPreparer.Concat(GetFkValues(updatingInstance)).ToDictionary(e => e.Key, e => e.Value);

            string updateSetContainer = string.Join(", ",
                queryPreparer.Zip(queryPreparer, (tableField, tableData) => tableField.Key + " = " + tableData.Value));
            var instanceType = updatingInstance.GetType();


            //throw new Exception("Insert Error: 
            // No Data Source was provided in the " + dataObject.GetType().Name + 
            //". Kindly review the class definition or the data mapper definition.");
            string stringQuery = $"UPDATE {dataSource.GetTableName(instanceType)} " +
                $"SET {updateSetContainer} " +
                $"WHERE {dataSource.GetPkField(instanceType)} = {dataSource.GetObjectId(updatingInstance)} ";
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
        private Dictionary<string, object> GetFkValues(object nestedObjectInstance)
        {
            Dictionary<string, object> containerFks = new();

            FKRelationshipAttribute[] MyAttributes =
                   (FKRelationshipAttribute[])Attribute.GetCustomAttributes(nestedObjectInstance.GetType(), typeof(FKRelationshipAttribute));

            for (int i = 0; i < MyAttributes.Length; i++)
            {
                if (RelationKeyContainer.Any(attr => attr.Key.GetType() == MyAttributes[i].ForeignKeyType))
                {
                    containerFks.Add(MyAttributes[i].ColumnTitle, RelationKeyContainer.FirstOrDefault(fkObject => fkObject.Key.GetType() == MyAttributes[i].ForeignKeyType).Value);
                }
            }


            foreach (var propertyInfo in nestedObjectInstance.GetType().GetProperties())
            {
                if ((propertyInfo.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute) != null)
                {
                    var currentFk = propertyInfo.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute;
                    if (RelationKeyContainer!=null && RelationKeyContainer.Any(attr => attr.Key.GetType() == currentFk.ForeignKeyType))
                    {
                        containerFks.Add(currentFk.ColumnTitle, RelationKeyContainer.FirstOrDefault(fkObject => fkObject.Key.GetType() == currentFk.ForeignKeyType).Value);
                    }
                }
            }
            return containerFks;
        }

        public string Include(T item, Type joinedType)
        {
            if (JoinRelationChecker(item, joinedType))
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

        private static bool JoinRelationChecker(T item, Type joinedType)
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