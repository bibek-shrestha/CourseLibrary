using System;
using System.Reflection;

namespace CourseLibrary.API.Services;

public class PropertyCheckerService: IPropertyCheckerService
{
    public bool TypeHasProperties<T>(string? fields)
    {
        if(string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }
        var allFields = fields.Split(',');
        foreach(var field in allFields)
        {
            var propertyName = field.Trim();
            var propertyInfo = typeof(T)
                .GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null)
            {
                return false;
            }
        }
        return true;
    }
}
