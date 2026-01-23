using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;

namespace Bookstore.Web.Helpers
{
    public static class MvcHelpers
    {
        public static IEnumerable<SelectListItem> GetSelectListForEnum<T>(this IHtmlHelper html, string emptyItem = null)
            where T : Enum
        {
            if (!string.IsNullOrEmpty(emptyItem))
            {
                yield return new SelectListItem()
                {
                    Text = emptyItem
                };
            }
            foreach (var val in Enum.GetValues(typeof(T)))
            {
                yield return new SelectListItem()
                {
                    Text = Enum.GetName(typeof(T), val),
                    Value = val.ToString()
                };
            }

        }

        public static IHtmlContent EnumDropDownListFor<TModel, TEnum>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TEnum>> expression,
            string optionLabel,
            object htmlAttributes)
        {
            var enumType = typeof(TEnum);
            var isNullable = Nullable.GetUnderlyingType(enumType) != null;
            var actualEnumType = isNullable ? Nullable.GetUnderlyingType(enumType) : enumType;

            if (!actualEnumType.IsEnum)
            {
                throw new ArgumentException("TEnum must be an enum type");
            }

            var selectList = Enum.GetValues(actualEnumType)
                .Cast<object>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = Convert.ToInt32(e).ToString()
                });

            return htmlHelper.DropDownListFor(expression, selectList, optionLabel, htmlAttributes);
        }
    }
}