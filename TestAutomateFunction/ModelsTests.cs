using SpecklePowerPivotForRevit;

namespace TestAutomateFunction;

[TestFixture]
public class ModelsTests
{
  [TestCase("model1", "bi-ready", "bi-ready/model1")]
  [TestCase("folder/model1", "bi-ready", "bi-ready/folder/model1")]
  [TestCase("folder/subfolder/model1", "bi-ready", "bi-ready/folder/subfolder/model1")]
  [TestCase("model1", "/bi-ready", "bi-ready/model1")]
  [TestCase("folder/model1", "/bi-ready/", "bi-ready/folder/model1")]
  public void GenerateTargetModelName_ValidInputs_ReturnsExpectedPath(
    string sourceModelName,
    string prefix,
    string expected
  )
  {
    // Act
    var result = Models.GenerateTargetModelName(sourceModelName, prefix);

    // Assert
    Assert.That(result, Is.EqualTo(expected));
  }

  [TestCase("", "b-ready")]
  [TestCase("", "b-ready/")]
  [TestCase("model1", "")]
  [TestCase("model1", "/")]
  public void GenerateTargetModelName_InvalidInputs_ThrowsArgumentException(
    string sourceModelName,
    string prefix
  )
  {
    // Act & Assert
    Assert.Throws<ArgumentException>(
      () => Models.GenerateTargetModelName(sourceModelName, prefix)
    );
  }
}
