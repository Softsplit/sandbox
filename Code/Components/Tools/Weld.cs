[Library( "tool_weld", Description = "Weld stuff together", Group = "construction" )]
public class Weld : BaseTool
{
	GameObject welded;

	public override bool Primary( SceneTraceResult trace )
	{
		if ( !trace.Hit )
			return false;

		if ( Input.Pressed( "attack1" ) && trace.GameObject.Components.TryGet<PropHelper>( out var propHelper ) )
		{
			if ( welded == null )
			{
				welded = trace.GameObject;

				Parent.ViewModel.Renderer.Set( "b_attack", true );
				BroadcastAttack();

				return true;
			}

			if ( trace.GameObject == welded )
				return false;

			propHelper.Weld( welded );

			Parent.ViewModel.Renderer.Set( "b_attack", true );
			BroadcastAttack();

			welded = null;
			return true;
		}

		return false;
	}

	public override bool Secondary( SceneTraceResult trace )
	{
		if ( !trace.Hit )
			return false;

		if ( Input.Pressed( "attack2" ) && trace.GameObject.Components.TryGet<PropHelper>( out var propHelper ) )
		{
			propHelper.UnWeld();

			Parent.ViewModel.Renderer.Set( "b_attack", true );
			BroadcastAttack();

			welded = null;
			return true;
		}

		return false;
	}

	[Broadcast]
	private void BroadcastAttack()
	{
		Owner?.Controller?.Renderer?.Set( "b_attack", true );
	}
}
