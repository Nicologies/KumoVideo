using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

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
