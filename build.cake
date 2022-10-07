#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#addin nuget:?package=Cake.FileHelpers&version=4.0.1

/*
    This should be a simple file.  It is not because the new .Net Core
    tooling does not support .Net 3.5
 */

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = DateTime.Now.ToString("yyyy.MM.dd.HHmm");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
  .Does(() =>
{
  CleanDirectory(Directory("./src/Innovator.Client/bin/"));
  CleanDirectory(Directory("./src/Innovator.Client/obj/"));
  CleanDirectory(Directory("./publish/Innovator.Client/lib/"));
  CleanDirectory(Directory("./artifacts/"));
});

Task("Patch-Version")
  .IsDependentOn("Clean")
  .Does(() =>
{
  CreateAssemblyInfo("./src/Innovator.Client/AssemblyInfo.Version.cs"
  , new AssemblyInfoSettings {
      Version = version,
      FileVersion = version
    }
  );
});

Task("Build")
  .IsDependentOn("Patch-Version")
  .Does(() =>
{
  CleanDirectory(Directory("./src/Innovator.Client/bin/"));
  CleanDirectory(Directory("./src/Innovator.Client/obj/"));
  DotNetCoreRestore("./src/Innovator.Client/Innovator.Client.csproj");
  DotNetCoreBuild("./src/Innovator.Client/Innovator.Client.csproj", new DotNetCoreBuildSettings
  {
     Configuration = configuration
  });
  
  CopyFiles("./src/Innovator.Client/bin/" + configuration + "/**/Innovator.Client.*", "./publish/Innovator.Client/lib/", true);
  var files = GetFiles("./publish/Innovator.Client/lib/**/*")
    .Where(f => !f.ToString().EndsWith("Innovator.Client.dll", StringComparison.OrdinalIgnoreCase)
      && !f.ToString().EndsWith("Innovator.Client.xml", StringComparison.OrdinalIgnoreCase));
  foreach (var file in files)
    DeleteFile(file);
});

Task("NuGet-Pack")
  .IsDependentOn("Build")
  .Does(() =>
{
  var nuGetPackSettings = new NuGetPackSettings {
    Version = version,
    OutputDirectory = "./artifacts/"
  };
  NuGetPack("./publish/Innovator.Client/Innovator.Client.nuspec", nuGetPackSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("NuGet-Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
