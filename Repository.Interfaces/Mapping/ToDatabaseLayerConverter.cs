using MyAttriubutes;
using System;
using System.Data;
using System.Reflection;


namespace RepositoryInterfaces.Mapping
{
    public static class ToDatabaseLayerConverter<T>
    {
        public static T MapDataToBusinessEntity(string typeInheritanceClass = "")
        {
            Type businessEntityType = typeof(T);
            string assemblyName = businessEntityType.Assembly.GetName().Name;
            string fullNameInheritanceClass = $"{businessEntityType.Namespace.ToString()}.{typeInheritanceClass}";
            T newObject;

            if (businessEntityType.IsAbstract)
            {
                var newGeneratedObjectReference = Activator.CreateInstance(assemblyName, fullNameInheritanceClass);
                newObject = (T)newGeneratedObjectReference.Unwrap();
            }
            else
            {
                var newGeneratedObjectReference = Activator.CreateInstance(assemblyName, businessEntityType.FullName.ToString());
                newObject = (T)newGeneratedObjectReference.Unwrap();
            }
            return newObject;
        }

        public static T FillObject(object instance, IDataReader dr)
        {
            PropertyInfo[] properties = instance.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                string dbFieldName = FieldConverter(property);
                if (dbFieldName == null)
                {
                    continue;
                }
                else
                {
                    var propertyValue = dr[dbFieldName];
                    if (propertyValue != null)
                        property.SetValue(instance, propertyValue);
                }
            }
            return (T)instance;
        }

        public static string FieldConverter(PropertyInfo property)
        {
            if ((property.GetCustomAttribute(typeof(PKRelationshipAttribute)) as PKRelationshipAttribute) != null)
            {
                var pkAtrtribute = property.GetCustomAttribute(typeof(PKRelationshipAttribute)) as PKRelationshipAttribute;

                return pkAtrtribute.ColumnTitle;
            }
            else if ((property.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute) != null)
            {
                var fkAtrtribute = property.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute;

                return fkAtrtribute.ColumnTitle;
            }
            else if ((property.GetCustomAttribute(typeof(ColumnDefinition)) as ColumnDefinition) != null)
            {
                var columnAtrtribute = property.GetCustomAttribute(typeof(ColumnDefinition)) as ColumnDefinition;

                return columnAtrtribute.ColumnTitle;
            }
            return null;
        }
    }
}
