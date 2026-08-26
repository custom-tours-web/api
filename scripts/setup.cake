//////////////////////////////////////////////////////////////////////
// ARGUMENTS & VARIABLES
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var verbosityLevel = Argument("verbosity", DotNetVerbosity.Minimal);

// Centralized paths prevent typos and make maintenance easier
var solutionFile = "./api.slnx";
var apiProject = "./api/api.csproj";
var testProjectsGlob = "./tests/**/*.csproj";

var artifactsDir = MakeAbsolute(Directory("./artifacts"));
var testResultsDir = artifactsDir.Combine(Directory("TestResults"));
var publishDir = artifactsDir.Combine(Directory("publish"));

//////////////////////////////////////////////////////////////////////
// LIFECYCLE HOOKS
//////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    Information("========================================");
    Information($"🚀 Starting CI Build Step");
    Information($"🎯 Target: {target}");
    Information($"⚙️  Configuration: {configuration}");
    Information("========================================");
});

Teardown(ctx =>
{
    Information("✅ Step completed successfully.");
});
