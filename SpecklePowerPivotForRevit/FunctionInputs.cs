using System.ComponentModel;

namespace SpecklePowerPivotForRevit;

/// <summary>
/// This class describes the user specified variables that the function wants to work with.
/// </summary>
/// This class is used to generate a JSON Schema to ensure that the user provided values
/// are valid and match the required schema.
public struct FunctionInputs
{
  /// <summary>
  /// Determines whether instances should be resolved. Default is true.
  /// </summary>
  [DefaultValue(true)]
  public bool ResolveInstances { get; set; }

  /// <summary>
  /// Indicates whether named properties should be propagated. Default is true.
  /// </summary>
  [DefaultValue(true)]
  public bool PropagateNamedProperties { get; set; }

  /// <summary>
  /// Indicates whether named properties should be propagated. Default is true.
  /// </summary>
  [DefaultValue(true)]
  public bool PrefixMergedDefinitionProperties { get; set; }

  /// <summary>
  /// Specifies the prefix for the output branch. Default is "PowerBI-Ready".
  /// </summary>
  [DefaultValue("bi-ready")]
  public string TargetModelPrefix { get; set; }
}
