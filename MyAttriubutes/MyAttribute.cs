using System;
using System.Linq;
using System.Reflection;

namespace MyAttriubutes
{
  
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
        public class MyPrimaryKeyAttribute : Attribute
        {
            public string ColumnTitle { get; set; }
        }
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class MyForeignKeyAttribute : Attribute
    {
        public string ColumnTitle { get; set; }
        public Type ForeignKeyType { get; set; }
        public MyForeignKeyAttribute(Type type)
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
        public class MyColumn : Attribute
        {
            public string ColumnTitle { get; set; }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = true)]
        public class MyTable : Attribute
        {
            public string ColumnTitle { get; set; }
        }
    }

