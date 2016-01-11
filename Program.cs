using System;
using System.IO;
using System.Reflection;

namespace OCS.Package.Util
{
  public class Program : MarshalByRefObject
  {
    static Program()
    {
      AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver;
    }
    static void Main(string[] args)
    {
      var settings = args.Length > 0 ? args[0] : "{}";
      if (settings.GetType().IsArray)
        settings = string.Join(string.Empty, settings);

      if (!settings.StartsWith("-v"))
      {
        var creator = new Creator(settings);
        creator.CreateSpec();
      }
      else
      {
        Console.WriteLine("Current version: {0}", AssemblyName.GetAssemblyName("OCS.Package.Util.exe").Version);
      }

    }

    private static Assembly AssemblyResolver(object sender, ResolveEventArgs arguments)
    {
      var lookupAssembly = new AssemblyName(arguments.Name).Name;
      var resourceName = string.Format("{0}.{1}", lookupAssembly, "dll");
      using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
      {
        if (stream != null)
        {
          var assemblyData = new Byte[stream.Length];
          stream.Read(assemblyData, 0, assemblyData.Length);
          return Assembly.Load(assemblyData);
        }
        //throw new ApplicationException(string.Format("Missing {0} assembly in resource stream {1}.", lookupAssembly, resourceName));
      }
      //not found in resoursefolder  will look in location folder
      if(arguments.RequestingAssembly != null)
      {
        var location = Path.GetDirectoryName( arguments.RequestingAssembly.Location) ?? string.Empty;
        var assemblyFile = Path.Combine(location, resourceName);
        if (File.Exists(assemblyFile))
        {
          var bytes = File.ReadAllBytes(assemblyFile);
          return Assembly.Load(bytes);
        }
      }
      throw new ApplicationException(string.Format("Missing {0} assembly in resource stream {1}.", lookupAssembly, resourceName));
    }
  }

}
