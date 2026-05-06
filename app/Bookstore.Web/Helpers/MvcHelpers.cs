using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookstore.Web.Helpers
{
    public static class MvcHelpers
    {
        public static IEnumerable<SelectListItem> GetSelectListForEnum<T>(string? emptyItem = null)
            where T : Enum
        {
            var items = new List<SelectListItem>();

            if (!string.IsNullOrEmpty(emptyItem))
            {
                items.Add(new SelectListItem { Text = emptyItem });
            }

            foreach (var val in Enum.GetValues(typeof(T)))
            {
                items.Add(new SelectListItem { Text = Enum.GetName(typeof(T), val) });
            }

            return items;
        }
    }
}
