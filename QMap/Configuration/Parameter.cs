using System.Xml.Serialization;
using System.Data;

namespace QMap.Configuration
{
    [XmlType("parameter")]
    public class Parameter
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("dbType")]
        public DbType ParameterType { get; set; }
        [XmlAttribute("direction")]
        public ParameterDirection Direction { get; set; }
        [XmlAttribute("size")]
        public int Size { get; set; }
    }
}
