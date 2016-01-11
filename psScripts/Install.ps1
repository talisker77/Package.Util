param($installPath, $toolsPath, $package, $project)
if(!$project){
  $project = Get-Project
}
#Write-Host "Configuring project file: $project.FullName"

#save the project file first - this commits the changes made by nuget before this     script runs.
$project.Save()

#Load the csproj file into an xml object
$xml = [XML] (gc $project.FullName)

#grab the namespace from the project element so your xpath works.
$nsmgr = New-Object System.Xml.XmlNamespaceManager -ArgumentList $xml.NameTable
$nsmgr.AddNamespace('csproj',$xml.Project.GetAttribute("xmlns"))


#<Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" /> 
#//csproj:Target[@Name='AfterBuild']
$packageTargetExists = $xml.Project.SelectSingleNode("//csproj:Import[@Project='`$(SolutionDir)\.tools\ocs.package.util.targets']", $nsmgr)

if(!$packageTargetExists) {
  $importTarget = $xml.Project.SelectSingleNode("//csproj:Import[contains(@Project, '.targets')]", $nsmgr)

  $packageTarget = $xml.CreateElement("Import", $xml.Project.GetAttribute("xmlns"))
  $packageTarget.SetAttribute("Project", "`$(SolutionDir)\.tools\ocs.package.util.targets")
  #Condition="Exists('$(SolutionDir)\.nuget\NuGet.targets')"
  $packageTarget.SetAttribute("Condition", "Exists('`$(SolutionDir)\.tools\ocs.package.util.targets')")

  if(!$importTarget) {
    Write-Host "Adding after import" 
    $importTarget.InsertAfter($packageTarget)
  }
  else {
    Write-Host "Import project *.targets tag not found. Appends it at project root level."
    $xml.Project.AppendChild($packageTarget)
  }
}

#save the changes.
$xml.Save($project.FullName)