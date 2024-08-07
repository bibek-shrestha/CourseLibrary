using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers;

public static class IEnumberableExtensions
{
    public static IEnumerable<ExpandoObject> ShapeData<TSource>(this IEnumerable<TSource> source, string? fields)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        var expandedObjectList = new List<ExpandoObject>();
        var propertyInfoList = new List<PropertyInfo>();
        if (string.IsNullOrWhiteSpace(fields))
        {
            var allPropertyInfo = typeof(TSource)
                .GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            propertyInfoList.AddRange(allPropertyInfo);
        } else
        {
            var fieldList = fields.Split(',');
            foreach (var field in fieldList)
            {
                var propertyName = field.Trim();
                var propertyInfo = typeof(TSource)
                    .GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo == null)
                {
                    throw new Exception($"Property {propertyName} cannot be found on {typeof(TSource)}.");
                }
                 propertyInfoList.Add(propertyInfo);
            }
            foreach(TSource sourceObject in source)
            {
                var shapedDataObject = new ExpandoObject();
                foreach(var propertyInfo in propertyInfoList)
                {
                    var propertyValue = propertyInfo.GetValue(sourceObject);
                    ((IDictionary<string, object?>)shapedDataObject).Add(propertyInfo.Name, propertyValue);
                }
                expandedObjectList.Add(shapedDataObject);
            }
        } 
        return expandedObjectList;
    }
}
