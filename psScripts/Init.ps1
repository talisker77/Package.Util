param($installPath, $toolsPath, $package, $project)

# Get the open solution.
$solution = Get-Interface $dte.Solution ([EnvDTE80.Solution2])

# Create a child solution folder.


$rootDir = (Get-Item $installPath).parent.parent.fullname
$toolsTarget = Join-Path $rootDir '.tools'
if(!(test-path $toolsTarget)){
  mkdir $toolsTarget
}

$toolsFile = (Join-Path $installPath '.util/OCS.Package.Util.exe')
Write-Host "OCS.Package.Util.exe source file: $toolsFile"

$toolsTargetFile = (Join-Path $installPath '.util/ocs.package.util.targets')
$destinationFile = (Join-Path $toolsTarget 'OCS.Package.Util.exe')

$targetDestinationFile = (Join-Path $toolsTarget 'ocs.package.util.targets')

Copy-Item $toolsFile $toolsTarget -force
Copy-Item $toolsTargetFile $toolsTarget -force

$solutionItemsNode = $solution.Projects | where-object { $_.ProjectName -eq ".tools" } | select -first 1

if (!$solutionItemsNode) {
  $toolsSolutionFolder = $solution.AddSolutionFolder(".tools")
  $solutionFolder = Get-Interface $toolsSolutionFolder.ProjectItems ([EnvDTE.ProjectItems])
 
  # add all our support deploy scripts to our Support solution folder
  $solutionFolder.AddFromFile($targetDestinationFile)
  $solutionFolder.AddFromFile($destinationFile)

  #$solutionItemsNode.Save()
  #$solutionFolder.Save()
  if(!$solution.Saved) {
    $solution.SaveAs($solution.FullName)
  }
}


Write-Host "OCS.Package.Util installed."
