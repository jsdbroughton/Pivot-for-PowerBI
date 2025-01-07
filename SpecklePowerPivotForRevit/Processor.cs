using Objects.Other.Revit;
using Speckle.Core.Models;
using Speckle.Core.Models.GraphTraversal;

namespace SpecklePowerPivotForRevit;

public static class Processor
{
  internal static readonly HashSet<string> PropsToSkip = CreatePropsToSkip(
    DefaultTraversal.ElementsPropAliases,
    new[] { "id", "applicationId", "referencedId", "elementId", },
    new[] { "speckle_type", "totalChildrenCount", "materialQuantities", }
  );

  internal static Base? ProcessObject(Base baseObject)
  {
    switch (baseObject)
    {
      case Collection:
        return null;
      case RevitInstance instance:
      {
        if (!AutomateFunction.ResolveInstances)
        {
          return instance;
        }

        foreach (
          var (definitionPropKey, definitionPropValue) in instance.definition
            .GetMembers()
            .ToList()
        )
        {
          if (PropsToSkip.Contains(definitionPropKey))
          {
            continue; // Skip merging for specified properties
          }

          if (!instance.IsPropNameValid(definitionPropKey, out _))
            continue;

          if (
            instance
              .GetMembers()
              .TryGetValue(definitionPropKey, out var instancePropValue)
          )
          {
            PropertyMerger.MergeProperties(
              instance,
              definitionPropKey,
              instancePropValue,
              definitionPropValue
            );
          }
          else
          {
            ((dynamic)instance)[definitionPropKey] = definitionPropValue;
          }
        }
        return instance;
      }
      default:
        return baseObject;
    }
  }

  private static HashSet<string> CreatePropsToSkip(
    params IEnumerable<string>[] propertyLists
  )
  {
    return new HashSet<string>(propertyLists.SelectMany(list => list));
  }
}
