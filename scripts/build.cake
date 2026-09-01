//////////////////////////////////////////////////////////////////////
// 1. SCRIPT DIRECTIVES
//////////////////////////////////////////////////////////////////////
#load "setup.cake"
#load "tasks.cake"

//////////////////////////////////////////////////////////////////////
// 2. EXECUTION CONTROL
//////////////////////////////////////////////////////////////////////
try
{
    Information($"[ENTRY POINT] Initiating execution for target: {target}");

    // Executes the Cake dependency graph
    RunTarget(target);

    Information($"[ENTRY POINT] Successfully completed execution for target: {target}");
}
//////////////////////////////////////////////////////////////////////
// 3. ERROR HANDLING & PIPELINE FAULT ENFORCEMENT
//////////////////////////////////////////////////////////////////////
catch (Exception ex)
{
    // Log the error in bold red in the console
    Error($"[ENTRY POINT] 🚨 Execution failed for target '{target}':");
    Error(ex.Message);

    // IMPORTANT: Rethrow the exception!
    // If you do not rethrow, the script will exit with a "0" (Success) code,
    // and GitHub Actions will mistakenly think the build passed.
    throw ex;
}
