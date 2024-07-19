namespace Softsplit;

public sealed class TestTool : ToolComponent
{
	TestToolMenu values;

	protected override void Start()
	{
		ToolName = "Test Tool";
		ToolDes = "Logs a set value. Right click for ALL CAPS.";
		values = Components.Get<TestToolMenu>();
		values.Enabled = false;
	}

	protected override void PrimaryAction()
	{
		Log.Info( values.ThingToLog );
	}

	protected override void SecondaryAction()
	{
		Log.Info( values.ThingToLog.ToUpper() );
	}
}
