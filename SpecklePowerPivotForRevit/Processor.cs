using Objects.Other.Revit;
using Speckle.Core.Models;
using Speckle.Core.Models.GraphTraversal;

namespace SpecklePowerPivotForRevit;

/// <summary>
/// Handles object processing and property management.
/// 
/// KEY CONCEPT: Immutable State and Pure Functions
/// This class demonstrates functional programming principles by:
/// 1. Using immutable state (readonly fields)
/// 2. Implementing pure functions that don't modify their inputs
/// 3. Using LINQ for declarative data transformations
/// </summary>
public static class Processor
{
    /// <summary>
    /// Properties that should be skipped during processing.
    /// Using a HashSet provides O(1) lookup time for better performance.
    /// </summary>
    public static readonly HashSet<string> PropsToSkip = CreatePropsToSkip(
        DefaultTraversal.ElementsPropAliases,
        ["id", "applicationId", "referencedId", "elementId"],
        ["speckle_type", "totalChildrenCount", "materialQuantities"]
    );

    /// <summary>
    /// Processes a base object, handling different types appropriately.
    /// 
    /// PATTERN: Type-Based Processing
    /// Shows how to:
    /// 1. Use pattern matching for type-specific logic
    /// 2. Handle different object types consistently
    /// 3. Transform data while maintaining type safety
    /// </summary>
    public static Base? ProcessObject(Base baseObject)
    {
        switch (baseObject)
        {
            case Collection:
                return null;
            case RevitInstance instance:
            {
                // Handle Revit instances specially
                if (instance.definition["speckle_type"] is string definitionType)
                {
                    instance["effective_speckle_type"] = definitionType;
                }

                // Process definition properties
                foreach (var (key, value) in instance.definition
                    .GetMembers(DynamicBaseMemberType.All)
                    .ToList())
                {
                    if (PropsToSkip.Contains(key)) continue;
                    if (!instance.IsPropNameValid(key, out _)) continue;

                    // Check if property exists and merge appropriately
                    if (instance.GetMembers(DynamicBaseMemberType.All)
                        .TryGetValue(key, out var instanceValue))
                    {
                        PropertyMerger.MergeProperties(
                            instance, key, instanceValue, value, PropsToSkip);
                    }
                    else
                    {
                        ((dynamic)instance)[key] = value;
                    }
                }

                return instance;
            }
            default:
                // Handle standard base objects
                if (baseObject["speckle_type"] is string type)
                {
                    baseObject["effective_speckle_type"] = type;
                }
                return baseObject;
        }
    }


    /// <summary>
    /// Creates a HashSet of properties to skip during processing.
    /// 
    /// PATTERN: Flexible Configuration
    /// Shows how to:
    /// 1. Use params for flexible method signatures
    /// 2. Combine multiple collections efficiently
    /// 3. Create immutable configurations
    /// </summary>
    private static HashSet<string> CreatePropsToSkip(
        params IEnumerable<string>[] propertyLists)
    {
        return [..propertyLists.SelectMany(list => list)];
    }
}