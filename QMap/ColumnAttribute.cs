using System;

namespace QMap              
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)] 
    public sealed class ColumnAttribute : Attribute
    {
        public string DbType { get; set; }
        public string Name { get; set; }
    }
}
