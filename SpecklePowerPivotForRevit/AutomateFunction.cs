using Objects;
using Objects.BuiltElements.Revit;
using Speckle.Automate.Sdk;
using Speckle.Core.Api.GraphQL.Inputs;
using Speckle.Core.Models;
using Speckle.Core.Models.GraphTraversal;

namespace SpecklePowerPivotForRevit;

/// <summary>
/// This class demonstrates how to create a Speckle Automation function that processes
/// Revit model data and republishes it in a format optimized for Power BI.
/// It serves as a practical example of common Speckle operations including:
/// - Receiving and processing version data
/// - Handling Speckle objects and their properties
/// - Creating new models and versions
/// - Error handling and automation status reporting
/// </summary>
public static class AutomateFunction
{
  private static string _sTargetModelPrefix = null!;

  /// <summary>
  /// Main entry point for the automation function. This method orchestrates the entire
  /// process of receiving, transforming, and republishing Speckle data.
  /// </summary>
  /// <param name="automationContext">Provides access to Speckle's automation capabilities</param>
  /// <param name="functionInputs">User-defined inputs that control the automation's behavior</param>
  public async static Task Run(
    AutomationContext automationContext,
    FunctionInputs functionInputs
  )
  {
    // Store the prefix for use throughout the function
    _sTargetModelPrefix = functionInputs.TargetModelPrefix;
    
    try
    {
      // STEP 1: Initialize and receive data
      // --------------------------------
      Console.WriteLine("Starting execution");
      
      // Force Objects kit to initialize - this ensures we have access to all Speckle object types
      // TIP: Always initialize the Objects kit when working with specific object types (like Revit elements)
      _ = typeof(ObjectsKit).Assembly;

      // Receive the version data from Speckle
      // TIP: ReceiveVersion() handles all the complexity of deserializing Speckle data
      Console.WriteLine("Receiving version");
      var versionObject = await automationContext.ReceiveVersion();

      // STEP 2: Process the received data
      // --------------------------------
      // Flatten the object graph and process each object
      // TIP: Always flatten complex Speckle objects before processing to handle nested structures
      var objects = FlattenAndProcessObjects(versionObject);

      // STEP 3: Enhance the data
      // --------------------------------
      Console.WriteLine("Propagating named properties");
      // Ensure all named properties are properly propagated through the object hierarchy
      objects = PropagateNamedProperties(objects);

      // STEP 4: Set up the target model
      // --------------------------------
      // Get the source model name for reference
      var sourceModelName = await Models.TriggerModelName(automationContext);

      // Generate a target model name using our prefix
      // TIP: Using a consistent naming scheme helps users find and organize their data
      var targetModelName = Models.GenerateTargetModelName(
        sourceModelName,
        _sTargetModelPrefix
      );

      // STEP 5: Create and publish the new version
      // --------------------------------
      // Package the processed objects into a new commit
      var newData = Commit.CommitObject(objects);

      // Create a new version in the project with our processed data
      // TIP: Always provide meaningful commit messages to help users track changes
      var newVersion = await automationContext.CreateNewVersionInProject(
        newData,
        targetModelName,
        "Data from PowerPivot for Revit"
      );

      // STEP 6: Update the automation context
      // --------------------------------
      // Find the target model's ID using its name
      var targetModelId = await ResolveModelIdByName(
        automationContext,
        targetModelName
      );

      // If we found the model, set it as the context view
      // This helps users navigate to the relevant data in the Speckle interface
      if (targetModelId != null)
      {
        var modelVersionIdentifier = $"{targetModelId}@{newVersion}";
        automationContext.SetContextView(
          [modelVersionIdentifier],
          false
        );

        Console.WriteLine($"Context view set with: {modelVersionIdentifier}");
      }
      else
      {
        // Always provide clear error messages to help users troubleshoot
        Console.WriteLine($"Error: No matching model found for the specified name.");
        automationContext.MarkRunException("Target model not found.");
      }

      // STEP 7: Log success and completion
      // --------------------------------
      // TIP: Provide detailed logging to help with debugging and monitoring
      Console.WriteLine("Generated target model name: " + targetModelName);
      Console.WriteLine("Received version: " + versionObject);
      Console.WriteLine("Created new version: " + newVersion);

      // Mark the automation as successful
      // TIP: Always use MarkRunSuccess to properly complete the automation
      automationContext.MarkRunSuccess($"Created new version: {newVersion}");
    }
    catch (Exception e)
    {
      // IMPORTANT: Proper error handling is crucial for automations
      // Always catch exceptions and mark the run as failed with a clear message
      Console.WriteLine("Error: " + e.Message);
      automationContext.MarkRunException(e.Message);
    }
  }

