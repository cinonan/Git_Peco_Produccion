using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Globalization;

namespace CEAM.AzureSearch.Loader.Extensions
{
    public static class StringExtensions
    {
        public static string SinTildes(this string texto) =>
            new String(
                texto.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray()
            )
            .Normalize(NormalizationForm.FormC);
    }
}
