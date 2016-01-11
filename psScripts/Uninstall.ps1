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

$packageTargetExists = $xml.Project.SelectSingleNode("//csproj:Import[@Project='`$(SolutionDir)\.tools\ocs.package.util.targets']", $nsmgr)

if($packageTargetExists) {
  $xml.Project.RemoveChild($packageTargetExists)
}

#save the changes.
$xml.Save($project.FullName)