
using System.Text;
using System.Text.RegularExpressions;

namespace ECommerceAPI.Helpers
{
    public static class SlugHelper
    {
        public static string Generate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "item";

            value = value.Trim().ToLowerInvariant();

            value = Regex.Replace(value, @"\s+", "-");

            var result = new StringBuilder();

            foreach (char character in value)
            {
                if (char.IsLetterOrDigit(character) || character == '-')
                {
                    result.Append(character);
                }
            }

            string slug = Regex.Replace(result.ToString(), @"-+", "-");

            slug = slug.Trim('-');

            return string.IsNullOrWhiteSpace(slug) ? "item" : slug;
        }
    }
}
