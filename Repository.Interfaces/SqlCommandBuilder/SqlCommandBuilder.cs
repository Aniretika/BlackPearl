using GameLibrary;
using MyAttriubutes;
using Repository.Interfaces.Mapping;
using RepositoryInterfaces.SqlCommandBuilder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Repository.Interfaces.SqlCommandBuilder
{

    public class SqlCommandBuilder<T> where T : IEntityBase
    {
        private DataSourceTransormation<T> dataSource = new();
        public Validations validations = new();

        public string Insert(object itemToInsert) => InsertQueryPreparer(itemToInsert);

        public string Update(object itemToUpdate) => UpdateQueryPreparer(itemToUpdate);

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

        public string Include(T item, Type joinedType)
        {
            if (JoinRelationChecker(item, joinedType))
            {
                string stringQuery =
                   $"SELECT * FROM {dataSource.GetTableName()}" +
                   $" WHERE {dataSource.GetPkField()} = {dataSource.GetObjectId(item)} ";
                return stringQuery;
            }
            else
            {
                throw new Exception("Join relation cannot executed, becase of lack of relation keys.");
            }
        }

        private string InsertQueryPreparer(object itemToInsert)
        {
            StringBuilder stringQuery = new StringBuilder();

            stringQuery.Append(InsertWrapper(itemToInsert));
            stringQuery.Append(" SELECT SCOPE_IDENTITY() ");

            return stringQuery.ToString();
        }

        private string UpdateQueryPreparer(object itemToUpdate)
        {
            StringBuilder stringQuery = new StringBuilder();

            stringQuery.Append(UpdateWrapper(itemToUpdate));

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
                    stringQuery += OneToManyRelationDataHandler(objectInstance, relatedObject);
                }
                else if(propertyMainObjectInfo.PropertyType.CustomAttributes.Any(attr => attr.AttributeType == typeof(TableDefinition)) && !propertyMainObjectInfo.GetIndexParameters().Any())
                {
                    relatedObject = propertyMainObjectInfo.GetValue(objectInstance);
                    stringQuery += OneToOneRelationDataHandler(objectInstance, relatedObject);
                }
            }
            return stringQuery;
        }

        private string OneToOneRelationDataHandler(object principalEntity, object dependedEntity)
        {
            StringBuilder stringQuery = new();
            string fkColumnTitle = dataSource.GetFkColumnTitleDependedEntity(principalEntity,dependedEntity);
            if (fkColumnTitle == null)
            {
                fkColumnTitle = dataSource.GetFkColumnTitleDependedEntity(dependedEntity, principalEntity);
                return OneToOneRelationDataHandler(dependedEntity, principalEntity);
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

        private string OneToManyRelationDataHandler(object principalEntity, object dependedEntity)
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

        private bool JoinRelationChecker(T item, Type joinedType)
        {
            foreach (var propertyInfo in item.GetType().GetProperties())
            {
                if (validations.EntityHasForeignKey(propertyInfo, joinedType))
                {
                    return true;
                }
            }
           
            return false;
        }
      
    }
}