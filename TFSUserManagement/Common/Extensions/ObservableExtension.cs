using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TFSUserManagement.Common.Extensions
{
    /// <summary>
    /// Extension Class for convertiing generic list to observable collection
    /// </summary>
    public static class ObservableExtension
    {
        /// <summary>
        /// Extension method to convertiing generic list to observable collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ObservableCollection<T> ToObservableCollection<T>(this List<T> source)
        {
            ObservableCollection<T> collection = new ObservableCollection<T>();
            source.ForEach(item => collection.Add(item));
            return collection;
        }
    }
}
