//////////////////////////////////////////////////////////////////////
// 1. CLI ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var verbosityLevel = Argument("verbosity", DotNetVerbosity.Minimal);

//////////////////////////////////////////////////////////////////////
// 2. PROJECT & SOLUTION PATHS
//////////////////////////////////////////////////////////////////////
var solutionFile = "../api.sln";
var apiProject = "../api/api.csproj";
var testProjectsGlob = "../tests/**/*.csproj";

//////////////////////////////////////////////////////////////////////
// 3. OUTPUT & ARTIFACT DIRECTORIES
//////////////////////////////////////////////////////////////////////
var artifactsDir = MakeAbsolute(Directory("../artifacts"));
var testResultsDir = artifactsDir.Combine(Directory("TestResults"));
var publishDir = artifactsDir.Combine(Directory("publish"));

//////////////////////////////////////////////////////////////////////
// 4. LIFECYCLE HOOKS & EXCEPTION HANDLING
//////////////////////////////////////////////////////////////////////
Setup(ctx =>
{
  Information("========================================");
  Information($"🚀 Starting CI Build Step");
  Information($"🎯 Target: {target}");
  Information($"⚙️  Configuration: {configuration}");
  Information("========================================");
});

TaskSetup(setupContext =>
{
  Information($"\n▶️ Starting Task: {setupContext.Task.Name}...");
});

TaskTeardown(teardownContext =>
{
  Information($"⏸️ Finished Task: {teardownContext.Task.Name} (Duration: {teardownContext.Duration})");
});

Teardown(ctx =>
{
  if (ctx.Successful)
  {
    Information("\n✅ Step completed successfully.");
  }
  else
  {
    Error("\n❌ Step failed!");
    if (ctx.ThrownException != null)
    {
      Error($"Failure Reason: {ctx.ThrownException.Message}");
    }
  }
});
