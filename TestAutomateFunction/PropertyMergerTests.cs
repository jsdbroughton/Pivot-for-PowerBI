using Speckle.Core.Models;
using SpecklePowerPivotForRevit;

namespace TestAutomateFunction;

[TestFixture]
public class PropertyMergerTests
{
    private HashSet<string> _propsToSkip;

    [SetUp]
    public void Setup()
    {
        _propsToSkip =
        [
            "skipMe",
            "speckle_type",
            "id",
            "totalChildrenCount"
        ];
    }

    [Test]
    public void MergeProperties_WithNullSourceValue_DoesNotModifyTarget()
    {
        // Arrange
        var target = new Base
        {
            ["testProp"] = "originalValue"
        };

        // Act
        PropertyMerger.MergeProperties(target, "testProp", "originalValue", null, _propsToSkip);

        // Assert
        Assert.That(target["testProp"], Is.EqualTo("originalValue"));
    }

    [Test]
    public void MergeProperties_WithNullTargetValue_SetsSourceValue()
    {
        // Arrange
        var target = new Base();
        var sourceValue = "newValue";

        // Act
        PropertyMerger.MergeProperties(target, "testProp", null, sourceValue, _propsToSkip);

        // Assert
        Assert.That(target["testProp"], Is.EqualTo(sourceValue));
    }

    [Test]
    public void MergeProperties_WithBaseObjects_MergesCorrectly()
    {
        // Arrange
        var target = new Base();
        var targetBase = new Base
        {
            ["userProperty1"] = "value1" // Using a custom property name that's not restricted
        };
        target["baseObj"] = targetBase;

        var sourceBase = new Base
        {
            ["userProperty2"] = "value2" // Using a custom property name that's not restricted
        };

        // Act
        PropertyMerger.MergeProperties(target, "baseObj", targetBase, sourceBase, _propsToSkip);

        // Assert
        var resultBase = target["baseObj"] as Base;
        Assert.That(resultBase, Is.Not.Null);
        Assert.That(resultBase!["userProperty1"], Is.EqualTo("value1"));
        Assert.That(resultBase["userProperty2"], Is.EqualTo("value2"));
    }

    [Test]
    public void MergeProperties_WithLists_MergesCorrectly()
    {
        // Arrange
        var target = new Base();
        var targetList = new List<string> { "item1" };
        target["listProp"] = targetList;

        var sourceList = new List<string> { "item2" };

        // Act
        PropertyMerger.MergeProperties(target, "listProp", targetList, sourceList, _propsToSkip);

        // Assert
        var resultList = target["listProp"] as List<string>;
        Assert.That(resultList, Is.Not.Null);
        Assert.That(resultList, Has.Count.EqualTo(2));
        Assert.That(resultList, Does.Contain("item1"));
        Assert.That(resultList, Does.Contain("item2"));
    }

    [Test]
    public void MergeProperties_WithDictionaries_MergesCorrectly()
    {
        // Arrange
        var target = new Base();
        var targetDict = new Dictionary<string, string> { ["key1"] = "value1" };
        target["dictProp"] = targetDict;

        var sourceDict = new Dictionary<string, string> { ["key2"] = "value2" };

        // Act
        PropertyMerger.MergeProperties(target, "dictProp", targetDict, sourceDict, _propsToSkip);

        // Assert
        var resultDict = target["dictProp"] as Dictionary<string, string>;
        Assert.That(resultDict, Is.Not.Null);
        Assert.That(resultDict, Has.Count.EqualTo(2));
        Assert.That(resultDict["key1"], Is.EqualTo("value1"));
        Assert.That(resultDict["key2"], Is.EqualTo("value2"));
    }

    [Test]
    public void MergeProperties_WithRestrictedProperties_ThrowsException()
    {
        // Arrange
        var target = new Base();
        var targetBase = new Base() { applicationId = "original" }; // Using a valid property for setup
        target["baseObj"] = targetBase;

        var sourceBase = new Base();
        var propsToSkip = new HashSet<string> { "speckle_type" };

        // Act & Assert
        Assert.That(() => 
                PropertyMerger.MergeProperties(target, "speckle_type", "originalType", "newType", propsToSkip),
            Throws.TypeOf<Speckle.Core.Logging.SpeckleException>()
                .With.Message.Contains("speckle_type")
        );
    }
}