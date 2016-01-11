# OCS.Package.Util

OCS.Package.Util has been added to your project.

A solution folder name '.tools' has been added to your solution containing the following files:

  - ocs.package.util.targets
  - OCS.Package.Util.exe

The ocs.package.util.targets contains a target named = 'PackageFactory'


    <Target Name="PackageFactory"  AfterTargets="CopyFilesToOutputDirectory" >
      <Exec Command="$(PackageCreateCommand)"  LogStandardErrorAsError="true"/>
    </Target>

If you want to restrict which configuration to build and publish the package add a condition to the Target tag like

    <Target Name="PackageFactory"  AfterTargets="CopyFilesToOutputDirectory" Condition="'$(Configuration)'=='Release'" >
      <Exec Command="$(PackageCreateCommand)"  LogStandardErrorAsError="true"/>
    </Target>

-----
## nuspec-def.xml file
##### Remember to include a nuspec-def.xml file if you need to build packages with more tooling and content.

###### Example of a nuspec-def.xml file

Following tokens in nuspec-def.xml can be used: 

- \#\#TargetDir\#\# =\> which is the outputpath of build (in given configuration)
- \#\#ProjectDir\#\# =\> which is the path to the project root folder. 

###### Example file:

    <?xml version="1.0" encoding="utf-8" ?>
      <package xmlns="http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd">
      <metadata>
        <id>id of package if different from assembly name</id>
        <summary>A package create and deploy tool...</summary>
        <isToolsOnly>true/false => no lib folder if true.</isToolsOnly>
        <includePdb>false/true => to include pdb in package</includePdb>
        <publishPackage>true/false => true if to push package to ocsutv01</publishPackage>
      </metadata>
      <files>
        <file src="##ProjectDir##\Areas\Onboard\**" target="Content\NewVersion\Areas\Onboard" exclude="**.nupkg;**.pdb;*.config;bin\**;obj\**;**.cs;**csproj*;*.xml;*.nuspec;**.old;**.tt;" />
        <file src="OCS.Package.Util.exe" target=".utils" />
        <file src="..\..\psScripts\init.ps1" target="tools\Init.ps1" />
        <file src="..\..\psScripts\Install.ps1" target="tools" />
        <file src="..\..\readme.md" target="content" />
        <file src="##TargetDir##\**.dll" target="lib\net40 " />
        <file src="##TargetDir##\**.xml" target="lib\net40 " />
      </files>
    </package>



-----














-----
This part is just left for references.
To learn more about the markdown syntax, refer to these links:

 - [Markdown Syntax Guide](http://daringfireball.net/projects/markdown/syntax)
 - [Markdown Basics](http://daringfireball.net/projects/markdown/basics)
 - [GitHub Flavored Markdown](http://github.github.com/github-flavored-markdown/) 
 - [markdown at wikipedia](https://secure.wikimedia.org/wikipedia/en/wiki/Markdown)