using System.ComponentModel;

namespace SpecklePowerPivotForRevit;

/// <summary>
/// Represents the user-configurable inputs for the automation.
///
/// KEY CONCEPT: Input Validation and Schema Generation
/// This class demonstrates how to use C# attributes to:
/// 1. Generate JSON schemas for validation
/// 2. Provide default values
/// 3. Document expected inputs
///
/// This pattern is useful whenever you need to validate external inputs,
/// whether from users or other systems.
/// </summary>
public struct FunctionInputs
{
  /// <summary>
  /// Specifies the prefix for the output branch.
  ///
  /// The DefaultValue attribute shows how to provide fallback values,
  /// making your automation more robust and user-friendly.
  /// </summary>
  [DefaultValue("bi-ready")]
  public string TargetModelPrefix { get; set; }
}
