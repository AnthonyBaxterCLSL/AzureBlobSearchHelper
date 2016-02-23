using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlobSearchHelperPackage
{
    public static class StringHelper
    {
        /// <summary>
        /// Converts all Diacritic characters in a string to their ASCII equivalent
        /// Courtesy: http://stackoverflow.com/a/13154805/476786
        /// A quick explanation:
        /// * Normalizing to form D splits charactes like è to an e and a nonspacing `
        /// * From this, the nospacing characters are removed
        /// * The result is normalized back to form C (I'm not sure if this is neccesary)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ConvertDiacriticToASCII(this string value)
        {
            if (value == null) return null;
            var chars =
                value.Normalize(NormalizationForm.FormD)
                     .ToCharArray()
                     .Select(c => new { c, uc = CharUnicodeInfo.GetUnicodeCategory(c) })
                     .Where(@t => @t.uc != UnicodeCategory.NonSpacingMark)
                     .Select(@t => @t.c);
            var cleanStr = new string(chars.ToArray()).Normalize(NormalizationForm.FormC);
            return cleanStr;
        }
    }
}
