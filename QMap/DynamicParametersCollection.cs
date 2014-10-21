using System;
using System.Linq;
using Dapper;

namespace QMap
{
    public class DynamicParametersCollection : IPropertyCollection
    {
        private DynamicParameters Parameters { get; set; }


        public DynamicParametersCollection(DynamicParameters parameters)
        {
            Parameters = parameters;
        }

        public object GetAttribute(string name)
        {
            return Parameters.Get<object>(name);
        }

        public T GetAttribute<T>(string name)
        {
            return Parameters.Get<T>(name);
        }

        public void SetValue(string name, object val)
        {
            throw new NotImplementedException();
        }


        public bool HasProperty(string name)
        {
            return Parameters.ParameterNames.FirstOrDefault(p => p == name) != null;
        }
    }
}
