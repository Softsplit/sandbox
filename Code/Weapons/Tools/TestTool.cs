using Sandbox.Events;

namespace Softsplit;

public sealed class TestTool : ToolComponent
{
    TestToolMenu values;
	protected override void OnStart()
	{
		values = Components.Get<TestToolMenu>();
        values.Enabled = false;
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