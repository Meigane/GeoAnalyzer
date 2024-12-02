using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoAnalyzer.Core.Models.Attributes
{
    public class FeatureAttributes
    {
        private Dictionary<string, object> attributes;

        public FeatureAttributes()
        {
            attributes = new Dictionary<string, object>();
        }

        public void SetAttribute(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("属性名不能为空", nameof(name));
            }

            attributes[name] = value;
        }

        public object GetAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("属性名不能为空", nameof(name));
            }

            if (attributes.TryGetValue(name, out object value))
            {
                return value;
            }

            throw new KeyNotFoundException($"未找到名为 '{name}' 的属性");
        }

        public T GetAttribute<T>(string name)
        {
            object value = GetAttribute(name);
            if (value is T typedValue)
            {
                return typedValue;
            }

            throw new InvalidCastException($"无法将属性 '{name}' 转换为类型 {typeof(T).Name}");
        }

        public bool HasAttribute(string name)
        {
            return attributes.ContainsKey(name);
        }

        public void RemoveAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("属性名不能为空", nameof(name));
            }

            attributes.Remove(name);
        }

        public void ClearAttributes()
        {
            attributes.Clear();
        }

        public IEnumerable<string> GetAttributeNames()
        {
            return attributes.Keys;
        }

        public int AttributeCount => attributes.Count;

        public Dictionary<string, object> GetAllAttributes()
        {
            return new Dictionary<string, object>(attributes);
        }

        public void SetAttributes(Dictionary<string, object> newAttributes)
        {
            if (newAttributes == null)
            {
                throw new ArgumentNullException(nameof(newAttributes));
            }

            attributes = new Dictionary<string, object>(newAttributes);
        }

        public override string ToString()
        {
            return $"FeatureAttributes: {AttributeCount} attributes";
        }
    }
}
