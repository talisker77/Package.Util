using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using NuGet;

namespace OCS.Package.Util
{
  public class Creator
  {
    private readonly Assembly _assembly;
    private readonly string _outputPath;
    private readonly Settings _settings;
    private readonly bool _hasAssembly;
    private readonly Dictionary<string, string> _libTargets = new Dictionary<string, string>
    {
      {"v4.0.30319", "net45"},
    };


    public NuspecDefinition NuspecDefinition { get; set; }

    public Creator(string settingsJson)
    {
      var settings = Settings.Load(settingsJson);
      if (string.IsNullOrEmpty(settings.PathToAssembly))
      {
        throw new FileNotFoundException("no file to analyze: " + settings.PathToAssembly);
      }
      _settings = settings;

      NuspecDefinition = new NuspecDefinition(Path.Combine(settings.ProjectPath, "nuspec-def.xml"), settings.OutputPath);
      _hasAssembly = File.Exists(settings.PathToAssembly);
      if (_hasAssembly)
      {
        _assembly = Assembly.LoadFile(settings.PathToAssembly);
      }
      _outputPath = settings.OutputPath;
    }

    public void CreateSpec()
    {
      if (!Directory.Exists(_outputPath))
        Directory.CreateDirectory(_outputPath);
      var dependencies = new List<ManifestDependencySet>();
      if (File.Exists(Path.Combine(_settings.ProjectPath, "packages.config")) && !NuspecDefinition.IsToolsOnly)
      {
        dependencies = AnalyzePackagesConfig();
      }

      var spec = new Manifest();
      var meta = RetrieveMetaData(dependencies);
      LoadAdditionalMetadata(meta);
      spec.Metadata = meta;
      LoadManifestFile(spec);

      spec.Files = NuspecDefinition.ManifestFiles;
      spec.OrganizeManifest(NuspecDefinition);
      spec.Metadata.DependencySets.ToList().ForEach(ds =>
      {
        var t = ds.Dependencies.FirstOrDefault(d => d.Id == "OCS.Package.Util");
        if (t != null)
        {
          ds.Dependencies.Remove(t);
        }
      });
      BuildPackage(spec);
    }

    private void LoadAdditionalMetadata(ManifestMetadata meta)
    {
      meta.Summary = NuspecDefinition.Summary;
      if (!string.IsNullOrEmpty(NuspecDefinition.Id))
        meta.Id = NuspecDefinition.Id;

      if (!string.IsNullOrEmpty(NuspecDefinition.Version))
      {
        meta.Version = NuspecDefinition.Version;
      }

      if (!string.IsNullOrEmpty(NuspecDefinition.Authors))
      {
        meta.Authors = NuspecDefinition.Authors;
      }

      if (!string.IsNullOrEmpty(NuspecDefinition.Description))
      {
        meta.Description = NuspecDefinition.Description;
      }
    }

    private List<ManifestDependencySet> AnalyzePackagesConfig()
    {
      var xml = new XmlDocument();
      xml.Load(Path.Combine(_settings.ProjectPath, "packages.config"));
      return (from XmlNode node in xml.SelectNodes("//packages/package")
              select new ManifestDependencySet
              {
                Dependencies = new List<ManifestDependency>
                  {
                    new ManifestDependency
                    {
                      Id = node.Attributes["id"].InnerText, 
                      Version = node.Attributes["version"].InnerText
                    }
                  }
              }).ToList();
    }

    private void BuildPackage(Manifest manifest)
    {
      var builder = new PackageBuilder();
      builder.Populate(manifest.Metadata);
      builder.PopulateFiles(_outputPath, manifest.Files);
      var file = Path.Combine(_outputPath, string.Format("{0}.{1}.nupkg", builder.Id, builder.Version));
      var localRepo = new LocalPackageRepository(_outputPath, NuspecDefinition.PackageSource);

      if (File.Exists(file))
      {
        File.Delete(file);
      }

      using (var buildFile = File.Open(file, FileMode.OpenOrCreate))
      {
        builder.Save(buildFile);
        buildFile.Flush();
        buildFile.Close();
      }
      var package = localRepo.GetPackage(builder.Id, builder.Version);

      if (NuspecDefinition.PublishPackage)
      {
        localRepo.PushPackage(package);
      }
    }

