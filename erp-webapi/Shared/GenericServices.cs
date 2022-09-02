using System;
using System.Collections.Generic;

namespace m_sort_server
{
    public static class GenericServices
    {
        public static T IfNotNull<U, T>(this U item, Func<U, T> lambda) where U : class
        {
            if (item == null)
            {
                return default(T);
            }

            return lambda(item);
        }
        
        public static string IfNull(this string item, Func<string, string> lambda) 
        {
            if (item != null)
            {
                return item;
            }

            return lambda(item);
        }
        
        public static dynamic IfFalse(this bool item, Func<bool, dynamic> lambda) 
        {
            if (item)
            {
                return lambda(item);
            }

            return null;
        }
        
       

    }
}

