namespace QMap
{
    public interface IPropertyCollection
    {
        object GetAttribute(string name);
        T GetAttribute<T>(string name);
        void SetValue(string name, object val);
        bool HasProperty(string name);
    }
}
