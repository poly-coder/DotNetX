///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var nugetSource = Argument("nugetSource", "https://api.nuget.org/v3/index.json");
var nugetApiKey = Argument("nugetApiKey", EnvironmentVariable("NUGET_API_KEY"));

Information($"Running target {target} in configuration {configuration}");

var packagesDirectory = Directory("./Packages");
var srcProjects = GetFiles("./Src/**/*.csproj").Select(p => p.FullPath);
var testProjects = GetFiles("./Tests/**/*.csproj").Select(p => p.FullPath);
var allProjects = srcProjects.Concat(testProjects);

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Noop");

var cleanTask = Task("Clean")
    .Does(() =>
{
    CleanDirectory(packagesDirectory);
});

var restoreTask = Task("Restore")
    .IsDependentOn(cleanTask)
    .Does(() =>
{
    foreach (var project in allProjects)
    {
        DotNetCoreRestore(project);
    }
});

var buildTask = Task("Build")
    .IsDependentOn(cleanTask)
    .IsDependentOn(restoreTask)
    .Does(() =>
{
    var buildSettings = new DotNetCoreBuildSettings {
        Configuration = configuration,
        NoRestore = true,
    };

    foreach (var project in allProjects)
    {
        DotNetCoreBuild(project, buildSettings);
    }
});

var testTask = Task("Test")
    .IsDependentOn(buildTask)
    .Does(() =>
{
    var testSettings = new DotNetCoreTestSettings {
        NoRestore = true,
        NoBuild = true,
        Configuration = configuration,
    };

    foreach (var project in testProjects)
    {
        DotNetCoreTest(project, testSettings);
    }
});

var packTask = Task("Pack")
    .IsDependentOn(testTask)
    .Does(() =>
{
    var packSettings = new DotNetCorePackSettings {
        NoRestore = true,
        NoBuild = true,
        OutputDirectory = packagesDirectory,
        Configuration = configuration,
    };

    foreach (var project in srcProjects)
    {
        DotNetCorePack(project, packSettings);
    }
});

var pushTask = Task("Push")
    .IsDependentOn(packTask)
    .Does(() =>
{
    var pushSettings = new DotNetCoreNuGetPushSettings {
        Source = nugetSource,
        ApiKey = nugetApiKey,
        SkipDuplicate = true,
        WorkingDirectory = packagesDirectory,
    };

    DotNetCoreNuGetPush("*.nupkg", pushSettings);
});

Task("Default")
    .IsDependentOn(pushTask);

RunTarget(target);
