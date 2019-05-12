using System.Reflection;
using DomoTroller2.ESFramework.Common.Interfaces;

namespace DomoTroller2.ESFramework.Common.Base
{
    public class Property : IProperty
    {
        internal PropertyInfo PropertyInfo { get; set; }

        string IProperty.Name
        {
            get
            {
                return PropertyInfo.Name;
            }
        }

        object IProperty.GetValue(object obj, object[] index)
        {
            return PropertyInfo.GetValue(obj, index);
        }

        void IProperty.SetValue(object obj, object val, object[] index)
        {
            PropertyInfo.SetValue(obj, val, index);
        }
    }

}
