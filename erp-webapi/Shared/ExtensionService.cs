using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace flexli_erp_webapi.Shared
{
        public static class ExtensionStaticService
        {
            public static TR IfNotNull<T, TR>(this T obj, Func<T, TR> func, Func<TR> ifNull) where T : class
            {
                if (obj != null)
                {
                    return func(obj);
                }

                return ifNull();
            }

            public static List<T> Clone<T>(this List<T> originalList)
            {
                List<T> newList = new List<T>();
                foreach (var item in originalList)
                {
                    newList.Add(item);
                }

                return newList;
            }
            
            public static T Clone<T>(this T originalItem)
            {
                var serialized = JsonConvert.SerializeObject(originalItem);
                return JsonConvert.DeserializeObject<T>(serialized);

           
            }
        }
    }
