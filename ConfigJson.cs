using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICraftLauncher
{
    class ConfigJson
    {
        public string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icraft.json");

        public ConfigJson()
        {
            this.init();
        }

        private void init()
        {
            if(!File.Exists(this.path))
            {
                FileStream fs = new FileStream(this.path, FileMode.CreateNew);
                StreamWriter stream = new StreamWriter(fs);
                JObject jObject = new JObject();
                jObject.Add("launch_version", "");
                stream.WriteLine(jObject.ToString());
                stream.Dispose();
            }
        }

        public void setLaunchVersion(string version)
        {
            FileStream fs = new FileStream(this.path, FileMode.Create);
            StreamWriter stream = new StreamWriter(fs);
            JObject jObject = new JObject();
            jObject.Add("launch_version", version);
            stream.WriteLine(jObject.ToString());
            stream.Dispose();
        }

        public string getLaunchVersion()
        {
            StreamReader stream = new StreamReader(this.path, true);
            JObject jObject = JObject.Parse(stream.ReadToEnd());
            stream.Dispose();

            return jObject.Value<string>("launch_version");
        }
    }
}
