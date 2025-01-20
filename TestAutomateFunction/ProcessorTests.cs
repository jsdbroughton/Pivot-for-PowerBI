using Objects.Other.Revit;
using Speckle.Core.Models;
using SpecklePowerPivotForRevit;

namespace TestAutomateFunction;

[TestFixture]
public class ProcessorTests
{
  [Test]
  public void ProcessObject_WithBaseObject_SetsEffectiveSpeckleType()
  {
    // Arrange - create a Base object without setting speckle_type
    var baseObj = new Base();

    // Act
    var result = Processor.ProcessObject(baseObj);

    // Assert
    Assert.That(result, Is.Not.Null);
    // Just verify the effective_speckle_type exists
    Assert.That(
      result!.GetMembers(DynamicBaseMemberType.Dynamic).Keys,
      Does.Contain("effective_speckle_type")
    );
  }

  [Test]
  public void ProcessObject_WithCollection_ReturnsNull()
  {
    // Arrange
    var collection = new Collection();

    // Act
    var result = Processor.ProcessObject(collection);

    // Assert
    Assert.That(result, Is.Null);
  }

  [Test]
  public void CreatePropsToSkip_WithMultipleLists_CombinesAllProperties()
  {
    // Arrange & Act
    var result = Processor.PropsToSkip;

    // Assert
    Assert.That(result, Does.Contain("id"));
    Assert.That(result, Does.Contain("applicationId"));
    Assert.That(result, Does.Contain("speckle_type"));
    Assert.That(result, Does.Contain("materialQuantities"));
  }
}
