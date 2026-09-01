//////////////////////////////////////////////////////////////////////
// 1. CORE BUILD PIPELINE (Clean -> Restore -> Build)
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    Information($"🧹 Cleaning artifacts directory: {artifactsDir}");
    CleanDirectory(artifactsDir);

    Information($"🧹 Cleaning solution: {solutionFile}");
    DotNetClean(solutionFile, new DotNetCleanSettings {
        Configuration = configuration,
        Verbosity = verbosityLevel
    });
})
.OnError(exception =>
{
    Error($"[Clean Task] 🚨 Failed to clean directories. Error: {exception.Message}");
    throw exception; // Preserves full stack trace instead of throw exception
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    Information($"🔧 Restoring NuGet packages for: {solutionFile}");
    bool isCI = EnvironmentVariable("CI") == "true";
    DotNetRestore(solutionFile, new DotNetRestoreSettings {
        Verbosity = verbosityLevel,
        LockedMode = isCI,
        ForceEvaluate = !isCI
    });
})
.OnError(exception =>
{
    Error($"[Restore Task] 🚨 Package restore failed. Error: {exception.Message}");
    throw exception;
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    Information($"🏗️ Building solution: {solutionFile} (Configuration: {configuration})");
    DotNetBuild(solutionFile, new DotNetBuildSettings {
        Configuration = configuration,
        NoRestore = true,
        Verbosity = verbosityLevel
    });
})
.OnError(exception =>
{
    Error($"[Build Task] 🚨 Build failed compilation. Error: {exception.Message}");
    throw exception;
});

//////////////////////////////////////////////////////////////////////
// 2. VALIDATION & QUALITY GATES (Test -> Mutation-Test)
//////////////////////////////////////////////////////////////////////

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    Information("🧪 Running Unit/Integration Tests...");
    var projects = GetFiles(testProjectsGlob);
    var failedProjects = new List<string>();

    foreach(var project in projects)
    {
        var projectName = project.GetFilenameWithoutExtension().ToString();
        Information($"\n  ▶️ Testing project: {projectName}");

        var absoluteProjectTestResultsDir = testResultsDir.Combine(Directory(projectName)).FullPath;

        try
        {
            DotNetTest(project.FullPath, new DotNetTestSettings {
                Configuration = configuration,
                NoBuild = true,
                NoRestore = true,
                Verbosity = verbosityLevel,
                ResultsDirectory = absoluteProjectTestResultsDir,
                Settings = "../.runsettings", // Use global .runsettings configuration
                ArgumentCustomization = args => args
                    .Append("--logger \"trx;LogFileName=test-results.trx\"")
                    .Append("--collect:\"XPlat Code Coverage\"") // Standard collector mechanism
            });
            Information($"  ✅ Tests passed for {projectName}");
        }
        catch (Exception ex)
        {
            Error($"  ❌ Tests FAILED for {projectName}. Error: {ex.Message}");
            failedProjects.Add(projectName);
        }
    }

    if (failedProjects.Any())
    {
        throw new Exception($"Test failures detected in {failedProjects.Count} project(s): {string.Join(", ", failedProjects)}");
    }
});

Task("Mutation-Test")
    .IsDependentOn("Test")
    .Does(() =>
{
    Information("🧬 Running Stryker Mutation Testing...");

    DotNetTool("stryker", new DotNetToolSettings {
        WorkingDirectory = MakeAbsolute(Directory("../")),
        ArgumentCustomization = args => args
            .Append($"--solution \"api.sln\"")
            .Append("-f \"stryker-config.json\"")
    });
})
.OnError(exception =>
{
    Error($"[Mutation-Test Task] 🚨 Stryker mutation testing failed or fell below threshold. Error: {exception.Message}");
    throw exception;
});

//////////////////////////////////////////////////////////////////////
// 3. PACKAGING & PUBLISHING
//////////////////////////////////////////////////////////////////////

Task("Package")
    .IsDependentOn("Test")
    .Does(() =>
{
    Information($"📦 Publishing project {apiProject} to {publishDir}");
    DotNetPublish(apiProject, new DotNetPublishSettings {
        Configuration = configuration,
        OutputDirectory = publishDir,
        Verbosity = verbosityLevel
    });
})
.OnError(exception =>
{
    Error($"[Package Task] 🚨 Publishing failed. Error: {exception.Message}");
    throw exception;
});

//////////////////////////////////////////////////////////////////////
// 4. PIPELINE ENTRY POINTS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Package");
