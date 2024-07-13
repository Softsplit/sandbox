using Sandbox.UI;
using Sandbox.UI.Construct;

public class Health : Panel
{
	public Label Label;

	public Health()
	{
		Label = Add.Label( "100", "value" );
	}

	public override void Tick()
	{
		var player = Game.ActiveScene.GetAllComponents<Player>().Where( player => !player.IsProxy ).FirstOrDefault();
		if ( player == null ) return;

		Label.Text = $"{player.Health.CeilToInt()}";
	}
}
