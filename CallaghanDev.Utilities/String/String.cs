using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CallaghanDev.Utilities.String
{
    public class Helpers
    {
        #region WordWrap

        private const string _newline = "\r\n";

        public string WordWrap(string the_string, int width = 75)
        {
            if (the_string == null)
            {
                return "";
            }
            int pos, next;
            StringBuilder sb = new StringBuilder();

            // Lucidity check
            if (width < 1)
                return the_string;

            // Parse each line of text
            for (pos = 0; pos < the_string.Length; pos = next)
            {
                // Find end of line
                int eol = the_string.IndexOf(_newline, pos);

                if (eol == -1)
                    next = eol = the_string.Length;
                else
                    next = eol + _newline.Length;

                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                {
                    do
                    {
                        int len = eol - pos;

                        if (len > width)
                            len = BreakLine(the_string, pos, width);

                        sb.Append(the_string, pos, len);
                        sb.Append(_newline);

                        // Trim whitespace following break
                        pos += len;

                        while (pos < eol && Char.IsWhiteSpace(the_string[pos]))
                            pos++;

                    } while (eol > pos);
                }
                else sb.Append(_newline); // Empty line
            }

            return sb.ToString();
        }

        private int BreakLine(string text, int pos, int max)
        {
            // Find last whitespace in line
            int i = max - 1;
            while (i >= 0 && !Char.IsWhiteSpace(text[pos + i]))
                i--;
            if (i < 0)
                return max; // No whitespace found; break at maximum length
                            // Find start of whitespace
            while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
                i--;
            // Return length of text before whitespace
            return i + 1;
        }

        #endregion

        public string BulletList(List<string> StringList, string AdditionalText = "") //This returns a bullet point list of every string within a list
        {
            string OutPutStr = "";
            int EventCount = 0;

            foreach (string ListedStr in StringList)
            {
                if (EventCount == StringList.Count)

                {
                    OutPutStr = OutPutStr + "\u2022" + ListedStr + AdditionalText;
                }
                else
                {
                    OutPutStr = OutPutStr + "\u2022" + ListedStr + AdditionalText + "\n";
                }
                EventCount++;
            }

            return OutPutStr;
        }

        public string GetNumbers(string input)
        {
            string RtnString = new string(input.Where(c => char.IsDigit(c)).ToArray());

            if (RtnString != "")
            {
                return RtnString;

            }
            else
            {
                return "0";
            }

        }
        public string GetNumbersDouble(string input)
        {
            char[] chars = input.Where(c => char.IsDigit(c) || c == '.').ToArray();

            string RtnString = "";

            for (int i = 0; i < chars.Length; i++)
            {
                if (char.IsDigit(chars[i]) == true || chars[i] == '.' && i != 0 && RtnString.Contains('.') == false)
                {
                    RtnString = RtnString + chars[i];
                }
            }

            if (RtnString.Length > 0)
            {
                if (RtnString != "" && Char.IsDigit(RtnString.ToArray()[0]))
                {
                    return RtnString;
                }
            }

            return "0";
        }

    }

}