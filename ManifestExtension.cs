using System.Linq;
using NuGet;

namespace OCS.Package.Util
{
  public static class ManifestExtension
  {
    public static void OrganizeManifest(this Manifest spec, NuspecDefinition definition)
    {
      switch (definition.TypeOfPackage)
      {
        case PackageType.Web:
          break;
        default:
          spec.Files
            .Where(s => s.Target.StartsWith("lib"))
            .ToList()
            .ForEach(f => spec.Files.Remove(f));

          break;

      }
    }
  }
}
