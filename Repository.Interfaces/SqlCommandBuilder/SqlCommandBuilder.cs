﻿using System;
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

            stringQuery.Append(UpdateWrapper(obj));

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

            string updateSetContainer = string.Join(", ",
                queryPreparer.Zip(queryPreparer, (tableField, tableData) => tableField.Key + " = " + tableData.Value));
            var instanceType = updatingInstance.GetType();

            string stringQuery = $"UPDATE {dataSource.GetTableName(instanceType)} " +
                $"SET {updateSetContainer} " +
                $"WHERE {dataSource.GetPkField(instanceType)} = {dataSource.GetObjectId(updatingInstance)} ";
            stringQuery += UpdateQueryDependedEntity(updatingInstance);

            return stringQuery;
        }

        private string UpdateQueryDependedEntity(object objectInstance)
        {
            object relatedObject;
            string stringQuery = "";
            foreach (var propertyMainObjectInfo in objectInstance.GetType().GetProperties())
            {
                Type oneToManyCaseTypeObject = propertyMainObjectInfo.PropertyType.GetElementType();
                var t = propertyMainObjectInfo.PropertyType.GetElementType();
                if (oneToManyCaseTypeObject != null && oneToManyCaseTypeObject.CustomAttributes.Any(attr => attr.AttributeType == typeof(TableDefinition)))
                {
                    relatedObject = propertyMainObjectInfo.GetValue(objectInstance);
                    stringQuery += OneToManyFkQueryData(objectInstance, relatedObject);
                }
                else if(propertyMainObjectInfo.PropertyType.CustomAttributes.Any(attr => attr.AttributeType == typeof(TableDefinition)) && !propertyMainObjectInfo.GetIndexParameters().Any())
                {
                    relatedObject = propertyMainObjectInfo.GetValue(objectInstance);
                    stringQuery += OneToOneFkQueryData(objectInstance, relatedObject);
                }
            }
            return stringQuery;
        }

        private string OneToOneFkQueryData(object principalEntity, object dependedEntity)
        {
            StringBuilder stringQuery = new();
            string fkColumnTitle = dataSource.GetFkColumnTitleDependedEntity(principalEntity,dependedEntity);
            if(fkColumnTitle==null)
            {
                fkColumnTitle = dataSource.GetFkColumnTitleDependedEntity(dependedEntity, principalEntity);
                return OneToOneFkQueryData(dependedEntity, principalEntity);
            }
            var pkPrincipalEntity = dataSource.GetPrimaryKeyValue(principalEntity);
            string tableName = dataSource.GetTableName(dependedEntity.GetType());

            Dictionary<string, object> queryPreparer = dataSource.GetDataForInsertQuery(dependedEntity);
            queryPreparer.Add(fkColumnTitle, pkPrincipalEntity);

            string setQueryContainer = string.Join(", ",
 queryPreparer.Zip(queryPreparer, (tableField, tableData) => tableField.Key + " = " + tableData.Value));


            stringQuery.Append($"UPDATE {tableName} " +
            $"SET {setQueryContainer} " +
            $"WHERE {dataSource.GetPkField(dependedEntity.GetType())} = {dataSource.GetObjectId(dependedEntity)} ");
            return stringQuery.ToString();
        }

        private string OneToManyFkQueryData(object principalEntity, object dependedEntity)
        {
            StringBuilder stringQuery = new();
            if (dependedEntity.GetType().IsArray || dependedEntity.GetType() is IEnumerable)
            {
                var array = dependedEntity as IEnumerable;

                foreach (var arrayItem in array)
                {
                    string fkColumnTitle = dataSource.GetFkColumnTitleDependedEntity(principalEntity, arrayItem);
                    var pkPrincipalEntity = dataSource.GetPrimaryKeyValue(principalEntity);
                    string tableName = dataSource.GetTableName(arrayItem.GetType());

                    Dictionary<string, object> queryPreparer = dataSource.GetDataForInsertQuery(arrayItem);
                    queryPreparer.Add(fkColumnTitle, pkPrincipalEntity);

                    string setQueryContainer = string.Join(", ",
         queryPreparer.Zip(queryPreparer, (tableField, tableData) => tableField.Key + " = " + tableData.Value));

                    stringQuery.Append($"UPDATE {tableName} " +
                    $"SET {setQueryContainer} " +
                    $"WHERE {dataSource.GetPkField(arrayItem.GetType())} = {dataSource.GetObjectId(arrayItem)} ");
                }
            }
            return stringQuery.ToString();
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