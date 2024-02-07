using System.Text.RegularExpressions;

namespace CallaghanDev.Utilities
{
    public static class DataValidation
    {           
        /// <summary>
                 /// This checks if the object is either null or a blank string, ie inval == "" or inval == null
                 /// </summary>
                 /// <param name="inval">Object you are checking is empty or null</param>
                 /// <returns></returns>
        public static bool StringIsEmpyOrNull(object inval)
        {
            if (inval != null)
            {
                if (Convert.ToString(inval) == "")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }

        }
        public static bool IsNumeric(object inValue)
        {
            bool bValid = false;
            try
            {
                double myDT = Convert.ToDouble(inValue.ToString());
                bValid = true;
            }
            catch (FormatException)
            {
                bValid = false;
            }

            return bValid;
        }
        public static bool IsNumeric(string value)
        {
            return value.All(char.IsNumber);
        }
        public static bool OnlyLetters(string value, bool ExcludeSpace = false)
        {
            //

            if (ExcludeSpace)
            {
                return Regex.IsMatch(value, @" ^[a-zA-Z]+$");
            }
            else
            {
                return Regex.IsMatch(value, @"^[A-Za-z ]+$");

            }


        }
        public static bool OnlyLettersAndNumbers(string value)
        {
            return Regex.IsMatch(value, @"^[a-zA-Z0-9]+$"); ;
        }
        public static bool OnlyNumbers(string value)
        {
            return Regex.IsMatch(value, @"^[0-9]([.,][0-9]{1,3})?$"); ;
        }
        public static bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }

    }
}
