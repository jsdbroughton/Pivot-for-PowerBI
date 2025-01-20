using Speckle.Automate.Sdk;

namespace SpecklePowerPivotForRevit;

/// <summary>
/// Handles model naming and model-related operations.
///
/// KEY CONCEPT: Separation of Concerns
/// This class demonstrates the Single Responsibility Principle by focusing solely
/// on model naming logic. This makes the code:
/// 1. Easier to test (see ModelsTests.cs)
/// 2. More maintainable
/// 3. More reusable
/// </summary>
public static class Models
{
  /// <summary>
  /// Retrieves the name of the triggering model.
  ///
  /// PATTERN: Async Operations with Clear Error Handling
  /// Shows how to safely handle asynchronous operations while providing
  /// meaningful logging.
  /// </summary>
  internal static async Task<string> TriggerModelName(
    AutomationContext automationContext
  )
  {
    // Extract IDs early to make the code more readable
    // TIP: Unpacking complex objects at the start of a method makes the logic clearer
    var modelId = automationContext.AutomationRunData.Triggers[0].Payload.ModelId;
    var versionId = automationContext.AutomationRunData.Triggers[0].Payload.VersionId;
    var projectId = automationContext.AutomationRunData.ProjectId;

    // Log key information for debugging
    // TIP: Structured logging helps with troubleshooting
    Console.WriteLine($"Project ID: {projectId}");
    Console.WriteLine($"Version ID: {versionId}");

    var client = automationContext.SpeckleClient;
    return (await client.Model.Get(modelId, projectId)).name;
  }

  /// <summary>
  /// Generates a new model name based on a source name and prefix.
  ///
  /// KEY CONCEPT: Robust String Processing
  /// Shows how to:
  /// 1. Validate inputs thoroughly
  /// 2. Handle edge cases (like multiple slashes)
  /// 3. Maintain consistent formatting
  /// </summary>
  public static string GenerateTargetModelName(string sourceModelName, string prefix)
  {
    // Validate inputs first - fail fast principle
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

    // Clean the input data
    prefix = prefix.TrimStart('/');

    if (string.IsNullOrEmpty(prefix))
    {
      throw new ArgumentException(
        "Prefix cannot be just a forward slash",
        nameof(prefix)
      );
    }

    // Process the path components
    var cleanPrefix = prefix.Trim('/');
    var modelParts = sourceModelName.Split('/', StringSplitOptions.RemoveEmptyEntries);

    // Construct the final path
    return $"{cleanPrefix}/{string.Join("/", modelParts)}";
  }
}
