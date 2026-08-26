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
// LIFECYCLE HOOKS & EXCEPTION HANDLING
//////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    Information("========================================");
    Information($"🚀 Starting CI Build Step");
    Information($"🎯 Target: {target}");
    Information($"⚙️  Configuration: {configuration}");
    Information("========================================");
});

// Automatically logs when ANY task starts
TaskSetup(setupContext =>
{
    Information($"\n▶️ Starting Task: {setupContext.Task.Name}...");
});

// Automatically logs when ANY task finishes and records its duration
TaskTeardown(teardownContext =>
{
    Information($"⏸️ Finished Task: {teardownContext.Task.Name} (Duration: {teardownContext.Duration})");
});

// Evaluates the final state of the entire pipeline
Teardown(ctx =>
{
    if (ctx.Successful)
    {
        Information("\n✅ Step completed successfully.");
    }
    else
    {
        Error("\n❌ Step failed!");
        if (ctx.Exception != null)
        {
            Error($"Failure Reason: {ctx.Exception.Message}");
        }
    }
});

// Global error handler to catch and log deep stack traces
OnError(exception =>
{
    Error("\n🚨 A fatal error occurred during pipeline execution:");
    Error(exception.ToString());
});
