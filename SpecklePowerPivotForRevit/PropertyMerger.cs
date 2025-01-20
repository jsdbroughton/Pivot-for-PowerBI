using System.Collections;
using Speckle.Core.Models;

namespace SpecklePowerPivotForRevit;

/// <summary>
/// Handles merging of properties between objects.
///
/// KEY CONCEPT: Generic Property Handling
/// This class demonstrates:
/// 1. Type-safe property merging
/// 2. Recursive object processing
/// 3. Safe handling of complex data structures
/// </summary>
public static class PropertyMerger
{
  /// <summary>
  /// Merges properties from a source object into a target object.
  ///
  /// PATTERN: Recursive Object Merging
  /// Shows how to:
  /// 1. Handle different property types
  /// 2. Maintain type safety during merging
  /// 3. Handle nested objects and collections
  /// </summary>
  public static void MergeProperties(
    Base target,
    string key,
    object? targetValue,
    object? sourceValue,
    HashSet<string> PropsToSkip
  )
  {
    // Early returns for null cases
    if (sourceValue == null)
      return;

    if (targetValue == null)
    {
      target[key] = sourceValue;
      return;
    }

    // Pattern match on the property types
    switch (targetValue)
    {
      case Base targetBase when sourceValue is Base sourceBase:
        MergeBaseObjects(targetBase, sourceBase, PropsToSkip);
        break;
      case IList targetList when sourceValue is IList sourceList:
        MergeLists(targetList, sourceList);
        break;
      case IDictionary targetDict when sourceValue is IDictionary sourceDict:
        MergeDictionaries(targetDict, sourceDict);
        break;
      default:
        target[key] = sourceValue;
        break;
    }
  }

  /// <summary>
  /// Merges two Base objects, handling their properties appropriately.
  ///
  /// PATTERN: Deep Object Merging
  /// Shows how to safely merge complex objects while respecting property restrictions.
  /// </summary>
  private static void MergeBaseObjects(
    Base targetBase,
    Base sourceBase,
    HashSet<string> PropsToSkip
  )
  {
    foreach (
      var kvp in sourceBase
        .GetMembers(DynamicBaseMemberType.All)
        .Where(kvp => !PropsToSkip.Contains(kvp.Key))
    )
    {
      targetBase[kvp.Key] = kvp.Value;
    }
  }

  /// <summary>
  /// Merges two lists while maintaining type safety.
  ///
  /// PATTERN: Type-Safe Collection Merging
  /// Shows how to merge collections while ensuring type compatibility.
  /// </summary>
  private static void MergeLists(IList targetList, IList sourceList)
  {
    var targetType = targetList.GetType().GetGenericArguments()[0];
    foreach (var item in sourceList)
    {
      if (item.GetType().IsAssignableTo(targetType))
      {
        targetList.Add(item);
      }
      else
      {
        throw new ArgumentException(
          $"Source list item type {item.GetType()} "
            + $"is not assignable to target list type {targetType}"
        );
      }
    }
  }

  /// <summary>
  /// Merges two dictionaries while maintaining type safety.
  ///
  /// PATTERN: Safe Dictionary Merging
  /// Shows how to merge dictionaries while handling type mismatches gracefully.
  /// </summary>
  private static void MergeDictionaries(IDictionary targetDict, IDictionary sourceDict)
  {
    var targetKeyType = targetDict.GetType().GetGenericArguments()[0];
    var targetValueType = targetDict.GetType().GetGenericArguments()[1];

    foreach (DictionaryEntry entry in sourceDict)
    {
      if (
        entry.Key.GetType().IsAssignableTo(targetKeyType)
        && entry.Value != null
        && entry.Value.GetType().IsAssignableTo(targetValueType)
      )
      {
        targetDict[entry.Key] = entry.Value;
      }
      else
      {
        if (entry.Value != null)
          throw new ArgumentException(
            $"Source dictionary key-value types "
              + $"{entry.Key.GetType()}-{entry.Value.GetType()} "
              + $"are not assignable to target dictionary types "
              + $"{targetKeyType}-{targetValueType}"
          );
      }
    }
  }
}
