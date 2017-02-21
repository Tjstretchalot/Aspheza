using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Logic
{
    public class LogicUtils
    {
        /// <summary>
        /// Inserts the element into the array using the specified comparison function such that the list
        /// is still sorted such that:
        /// 
        /// comparer(arr[0], arr[1]) &lt;= 0
        /// comparer(arr[1], arr[2]) &lt;= 0
        /// comparer(arr[2], arr[3]) &lt;= 0
        /// 
        /// etc
        /// 
        /// If the comparer was i1 - i2 for the type int, the result would be in ascending order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="element"></param>
        /// <param name="comparer"></param>
        public static void BinaryInsert<T>(List<T> arr, T element, Func<T, T, int> comparer)
        {
            if(arr.Count == 0)
            {
                arr.Add(element);
                return;
            }

            int start = 0;
            int end = arr.Count - 1;
            int mid = (end + start) / 2;

            while (start < end)
            {
                var val = arr[mid];

                var compar = comparer(val, element);

                if (compar == 0)
                {
                    start = end = mid;
                    break;
                }
                else if (compar < 0)
                {
                    start = mid + 1;
                }
                else
                {
                    end = mid;
                }

                mid = (end + start) / 2;
            }

            if (start == arr.Count - 1)
            { 
                var compar = comparer(arr[start], element);
                
                if(compar < 0)
                {
                    arr.Add(element);
                    return;
                }
            }
            arr.Insert(start, element);
        }
    }
}
