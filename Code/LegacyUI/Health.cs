using Sandbox.UI.Construct;
using Softsplit;

public class Health : Panel
{
	public Label Label;

	public Health()
	{
		Label = Add.Label( "100", "value" );
	}

	public override void Tick()
	{
		var player = PlayerState.Local;

		Label.Text = $"{player?.PlayerPawn?.HealthComponent?.Health.CeilToInt()}";
	}
}
