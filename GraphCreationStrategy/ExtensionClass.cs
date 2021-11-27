using System;

namespace m_sort_server.Services
{
    public static class ExtensionClass
    {
        public static TR IfNotNull<T, TR>(this T obj, Func<T, TR> func, Func<TR> ifNull) where T : class
        {
            if (obj != null)
            {
                return func(obj);
            }

            return ifNull();
        }
    }
}