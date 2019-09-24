using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatementReader.Mcc
{
    public class Mcc
    {
        public static Dictionary<int, string> VisaCodes { get; }
        public static Dictionary<int, string> IrsUsdaMccCodes { get; }
        static Mcc()
        {
            var visaMccs = JsonConvert.DeserializeObject<List<MccItem>>(System.IO.File.ReadAllText("Static/VisaMerchantCategoryCodes.json"));
            var irsUsdaMccs = JsonConvert.DeserializeObject<List<IrsUsdaMccItem>>(System.IO.File.ReadAllText("Static/MerchantCategoryCodes.json"));
            VisaCodes = visaMccs.ToDictionary(x => x.Code, x => x.Description);
            IrsUsdaMccCodes = irsUsdaMccs.ToDictionary(x => x.Code, x => x.Description);
            //var groupedMccs = visaMccs.GroupBy(x => x.Code).OrderByDescending(x => x.Count());
            //var cleanedMccs = new List<MccItem>();
            //foreach (var item in groupedMccs)
            //{
            //    if (item.Count() > 1)
            //    {
            //        cleanedMccs.Add(new MccItem { Code = item.Key, Description = item.Select(x => x.Description).OrderByDescending(x => x.Length).First() });
            //    }
            //    else
            //    {
            //        cleanedMccs.Add(item.First());
            //    }
            //}
            //System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(cleanedMccs.OrderBy(x => x.Code)));
        }
    }
}
