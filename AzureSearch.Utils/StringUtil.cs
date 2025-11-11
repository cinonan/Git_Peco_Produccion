using System;
using System.Text;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace AzureSearch.Utils
{
    public static class StringUtil
    {
        public static string Separator = "•";
        public static string RemoveDiacritics_Publico(string text)
        {
            string formD = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char ch in formD)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
        public static string RemoveDiacritics_Estimador(string text)
        {
            text = Regex.Replace(text, @"[^0-9a-zA-ZñÑäÄëËïÏöÖüÜáéíóúáéíóúÁÉÍÓÚÂÊÎÔÛâêîôûàèìòùÀÈÌÒÙ_\-._ ]+", "").Trim();

            string formD = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char ch in formD)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            string newText = sb.ToString().Normalize(NormalizationForm.FormC);

            var wordList = newText.Split(" ")
                                 .ToList()
                                 .Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s).ToList();

            for (int i = 0; i < wordList.Count; i++)
            {
                if (wordList[i].IndexOf("-") > -1)
                    wordList[i] = "\"" + wordList[i] + "\"";
            }

            return string.Join(" ", wordList);
        }
    }
}