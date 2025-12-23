using System.Text;

namespace Konfus.Utility.Extensions
{
    public static class StringExtensions
    {
        public static string ToPascalCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            var result = new StringBuilder(input.Length);
            var startOfWord = true;

            foreach (char c in input)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    startOfWord = true;
                    continue;
                }

                if (startOfWord)
                {
                    // Digits should not start a PascalCase word
                    if (char.IsDigit(c))
                        continue;

                    result.Append(char.ToUpperInvariant(c));
                    startOfWord = false;
                }
                else
                    result.Append(char.ToLowerInvariant(c));
            }

            return result.Length == 0 ? "" : result.ToString();
        }
    }
}