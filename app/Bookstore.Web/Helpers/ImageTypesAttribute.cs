using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Bookstore.Web.Helpers
{
    public class ImageTypesAttribute : ValidationAttribute
    {
        private readonly string[] imageTypes;

        public ImageTypesAttribute(string[] imageTypes)
        {
            this.imageTypes = imageTypes;
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;

            if (!(value is IFormFile file)) return base.IsValid(value);

            var extension = Path.GetExtension(file.FileName);

            return imageTypes.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The {name} field must be one of the following types: {string.Join(", ", imageTypes)}";
        }
    }
}
