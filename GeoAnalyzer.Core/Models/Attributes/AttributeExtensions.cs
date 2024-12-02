using System.Collections.Generic;
using System.Linq;

namespace GeoAnalyzer.Core.Models.Attributes
{
    public static class AttributeExtensions
    {
        public static object GetAttribute(this Dictionary<string, object> properties, string name)
        {
            return properties.TryGetValue(name, out object value) ? value : null;
        }

        public static void SetAttribute(this Dictionary<string, object> properties, string name, object value)
        {
            if (properties.ContainsKey(name))
            {
                properties[name] = value;
            }
            else
            {
                properties.Add(name, value);
            }
        }

        public static bool HasAttribute(this Dictionary<string, object> properties, string name)
        {
            return properties.ContainsKey(name);
        }

        public static IEnumerable<string> GetAttributeNames(this Dictionary<string, object> properties)
        {
            return properties.Keys.ToList();
        }
    }
} 