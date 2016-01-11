using System;
using System.Linq;
using NuGet;

namespace OCS.Package.Util
{
  public class LocalPackageRepository
  {
    private const string ApiKey = "your-api-key";
    private readonly IPackageRepository _localRepository;
    private readonly PackageServer _ps;
    public LocalPackageRepository(string path, string packageSource)
    {
      packageSource = string.IsNullOrEmpty(packageSource) ? "http://nugetsource/" : packageSource;
      _ps = new PackageServer(packageSource, "Package.Util");
      _localRepository = PackageRepositoryFactory.Default.CreateRepository(path);
    }

    public void RemoveOldVersions(string id)
    {
      _localRepository.FindPackagesById(id).ToList().ForEach(p => _localRepository.RemovePackage(p));

    }

    public IPackage GetPackage(string id, SemanticVersion version)
    {
      return _localRepository.FindPackagesById(id).FirstOrDefault(v => v.Version == version);
    }

    public void PushPackage(string id)
    {
      var package = _localRepository.FindPackagesById(id).First(p => p.IsLatestVersion);
      PushPackage(package);
    }

    public void PushPackage(IPackage package)
    {
      var timeout = Convert.ToInt32(TimeSpan.FromMinutes(5).TotalMilliseconds);
#if !DEBUG
      using (var stream = package.GetStream())
      {
        var packageSize = stream.Length;
        _ps.PushPackage(ApiKey, package, packageSize, timeout, false);
      }
#endif

    }
  }
}
