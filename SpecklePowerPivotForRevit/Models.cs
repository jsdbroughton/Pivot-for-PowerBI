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

  public static string GenerateTargetModelName(string sourceModelName, string prefix)
  {
    if (string.IsNullOrEmpty(sourceModelName))
    {
      throw new ArgumentException(
        "Source model name cannot be null or empty",
        nameof(sourceModelName)
      );
    }

    if (string.IsNullOrEmpty(prefix))
    {
      throw new ArgumentException("Prefix cannot be null or empty", nameof(prefix));
    }

    // Ensure the prefix doesn't start with a slash
    prefix = prefix.TrimStart('/');

    if (string.IsNullOrEmpty(prefix))
    {
      throw new ArgumentException(
        "Prefix cannot be just a forward slash",
        nameof(prefix)
      );
    }

    // Split and clean both prefix and source model name
    var cleanPrefix = prefix.Trim('/');
    var modelParts = sourceModelName.Split('/', StringSplitOptions.RemoveEmptyEntries);

    // Join all parts ensuring no double slashes
    return $"{cleanPrefix}/{string.Join("/", modelParts)}";
  }
}
