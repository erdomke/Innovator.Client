#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#addin "Cake.FileHelpers"

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

private string[] MergeCompileLines(string[] file, string[] newCompiles)
{
  var first = int.MaxValue;
  var last = int.MinValue;
  
  for (var i = 0; i < file.Length; i++)
  {
    if (file[i].Trim().StartsWith("<Compile Include="))
    {
      first = Math.Min(first, i);
      last = Math.Max(last, i);
    }
  }
  
  return file.Take(first)
    .Concat(newCompiles)
    .Concat(file.Skip(last + 1))
    .ToArray();
}

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

Task("Patch-Project-Files")
  .IsDependentOn("Patch-Version")
  .Does(() =>
{
  var compileLines = FileReadLines("./src/Innovator.Client/Innovator.Client.csproj")
    .Where(l => l.Trim().StartsWith("<Compile Include="))
    .ToArray();
  
  var newLines = MergeCompileLines(FileReadLines("./src/Innovator.Client/Innovator.Client.Net35.csproj"), compileLines);
  FileWriteLines("./src/Innovator.Client/Innovator.Client.Net35.csproj", newLines);
  
  newLines = MergeCompileLines(FileReadLines("./src/Innovator.Client/Innovator.Client.Net45.csproj"), compileLines);
  FileWriteLines("./src/Innovator.Client/Innovator.Client.Net45.csproj", newLines);
});

Task("Build-Net35")
  .IsDependentOn("Patch-Project-Files")
  .Does(() =>
{
  try
  {
    CleanDirectory(Directory("./src/Innovator.Client/bin/"));
    CleanDirectory(Directory("./src/Innovator.Client/obj/"));
    NuGetRestore("./src/Innovator.Client.Net35.sln");
    DotNetBuild("./src/Innovator.Client/Innovator.Client.Net35.csproj", settings =>
      settings.SetConfiguration(configuration));    
  }
  catch (Exception ex)
  {
    Error(ex.ToString());
    throw;
  }
});

Task("Build")
  .IsDependentOn("Build-Net35")
  .Does(() =>
{
  CleanDirectory(Directory("./src/Innovator.Client/bin/"));
  CleanDirectory(Directory("./src/Innovator.Client/obj/"));
  DotNetCoreRestore("./src/Innovator.Client/Innovator.Client.NetCore.csproj");
  DotNetCoreBuild("./src/Innovator.Client/Innovator.Client.NetCore.csproj", new DotNetCoreBuildSettings
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
