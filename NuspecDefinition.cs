using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NuGet;

namespace OCS.Package.Util
{
  public class NuspecDefinition
  {
    public bool IsToolsOnly { get; set; }
    public bool IncludePdb { get; set; }
    public bool PublishPackage { get; set; }
    public PackageType TypeOfPackage { get; set; }
    public XmlNode NuspecDefData { get; set; }
    private XmlNamespaceManager _namespaceManager;
    public string Id { get; set; }
    public string Summary { get; set; }
    public string Version { get; set; }
    public string Authors { get; set; }
    public string Description { get; set; }
    public string TargetDir { get; set; }
    public string ProjectDir { get; set; }
    public string PackageSource { get; set; }
    public readonly List<ManifestFile> ManifestFiles;// { get; set; } 
    
    private readonly Converters _converters;
    public NuspecDefinition()
    {
      ManifestFiles = new List<ManifestFile>();
      _converters = new Converters();
    }

    public NuspecDefinition(string path, string targetDir)
      : this()
    {
      NuspecDefData = LoadDefFile(path);
      TargetDir = targetDir;
      ProjectDir = Path.GetDirectoryName(path);

      if (NuspecDefData != null)
      {
        AnalyzeDefFile();
      }
    }

    private XmlNode LoadDefFile(string path)
    {
      var nuspecDefFile = path;
      if (!File.Exists(nuspecDefFile))
      {
        //no extra definition file exists => no further process needed. 
        return null;
      }
      var xdoc = new XmlDocument();
      xdoc.Load(nuspecDefFile);
      _namespaceManager = new XmlNamespaceManager(xdoc.NameTable);
      _namespaceManager.AddNamespace("nuget", "http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd");
      return xdoc.DocumentElement;
    }

    private void AnalyzeDefFile()
    {
      //setting is tools only
      var t = GetType();
      _converters.BooleanConverters.Converters.ForEach(element =>
      {
        var elm = NuspecDefData.SelectSingleNode(element.ElementName, _namespaceManager);
        Converters.Convert(element, elm, this, t);
      });


      //for remember purpose
      _converters.StringConverters.Converters.ForEach(element =>
      {
        var summary = NuspecDefData.SelectSingleNode(element.ElementName, _namespaceManager);
        Converters.Convert(element, summary, this, t);
      });

      _converters.PackageTypeConverters.Converters.ForEach(element =>
      {
        var elm = NuspecDefData.SelectSingleNode(element.ElementName, _namespaceManager);
        Converters.Convert(element, elm, this, t);
      });

      _converters.ManifestFileConverters.Converters.ForEach(element =>
      {
        var xmlNodeList = NuspecDefData.SelectNodes(element.ElementName, _namespaceManager);
        if (xmlNodeList == null) return;

        foreach (var manifestFile in from XmlNode node in xmlNodeList select element.Handler(node))
        {
          var mf = manifestFile;
          mf.Source = mf.Source.Replace("##TargetDir##", TargetDir);
          mf.Source = mf.Source.Replace("##ProjectDir##", ProjectDir);
         
          ManifestFiles.Add(mf);
        }
      });
    }
  }


}
