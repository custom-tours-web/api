// 1. Tooling & Addins
#tool nuget:?package=Cake.Common&version=6.2.0
#tool nuget:?package=Cake.DotNetTool.Module&version=6.2.0
#tool nuget:?package=ReportGenerator&version=5.5.11

//////////////////////////////////////////////////////////////////////
// 2. ARGUMENTS & VARIABLES
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var verbosityLevel = Argument("verbosity", DotNetVerbosity.Minimal);

// Centralized paths prevent typos and make maintenance easier
var solutionFile = "./api.sln";
var apiProject = "./API/api.csproj";
var testProjectsGlob = "./Tests/**/*.csproj";

var artifactsDir = Directory("./artifacts");
var testResultsDir = artifactsDir + Directory("TestResults");
var publishDir = artifactsDir + Directory("publish");
var coverageReportDir = testResultsDir + Directory("Report");

//////////////////////////////////////////////////////////////////////
// 3. LIFECYCLE HOOKS
//////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    Information("========================================");
    Information($"🚀 Starting CI Build");
    Information($"🎯 Target: {target}");
    Information($"⚙️  Configuration: {configuration}");
    Information("========================================");
});

Teardown(ctx =>
{
    Information("✅ Build lifecycle completed successfully.");
});

//////////////////////////////////////////////////////////////////////
// 4. TASKS
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
        DotNetTest(project.FullPath, new DotNetTestSettings {
            Configuration = configuration,
            NoBuild = true,    // Speeds up test since Build task already ran
            NoRestore = true,
            Verbosity = verbosityLevel,
            Logger = "trx",
            ArgumentCustomization = args => args.Append("--collect:\"XPlat Code Coverage\""),
            ResultsDirectory = testResultsDir
        });
    }

    // 📊 Generate HTML Coverage Report automatically
    Information("📊 Generating Coverage Report...");
    ReportGenerator(
        GetFiles($"{testResultsDir}/**/coverage.cobertura.xml"),
        coverageReportDir,
        new ReportGeneratorSettings {
            ReportTypes = new[] { ReportGeneratorReportType.Html }
        }
    );
});

Task("Package")
    .IsDependentOn("Test")
    .Does(() =>
{
    DotNetPublish(apiProject, new DotNetPublishSettings {
        Configuration = configuration,
        NoBuild = true,
        NoRestore = true,
        OutputDirectory = publishDir,
        Verbosity = verbosityLevel
    });
});

Task("Default")
    .IsDependentOn("Package");

//////////////////////////////////////////////////////////////////////
// 5. EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
