using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Great.Utils
{
    public static class CurrencyCodeMapper
    {
        private static readonly Dictionary<string, string> SymbolsByCode;

        public static string GetSymbol(string code)
        {
            if (!string.IsNullOrEmpty(code) && SymbolsByCode.ContainsKey(code))
                return SymbolsByCode[code];
            else
                return string.Empty;
        }

        static CurrencyCodeMapper()
        {
            SymbolsByCode = new Dictionary<string, string>();

            var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                          .Select(x => new RegionInfo(x.LCID));

            foreach (var region in regions)
                if (!SymbolsByCode.ContainsKey(region.ISOCurrencySymbol))
                    SymbolsByCode.Add(region.ISOCurrencySymbol, region.CurrencySymbol);
        }
    }
}
