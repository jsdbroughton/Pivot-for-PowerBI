using Speckle.Core.Api.GraphQL.Models;
using SpecklePowerPivotForRevit;

namespace TestAutomateFunction;

using Speckle.Automate.Sdk;
using Speckle.Automate.Sdk.Test;
using Speckle.Core.Api;
using Speckle.Core.Credentials;

[TestFixture]
public sealed class AutomationContextTest : IDisposable
{
  private Client _client;
  private Account _account;

  [OneTimeSetUp]
  public void Setup()
  {
    _account = new Account
    {
      token = TestAutomateEnvironment.GetSpeckleToken(),
      serverInfo = new ServerInfo
      {
        url = TestAutomateEnvironment.GetSpeckleServerUrl().ToString()
      }
    };
    _client = new Client(_account);
  }

  [Test]
  public async Task TestFunctionRun()
  {
    var inputs = new FunctionInputs
    {
      // ResolveInstances = true,
      // PropagateNamedProperties = true,
      TargetModelPrefix = "bi-ready",
      // PrefixMergedDefinitionProperties = true
    };

    var automationRunData = await TestAutomateUtils.CreateTestRun(_client);
    var automationContext = await AutomationRunner.RunFunction(
      AutomateFunction.Run,
      automationRunData,
      _account.token,
      inputs
    );

    Assert.That(automationContext.RunStatus, Is.EqualTo("SUCCEEDED"));
  }

  public void Dispose()
  {
    _client.Dispose();
    TestAutomateEnvironment.Clear();
  }
}
