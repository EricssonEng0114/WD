using System.Text.RegularExpressions;

namespace UBPC.Web.Common
{
    public static class CommonFunction
    {
        public static string formatHTMLTag(string input)
        {
            System.Text.StringBuilder output = new System.Text.StringBuilder();
            string[] inputs;

            if (input.IndexOf('<') != -1)
            {
                inputs = input.Split('<');

                foreach (string text in inputs)
                {
                    output.Append(text.Trim());
                    output.Append(" < ");
                }

                output.Remove(output.Length - 3, 3);
            }

            if (output.Length > 0)
            {
                input = output.ToString();
                output.Clear();
            }

            if (input.IndexOf('>') != -1)
            {
                inputs = input.Split('>');

                foreach (string text in inputs)
                {
                    output.Append(text.Trim());
                    output.Append(" > ");
                }

                output.Remove(output.Length - 3, 3);
            }

            if (output.Length == 0)
                output.Append(input);

            return output.ToString();
        }

        public static string ToLowerCase(string input)
        {
            return input.ToLower(System.Globalization.CultureInfo.CurrentCulture);

        }

        public static string formatText(string input)
        {
            if (input == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(input.Trim()))
            {
                return string.Empty;
            }
            else
            {
                return input.Trim();
            }
        }

        public static bool isAlphanumeric(string input)
        {
            bool isValid = true;

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasLowerChar = new Regex(@"[a-z]+");
            bool num = false;
            bool UpperChar = false;
            bool LowerChar = false;


            if (hasNumber.IsMatch(input))
            {
                num = true;
            }

            if (hasUpperChar.IsMatch(input))
            {
                UpperChar = true;
            }

            if (hasLowerChar.IsMatch(input))
            {
                LowerChar = true;
            }



            if (!(num || (UpperChar || LowerChar)))
            {
                isValid = false;
            }


            return isValid;
        }

        public static bool isNumberOnly(string input)
        {
            bool isValid = true;
            bool num = false;

            var hasNumber = new Regex(@"[0-9]+");
            if (hasNumber.IsMatch(input))
            {
                num = true;
            }

            isValid = num;

            return isValid;
        }


    }
}
