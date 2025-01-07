using Objects;
using Objects.BuiltElements.Revit;
using Speckle.Automate.Sdk;
using Speckle.Core.Models;
using Speckle.Core.Models.GraphTraversal;

namespace SpecklePowerPivotForRevit;

public static class AutomateFunction
{
  internal static bool ResolveInstances;
  private static bool _sPropagateNamedProperties;
  private static string _sTargetModelPrefix = null!;
  internal static bool PrefixMergedDefinitionProperties;

  public async static Task Run(
    AutomationContext automationContext,
    FunctionInputs functionInputs
  )
  {
    ResolveInstances = functionInputs.ResolveInstances;
    _sPropagateNamedProperties = functionInputs.PropagateNamedProperties;
    _sTargetModelPrefix = functionInputs.TargetModelPrefix;
    PrefixMergedDefinitionProperties = functionInputs.PrefixMergedDefinitionProperties;
    try
    {
      Console.WriteLine("Starting execution");
      _ = typeof(ObjectsKit).Assembly; // INFO: Force objects kit to initialize

      Console.WriteLine("Receiving version");
      var versionObject = await automationContext.ReceiveVersion();

      var objects = FlattenAndProcessObjects(versionObject);

      if (_sPropagateNamedProperties)
      {
        Console.WriteLine("Propagating named properties");
        objects = PropagateNamedProperties(objects);
      }

      // Get the source model name
      var sourceModelName = await Models.TriggerModelName(automationContext);

      // Generate the target model name
      var targetModelName = Models.GenerateTargetModelName(
        sourceModelName,
        _sTargetModelPrefix
      );

      var newData = Commit.CommitObject(objects);

      var newVersion = await automationContext.CreateNewVersionInProject(
        newData,
        targetModelName,
        "Data from PowerPivot for Revit"
      );

      automationContext.SetContextView(new List<string> { newVersion }, false);

      Console.WriteLine("Generated target model name: " + targetModelName);
      Console.WriteLine("Received version: " + versionObject);
      Console.WriteLine("Created new version: " + newVersion);

      automationContext.MarkRunSuccess($"Created new version: {newVersion}");
    }
    catch (Exception e)
    {
      Console.WriteLine("Error: " + e.Message);
      automationContext.MarkRunException(e.Message);
    }
  }

  private static List<Base> PropagateNamedProperties(List<Base> objects)
  {
    foreach (var obj in objects)
    {
      var newParameters = new Base();
      var existingParameters = obj["parameters"] as Base;

      if (existingParameters != null)
      {
        foreach (
          var prop in existingParameters
            .GetMembers(DynamicBaseMemberType.InstanceAll)
            .Where(prop => !Processor.PropsToSkip.Contains(prop.Key))
        )
        {
          newParameters[prop.Key] = prop.Value;
        }

        foreach (
          var prop in existingParameters
            .GetMembers(DynamicBaseMemberType.Dynamic)
            .Where(prop => !Processor.PropsToSkip.Contains(prop.Key))
        )
        {
          if (prop.Value is Parameter parameter)
          {
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

  private static List<Base> FlattenAndProcessObjects(Base versionObject)
  {
    var traversal = DefaultTraversal.CreateTraversalFunc();
    var traversalContexts = traversal.Traverse(versionObject);

    return traversalContexts
      .Select(tc => Processor.ProcessObject(tc.Current))
      .Where(obj => obj != null) // Filter out nulls
      .ToList()!;
  }
}
