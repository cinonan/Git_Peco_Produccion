using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CEAM.AzureSearch.WebApp.Helpers
{
    public static class StringHelper
    {
        public static string Separator = "•";
        public static string RemoveDiacritics(string text)
        {
            //text = Regex.Replace(text, @"[^0-9a-zA-ZñÑäÄëËïÏöÖüÜáéíóúáéíóúÁÉÍÓÚÂÊÎÔÛâêîôûàèìòùÀÈÌÒÙ_ ]+", "").Trim();
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

            for(int i = 0; i < wordList.Count; i++)
            {
                if (wordList[i].IndexOf("-") > -1)
                    wordList[i] = "\"" + wordList[i] + "\"";
            }

            //wordList.ForEach(word => { 
            //    if (word.IndexOf("-") > -1) 
            //        word = "\"" + word + "\""; 
            //});

            return string.Join(" ", wordList);
        }
    }
}
