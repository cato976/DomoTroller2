namespace DomoTroller2.ESFramework.Common.Interfaces
{
    public interface IProperty
    {
        string Name { get; }
        object GetValue(object obj, object[] index);
        void SetValue(object obj, object val, object[] index);
    }
}
