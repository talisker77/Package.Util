using System;
using System.Collections.Generic;
using System.Xml;
using NuGet;

namespace OCS.Package.Util
{
  public enum PackageType
  {
    Web,
    Program,
    Service
  }

  public class NuspecConverters<T>
  {
    private readonly List<NuspecElementConverter<T>> _innerList = new List<NuspecElementConverter<T>>();

    public void Add(NuspecElementConverter<T> item)
    {
      _innerList.Add(item);
    }

    public List<NuspecElementConverter<T>> Converters { get { return _innerList; } }
  }

  public class Converters
  {
    public readonly NuspecConverters<bool> BooleanConverters = new NuspecConverters<bool>();
    public readonly NuspecConverters<string> StringConverters = new NuspecConverters<string>();
    public readonly NuspecConverters<ManifestFile> ManifestFileConverters = new NuspecConverters<ManifestFile>();
    public readonly NuspecConverters<PackageType> PackageTypeConverters = new NuspecConverters<PackageType>();
    public Converters()
    {
      BooleanConverters.Add(new BooleanHandler
      {
        ElementName = "//nuget:metadata/nuget:isToolsOnly",
        TargetName = "IsToolsOnly"
      });
      BooleanConverters.Add(new BooleanHandler
      {
        ElementName = "//nuget:metadata/nuget:includePdb",
        TargetName = "IncludePdb"
      });

      BooleanConverters.Add(new BooleanHandler
      {
        ElementName = "//nuget:metadata/nuget:publishPackage",
        TargetName = "PublishPackage"
      });

      StringConverters.Add(new StringHandler
      {
        ElementName = "//nuget:metadata/nuget:summary",
        TargetName = "Summary"
      });

      StringConverters.Add(new StringHandler
      {
        ElementName = "//nuget:metadata/nuget:version",
        TargetName = "Version"
      });

      StringConverters.Add(new StringHandler
      {
        ElementName = "//nuget:metadata/nuget:authors",
        TargetName = "Authors"
      });


      StringConverters.Add(new StringHandler
      {
        ElementName = "//nuget:metadata/nuget:description",
        TargetName = "Description"
      });

      StringConverters.Add(new StringHandler
      {
        ElementName = "//nuget:metadata/nuget:packageSource",
        TargetName = "PackageSource"
      });

      StringConverters.Add(new StringHandler
      {
        ElementName = "//nuget:metadata/nuget:id",
        TargetName = "Id"
      });
      ManifestFileConverters.Add(new ManifestFileHandler
      {
        ElementName = "//nuget:files/nuget:file",
        TargetName = "ManifestFiles"
      });
      PackageTypeConverters.Add(new PackageTypeHandler
      {
        ElementName = "//nuget:metadata/nuget:packageType",
        TargetName = "TypeOfPackage"
      });
    }

    public static void Convert<T>(NuspecElementConverter<T> handler, XmlNode node, object obj, Type targetType)
    {
      if (node == null) return;
      var prop = targetType.GetProperty(handler.TargetName);
      prop.SetValue(obj, handler.Handler(node.InnerText));
    }
  }

  public abstract class NuspecElementConverter<TOut> : INuspecElementHandler<TOut>
  {
    public string ElementName { get; set; }
    public string TargetName { get; set; }
    public TOut ElementValue { get; set; }

    public abstract TOut Handler(object value);
  }

  public class BooleanHandler : NuspecElementConverter<bool>
  {
    public override bool Handler(object value)
    {
      ElementValue = Convert.ToBoolean(value);
      return ElementValue;
    }
  }

  public class StringHandler : NuspecElementConverter<string>
  {
    public override string Handler(object value)
    {
      ElementValue = value.ToString();
      return ElementValue;
    }
  }

  public class PackageTypeHandler : NuspecElementConverter<PackageType>
  {
    private readonly Dictionary<string, PackageType> _packageTypes =
      new Dictionary<string, PackageType>
      {
        {"Web", PackageType.Web},
        {"Program", PackageType.Program},
        {"Service", PackageType.Service}
      };

    public override PackageType Handler(object value)
    {
      ElementValue = _packageTypes[value.ToString()];
      return ElementValue;
    }
  }

  public class ManifestFileHandler : NuspecElementConverter<ManifestFile>
  {
    public override ManifestFile Handler(object value)
    {
      /*
      ManifestFiles.Add(new ManifestFile()
      {
        Source = node.Attributes["src"].Value,
        Exclude = node.Attributes["exclude"] != null ? node.Attributes["exclude"].Value : string.Empty,
        Target = node.Attributes["target"].Value
      });
      */

      if (!(value is XmlNode)) return null;

      var node = value as XmlNode;
      return new ManifestFile
      {
        Source = node.Attributes["src"].Value,
        Exclude = node.Attributes["exclude"] != null ? node.Attributes["exclude"].Value : string.Empty,
        Target = node.Attributes["target"].Value
      };
    }
  }
}