    private void LoadManifestFile(Manifest manifest)
    {

      if (!_hasAssembly)
        return;

      _assembly.GetReferencedAssemblies().ToList().ForEach(references =>
      {
        if (manifest.Metadata.DependencySets.Exists(s => s.Dependencies.Exists(d => d.Id == references.Name)))
        {
          return;
        }

        Assembly ass;
        try
        {
          ass = Assembly.Load(references);
        }
        catch (Exception)
        {
          ass = Assembly.LoadFrom(Path.Combine(_settings.OutputPath, string.Format("{0}.{1}", references.Name, "dll")));
        }

        if (ass == null)
          return;
        if (CheckAssemblyAndIncludeIfReferenced(ass))
        {
          ass.GetReferencedAssemblies().ToList().ForEach(dependent =>
          {
            if (manifest.Metadata.DependencySets.Exists(s => s.Dependencies.Exists(d => d.Id == dependent.Name)))
            {
              return;
            }
            try
            {
              var depAss = Assembly.Load(dependent);
              CheckAssemblyAndIncludeIfReferenced(depAss);
            }
            catch (Exception)
            {
            }
          });
        }
      });

      CheckAssemblyAndIncludeIfReferenced(_assembly);
    }

    private bool CheckAssemblyAndIncludeIfReferenced(Assembly ass)
    {

      if (NuspecDefinition.IsToolsOnly)
        return true;
      string fileName;
      string pbdFileName;
      if (string.IsNullOrEmpty(ass.Location))
      {
        fileName = ass.GetName().Name + ".dll";
        pbdFileName = ass.GetName().Name + ".pbd";
      }
      else
      {
        var assFileInfo = new FileInfo(ass.Location);
        fileName = assFileInfo.Name;
        pbdFileName = assFileInfo.Name.Replace(assFileInfo.Extension, ".pdb");
      }
      var assemblyInBuildFolder = Path.Combine(_outputPath, fileName);
      if (!File.Exists(assemblyInBuildFolder))
        return false;

      if (CheckAssemblyFoundInPackagesFolders(fileName))
      {
        return false;
      }

      var target = _libTargets[_assembly.ImageRuntimeVersion];
      var mf = new ManifestFile
      {
        Source = assemblyInBuildFolder,
        Target = string.Format(@"lib\{0}", target)
      };
      NuspecDefinition.ManifestFiles.Add(mf);

      var pdbFullName = Path.Combine(_outputPath, pbdFileName);
      if (NuspecDefinition.IncludePdb && File.Exists(pdbFullName))
      {
        var pbd = new ManifestFile
        {
          Source = pdbFullName,
          Target = string.Format(@"lib\{0}", target)
        };
        NuspecDefinition.ManifestFiles.Add(pbd);
      }


      return true;
    }

    private bool CheckAssemblyFoundInPackagesFolders(string fileName)
    {
      var packageFolder = Path.Combine(_settings.ProjectPath, "packages");
      IEnumerable<string> searchResult;
      if (Directory.Exists(packageFolder))
      {
        searchResult = Directory.EnumerateFiles(packageFolder, fileName, SearchOption.AllDirectories);
        return searchResult.Any();
      }
      packageFolder = Path.Combine(_settings.ProjectPath, "..", "packages");
      if (!Directory.Exists(packageFolder)) return false;

      searchResult = Directory.EnumerateFiles(packageFolder, fileName, SearchOption.AllDirectories);
      return searchResult.Any();
    }

    private ManifestMetadata RetrieveMetaData(List<ManifestDependencySet> dependencySets)
    {

      if (!_hasAssembly)
      {
        return new ManifestMetadata { DependencySets = dependencySets };
      }

      var version = _assembly.GetName().Version.ToString();

      var meta = new ManifestMetadata { Version = version, DependencySets = dependencySets };
      foreach (var attr in _assembly.GetCustomAttributes())
      {
        if (attr is AssemblyDescriptionAttribute)
        {
          meta.Description = (attr as AssemblyDescriptionAttribute).Description;
          if (string.IsNullOrEmpty(meta.Description))
          {
            meta.Description = "description";
          }
        }
        if (attr is AssemblyCopyrightAttribute)
        {
          meta.Copyright = (attr as AssemblyCopyrightAttribute).Copyright;
          if (string.IsNullOrEmpty(meta.Copyright))
          {
            meta.Copyright = string.Format("{0} &copy;", DateTime.Today.Year);
          }
          var expr = new Regex(@"(\d{4})");
          meta.Copyright = expr.Replace(meta.Copyright, string.Format("{0}", DateTime.Today.Year));
        }
        if (attr is AssemblyTitleAttribute)
        {
          meta.Id = string.IsNullOrEmpty(_settings.Id) ? (attr as AssemblyTitleAttribute).Title : _settings.Id;
        }
        if (attr is AssemblyCompanyAttribute)
        {
          meta.Authors = (attr as AssemblyCompanyAttribute).Company;
        }

        if (attr is AssemblyVersionAttribute)
        {
          meta.Version = (attr as AssemblyVersionAttribute).Version;
        }
      }
      return meta;
    }
  }
}