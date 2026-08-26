//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(artifactsDir);
    DotNetClean(solutionFile, new DotNetCleanSettings {
        Configuration = configuration,
        Verbosity = verbosityLevel
    });
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetRestore(solutionFile, new DotNetRestoreSettings {
        Verbosity = verbosityLevel,
        // Enforce deterministic CI restorations
        LockedMode = EnvironmentVariable("CI") == "true"
    });
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    DotNetBuild(solutionFile, new DotNetBuildSettings {
        Configuration = configuration,
        NoRestore = true, // Speeds up build since Restore task already ran
        Verbosity = verbosityLevel
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    Information("🧪 Running Unit/Integration Tests...");
    var projects = GetFiles(testProjectsGlob);

    foreach(var project in projects)
    {
        var projectName = project.GetFilenameWithoutExtension().ToString();

        // Gets the absolute string path (e.g., C:/Repo/artifacts/TestResults/ut)
        var absoluteProjectTestResultsDir = testResultsDir.Combine(Directory(projectName)).FullPath;

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
    }
});

Task("Package")
    .IsDependentOn("Test")
    .Does(() =>
{
    DotNetPublish(apiProject, new DotNetPublishSettings {
        Configuration = configuration,
        OutputDirectory = publishDir,
        Verbosity = verbosityLevel
    });
});

Task("Default")
    .IsDependentOn("Package");
