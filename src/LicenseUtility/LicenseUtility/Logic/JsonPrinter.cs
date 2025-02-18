using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NugetUtility.Logic
{
    internal static class JsonPrinter
    {
        public static void PrintToJson(Dictionary<string, Package> packages, string outputPath)
        {
            var nuget = packages.Values.ToList();
            var json = JsonConvert.SerializeObject(nuget, mJsonSerializerSettings);
            File.WriteAllText(Path.Combine(outputPath, "packages.json"), json);
        }

        private static readonly JsonSerializerSettings mJsonSerializerSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };
    }
}
