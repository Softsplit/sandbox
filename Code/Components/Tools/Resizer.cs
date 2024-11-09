// TODO: I can't get this to work with ModelPhysics so I'm leaving it here
// for someone else

/*
[Library( "tool_resizer", Title = "Resizer", Description = "Change the scale of things", Group = "construction" )]
public partial class ResizerTool : BaseTool
{
	public override bool Primary( SceneTraceResult trace )
	{
		int resizeDir = 0;
		var reset = false;

		if ( Input.Down( "attack1" ) ) resizeDir = 1;
		else if ( Input.Down( "attack2" ) ) resizeDir = -1;
		else if ( Input.Pressed( "reload" ) ) reset = true;
		else return false;

		if ( !trace.Hit || !trace.GameObject.IsValid() )
			return false;

		var go = trace.GameObject.Root;
		if ( !go.IsValid() )
			return false;

		if ( !go.Components.TryGet<PropHelper>( out var propHelper ) )
			return false;

		var scale = reset ? 1.0f : new Vector3(
			MathX.Clamp( go.WorldScale.x + (0.5f * Time.Delta * resizeDir), 0.4f, 4.0f ),
			MathX.Clamp( go.WorldScale.y + (0.5f * Time.Delta * resizeDir), 0.4f, 4.0f ),
			MathX.Clamp( go.WorldScale.z + (0.5f * Time.Delta * resizeDir), 0.4f, 4.0f )
		);

		if ( go.WorldScale != scale )
		{
			go.WorldScale = scale;

			if ( !propHelper.Rigidbody.IsValid() || !propHelper.ModelPhysics.IsValid() )
				return false;

			if ( propHelper.Rigidbody.IsValid() )
			{
				propHelper.Rigidbody.PhysicsBody.RebuildMass();
				propHelper.Rigidbody.PhysicsBody.Sleeping = false;
			}

			if ( propHelper.ModelPhysics.IsValid() )
			{
				propHelper.ModelPhysics.PhysicsGroup.RebuildMass();
				propHelper.ModelPhysics.PhysicsGroup.Sleeping = false;
			}

			foreach ( var child in go.Children )
			{
				if ( !child.IsValid() )
					continue;

				if ( go.Components.TryGet<PropHelper>( out var childPropHelper ) )
					return false;

				if ( !childPropHelper.Rigidbody.IsValid() || !childPropHelper.ModelPhysics.IsValid() )
					continue;

				if ( childPropHelper.Rigidbody.IsValid() )
				{
					childPropHelper.Rigidbody.PhysicsBody.RebuildMass();
					childPropHelper.Rigidbody.PhysicsBody.Sleeping = false;
				}

				if ( childPropHelper.ModelPhysics.IsValid() )
				{
					childPropHelper.ModelPhysics.PhysicsGroup.RebuildMass();
					childPropHelper.ModelPhysics.PhysicsGroup.Sleeping = false;
				}
			}
		}

		if ( Input.Pressed( "attack1" ) || Input.Pressed( "attack2" ) || reset )
		{
			return true;
		}

		return false;
	}
}
*/
