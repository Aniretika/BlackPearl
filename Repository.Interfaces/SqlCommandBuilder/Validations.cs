using MyAttriubutes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryInterfaces.SqlCommandBuilder
{
    public class Validations
    {
        public Validations() { }
        public bool EntityHasForeignKey(PropertyInfo propertyInfo, Type joinedType)
        {
            FKRelationshipAttribute foreignKeyAttribute = CurrentPropertyFkAttribute(propertyInfo);
            return foreignKeyAttribute != null && (foreignKeyAttribute.ForeignKeyType == joinedType || foreignKeyAttribute.ForeignKeyType == joinedType.BaseType);
        }

        public FKRelationshipAttribute CurrentPropertyFkAttribute(PropertyInfo property)
        {
            return property.GetCustomAttribute(typeof(FKRelationshipAttribute)) as FKRelationshipAttribute;
        }

        public ColumnDefinition CurrentPropertyColumnAttribute(PropertyInfo property)
        {
            return property.GetCustomAttribute(typeof(ColumnDefinition)) as ColumnDefinition;
        }

        public PKRelationshipAttribute CurrentPropertyPkAttribute(PropertyInfo property)
        {
            return property.GetCustomAttribute(typeof(PKRelationshipAttribute)) as PKRelationshipAttribute;
        }
    }
}
