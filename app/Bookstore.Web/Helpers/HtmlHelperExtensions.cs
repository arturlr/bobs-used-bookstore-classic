using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookstore.Web.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlContent EnumDropDownListFor<TModel, TEnum>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TEnum>> expression,
            string optionLabel,
            object htmlAttributes)
        {
            var enumType = typeof(TEnum);
            var underlyingType = Nullable.GetUnderlyingType(enumType) ?? enumType;
            
            if (!underlyingType.IsEnum)
            {
                throw new ArgumentException("TEnum must be an enumerated type");
            }

            var selectList = Enum.GetValues(underlyingType)
                .Cast<object>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = e.ToString()
                });

            return htmlHelper.DropDownListFor(expression, selectList, optionLabel, htmlAttributes);
        }
    }
}
