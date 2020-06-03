using Newtonsoft.Json;
using System.IO;

namespace CanBusDisplay
{
    class Config
    {
        public Display[] Displays;

        public static Config Load(string path)
        {
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
        }
    }
}
