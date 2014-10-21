using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;

namespace QMap.Configuration
{

    [XmlType("request")]
    public class Request
    {

        [XmlAttribute("connectionName")]
        public string ConnectionName { get; set; }
        [XmlAttribute("text")]
        public string Text { get; set; }
        [XmlAttribute("name")]
        public string RequestName { get; set; }
        [XmlAttribute("commandType")]
        public CommandType CommandType { get; set; }
        [XmlArray("parameters")]
        public List<Parameter> Parameters { get; set; }
        [XmlAttribute("timeout")]
        public int Timeout { get; set; }
    }
}
