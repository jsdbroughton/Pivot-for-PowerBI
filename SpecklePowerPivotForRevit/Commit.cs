using Speckle.Core.Models;

namespace SpecklePowerPivotForRevit;

public static class Commit
{
  /// <summary>
  /// Gets a new instance of a commit object with initial properties.
  /// </summary>
  internal static Collection CommitObject(
    List<Base> objects,
    string title = "Pivoted Revit model"
  )
  {
    return new Collection()
    {
      collectionType = "pivoted model",
      name = title,
      elements = objects
    };
  }
}
