using System;
using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers;

public static class ObjectExtensions
{
    public static ExpandoObject ShapeData<TSource>(this TSource source, string? fields)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        var shapedDataObject = new ExpandoObject();
        if (string.IsNullOrWhiteSpace(fields))
        {
            var allPropertyInfo = typeof(TSource)
                .GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            foreach(var propertyInfo in allPropertyInfo)
            {
                ((IDictionary<string, object?>)shapedDataObject).Add(propertyInfo.Name, propertyInfo.GetValue(source));
            }
            return shapedDataObject;
        } else
        {
            var requestFields = fields.Split(',');
            foreach(var field in requestFields)
            {
                var propertyName = field.Trim();
                var propertyInfo = typeof(TSource)
                    .GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo == null)
                {
                    throw new Exception($"Property {propertyName} can not be found for {typeof(TSource)}.");
                }
                ((IDictionary<string, object?>)shapedDataObject).Add(propertyInfo.Name, propertyInfo.GetValue(source));
            }
            return shapedDataObject;
        }
    }
}
