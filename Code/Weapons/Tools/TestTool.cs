using Sandbox.Events;

namespace Softsplit;

public sealed class TestTool : ToolComponent
{
    TestToolMenu values;
	protected override void OnAwake()
	{
		values = Components.Get<TestToolMenu>();
	}

	protected override void PrimaryAction()
	{
        Log.Info(values.ThingToLog);
	}

    protected override void SecondaryAction()
	{
        Log.Info(values.ThingToLog.ToUpper());
	}
}