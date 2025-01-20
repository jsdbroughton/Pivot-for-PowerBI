using Objects.Other.Revit;
using Speckle.Core.Models;
using Speckle.Core.Models.GraphTraversal;

namespace SpecklePowerPivotForRevit;

public static class Processor
{
    public static readonly HashSet<string> PropsToSkip = CreatePropsToSkip(
        DefaultTraversal.ElementsPropAliases,
        ["id", "applicationId", "referencedId", "elementId"],
        ["speckle_type", "totalChildrenCount", "materialQuantities"]
    );

    public static Base? ProcessObject(Base baseObject)
    {
        switch (baseObject)
        {
            case Collection:
                return null;
            case RevitInstance instance:
            {
                // For instances, use the definition's type
                if (instance.definition["speckle_type"] is string definitionType)
                {
                    instance["effective_speckle_type"] = definitionType;
                }

                foreach (
                    var (definitionPropKey, definitionPropValue) in instance.definition
                        .GetMembers(DynamicBaseMemberType.All)
                        .ToList()
                )
                {
                    if (PropsToSkip.Contains(definitionPropKey))
                    {
                        continue; // Skip merging for specified properties
                    }


                    if (!instance.IsPropNameValid(definitionPropKey, out _))
                    {
                        continue;
                    }


                    if (
                        instance
                        .GetMembers(DynamicBaseMemberType.All)
                        .TryGetValue(definitionPropKey, out var instancePropValue)
                    )
                    {
                        PropertyMerger.MergeProperties(
                            instance,
                            definitionPropKey,
                            instancePropValue,
                            definitionPropValue,
                            PropsToSkip
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
                if (baseObject["speckle_type"] is string type)
                {
                    baseObject["effective_speckle_type"] = type;
                }
                return baseObject;
        }
    }

    private static HashSet<string> CreatePropsToSkip(
        params IEnumerable<string>[] propertyLists
    )
    {
        return [..propertyLists.SelectMany(list => list)];
    }
}