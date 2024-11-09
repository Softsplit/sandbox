[Library( "tool_color", Title = "Color", Description = "Change render color and alpha of entities", Group = "construction" )]
public partial class ColorTool : BaseTool
{
	public override bool Primary( SceneTraceResult trace )
	{
		if ( Input.Pressed( "attack1" ) )
		{
			if ( !trace.Hit || !trace.GameObject.IsValid() )
				return false;

			if ( !trace.GameObject.Components.TryGet<PropHelper>( out var propHelper ) )
				return false;

			BroadcastColor( propHelper, Color.Random );

			return true;
		}

		return false;
	}

	public override bool Secondary( SceneTraceResult trace )
	{
		if ( Input.Pressed( "attack2" ) )
		{
			if ( !trace.Hit || !trace.GameObject.IsValid() )
				return false;

			if ( !trace.GameObject.Components.TryGet<PropHelper>( out var propHelper ) )
				return false;

			BroadcastColor( propHelper, Color.White );

			return true;
		}

		return false;
	}

	[Broadcast]
	private void BroadcastColor( PropHelper propHelper, Color color )
	{
		propHelper.Prop.Tint = color;
	}
}
