using System;
using System.Linq;
using System.Reflection;

namespace MyAttriubutes
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PKRelationshipAttribute : Attribute
    {
        public string ColumnTitle { get; set; }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class FKRelationshipAttribute : Attribute
    {
        public string ColumnTitle { get; set; }
        public Type ForeignKeyType { get; set; }
        public FKRelationshipAttribute(Type type)
        {
            this.ForeignKeyType = type;
        }
    }

    /// <summary>
    /// Using for properties for example:
    /// [Table(ColumnTitle = "product")]
    /// public class Product
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ColumnDefinition : Attribute
    {
        public string ColumnTitle { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = true)]
    public class TableDefinition : Attribute
    {
        public string ColumnTitle { get; set; }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class NavigationProperty : Attribute
    {
    }
}