  /// <summary>
  /// Resolves a model ID using its name. This is a common operation when working with
  /// Speckle's project structure.
  /// </summary>
  /// <param name="automationContext">The current automation context</param>
  /// <param name="modelName">Name of the model to find</param>
  /// <returns>The model ID if found, null otherwise</returns>
  private static async Task<string?> ResolveModelIdByName(
    AutomationContext automationContext,
    string modelName
  )
  {
    var client = automationContext.SpeckleClient;

    // Create a filter to find the model
    // TIP: Always use filters to efficiently query Speckle's database
    var filter = new ProjectModelsFilter(
      search: modelName,
      contributors: null,
      sourceApps: null,
      ids: null,
      excludeIds: null,
      onlyWithVersions: false
    );

    // Query the project for matching models
    var targetProjectModels = (
      await client.Project.GetWithModels(
        projectId: automationContext.AutomationRunData.ProjectId,
        modelsLimit: 1, // Efficiency: Only request what we need
        modelsFilter: filter
      )
    ).models;

    // Return the first matching model ID or null
    return targetProjectModels?.items.FirstOrDefault()?.id;
  }

  /// <summary>
  /// Ensures that named properties are properly propagated through the object hierarchy.
  /// This is crucial for maintaining data relationships and accessibility.
  /// </summary>
  private static List<Base> PropagateNamedProperties(List<Base> objects)
  {
    foreach (var obj in objects)
    {
      var newParameters = new Base();

      if (obj["parameters"] is Base existingParameters)
      {
        // Process instance properties
        foreach (
          var prop in existingParameters
            .GetMembers(DynamicBaseMemberType.InstanceAll)
            .Where(prop => !Processor.PropsToSkip.Contains(prop.Key))
        )
        {
          newParameters[prop.Key] = prop.Value;
        }

        // Process dynamic properties
        foreach (
          var prop in existingParameters
            .GetMembers(DynamicBaseMemberType.Dynamic)
            .Where(prop => !Processor.PropsToSkip.Contains(prop.Key))
        )
        {
          if (prop.Value is Parameter parameter)
          {
            // Ensure property names are valid
            var newPropName = DynamicBase.RemoveDisallowedPropNameChars(parameter.name);

            if (string.IsNullOrEmpty(parameter.name))
            {
              continue;
            }

            newParameters[newPropName] = parameter;
          }
          else
          {
            newParameters[prop.Key] = prop.Value;
          }
        }
      }

      obj["parameters"] = newParameters;
    }

    return objects;
  }

  /// <summary>
  /// Flattens and processes a complex Speckle object graph into a list of individual objects.
  /// This is a fundamental operation when working with Speckle data structures.
  /// </summary>
  private static List<Base> FlattenAndProcessObjects(Base versionObject)
  {
    // Create a traversal function to walk the object graph
    // TIP: Always use the DefaultTraversal for consistent object graph processing
    var traversal = DefaultTraversal.CreateTraversalFunc();
    var traversalContexts = traversal.Traverse(versionObject);

    // Process each object and filter out nulls
    // TIP: Use LINQ for clean, functional processing of collections
    return traversalContexts
      .Select(tc => Processor.ProcessObject(tc.Current))
      .Where(obj => obj != null)
      .ToList()!;
  }
}