using System.Collections;
using Speckle.Core.Models;

namespace SpecklePowerPivotForRevit;

public static class PropertyMerger
{
  public static void MergeProperties(
    Base target,
    string key,
    object? targetValue,
    object? sourceValue
  )
  {
    if (sourceValue == null)
    {
      return; // No need to set anything if source value is null
    }

    if (targetValue == null)
    {
      target[key] = sourceValue;
      return;
    }

    switch (targetValue)
    {
      case Base targetBase when sourceValue is Base sourceBase:
        MergeBaseObjects(targetBase, sourceBase);
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

  private static void MergeBaseObjects(Base targetBase, Base sourceBase)
  {
    foreach (var kvp in sourceBase.GetMembers())
    {
      var newKey = AutomateFunction.PrefixMergedDefinitionProperties
        ? $"d_{kvp.Key}"
        : kvp.Key;
      targetBase[newKey] = kvp.Value;
    }
  }

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
          $"Source list item type {item.GetType()} is not assignable to target list type {targetType}"
        );
      }
    }
  }

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
            $"Source dictionary key-value types {entry.Key.GetType()}-{entry.Value.GetType()} are not assignable to target dictionary types {targetKeyType}-{targetValueType}"
          );
      }
    }
  }
}
