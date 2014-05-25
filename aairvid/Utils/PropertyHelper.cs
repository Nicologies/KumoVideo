using System;
using System.Linq.Expressions;

namespace aairvid
{
    public static class PropertyHelper
    {
        public static string GetName<TObject, TProperty>(Expression<Func<TObject, TProperty>> property)
        {
            string propertyName = ((MemberExpression)property.Body).Member.Name;
            return propertyName;
        }
    }
}
