using Speckle.Automate.Sdk;

namespace SpecklePowerPivotForRevit;

public static class Models
{
  internal static async Task<string> TriggerModelName(
    AutomationContext automationContext
  )
  {
    var modelId = automationContext.AutomationRunData.Triggers[0].Payload.ModelId;
    var versionId = automationContext.AutomationRunData.Triggers[0].Payload.VersionId;
    var projectId = automationContext.AutomationRunData.ProjectId;

    Console.WriteLine($"Project ID: {projectId}");
    Console.WriteLine($"Version ID: {versionId}");

    var client = automationContext.SpeckleClient;

    return (await client.Model.Get(modelId, projectId)).name;
  }

  internal static string GenerateTargetModelName(string sourceModelName, string prefix)
  {
    // Ensure the prefix doesn't start with a slash
    prefix = prefix.TrimStart('/');

    // Split the source model name into parts
    var parts = sourceModelName.Split('/', StringSplitOptions.RemoveEmptyEntries);

    // Combine the prefix with the original model path
    var targetModelName = $"{prefix}/{string.Join("/", parts)}";

    return targetModelName;
  }
}
