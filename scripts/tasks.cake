//////////////////////////////////////////////////////////////////////
// TASKS
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
    throw; // Rethrow to fail the pipeline
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    Information($"🔧 Restoring NuGet packages for: {solutionFile}");
    DotNetRestore(solutionFile, new DotNetRestoreSettings {
        Verbosity = verbosityLevel,
        // Enforce deterministic CI restorations
        LockedMode = EnvironmentVariable("CI") == "true"
    });
})
.OnError(exception =>
{
    Error($"[Restore Task] 🚨 Package restore failed. Error: {exception.Message}");
    throw;
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    Information($"🏗️ Building solution: {solutionFile} (Configuration: {configuration})");
    DotNetBuild(solutionFile, new DotNetBuildSettings {
        Configuration = configuration,
        NoRestore = true, // Speeds up build since Restore task already ran
        Verbosity = verbosityLevel
    });
})
.OnError(exception =>
{
    Error($"[Build Task] 🚨 Build failed compilation. Error: {exception.Message}");
    throw;
});

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

        // Gets the absolute string path (e.g., C:/Repo/artifacts/TestResults/ut)
        var absoluteProjectTestResultsDir = testResultsDir.Combine(Directory(projectName)).FullPath;

        try
        {
            DotNetTest(project.FullPath, new DotNetTestSettings {
                Configuration = configuration,
                NoBuild = true,
                NoRestore = true,
                Verbosity = verbosityLevel,
                // 👇 Forces the TRX logger to use the absolute root path
                ResultsDirectory = absoluteProjectTestResultsDir,

                ArgumentCustomization = args => args
                    .Append("--logger \"trx;LogFileName=test-results.trx\"")
                    .Append("/p:CollectCoverage=true")
                    .Append("/p:CoverletOutputFormat=cobertura")
                    // 👇 Forces Coverlet to use the absolute root path
                    .Append($"/p:CoverletOutput=\"{absoluteProjectTestResultsDir}/\"")
            });
            Information($"  ✅ Tests passed for {projectName}");
        }
        catch (Exception ex)
        {
            Error($"  ❌ Tests FAILED for {projectName}. Error: {ex.Message}");
            // Add to list but continue running the rest of the test projects
            failedProjects.Add(projectName);
        }
    }

    // Evaluate if any test projects failed during the loop
    if (failedProjects.Any())
    {
        throw new Exception($"Test failures detected in {failedProjects.Count} project(s): {string.Join(", ", failedProjects)}");
    }
});

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
    throw;
});

Task("Default")
    .IsDependentOn("Package");
