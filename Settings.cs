using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OCS.Package.Util
{
  public class Settings
  {
    [JsonProperty(PropertyName = "Id")]
    public string Id { get; set; }
    [JsonProperty(PropertyName = "PathToAssembly")]
    public string PathToAssembly { get; set; }
    [JsonProperty(PropertyName = "OutputPath")]
    public string OutputPath { get; set; }
    [JsonProperty(PropertyName = "ProjectPath")]
    public string ProjectPath { get; set; }

    public static Settings Load(string json)
    {
      json = json.Replace(@"\", @"\\");
      var t = JObject.Parse(json);
      var settings = JsonConvert.DeserializeObject<Settings>(JsonConvert.SerializeObject(t));
      return settings;
    }
  }
}
