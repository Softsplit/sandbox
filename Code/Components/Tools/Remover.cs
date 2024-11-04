[Library( "tool_remover", Description = "Remove entities", Group = "construction" )]
public class Remover : BaseTool
{
	public override bool Primary( SceneTraceResult trace )
	{
		if ( Input.Pressed( "attack1" ) )
		{
			if ( trace.GameObject.Components.Get<PropHelper>() == null )
				return false;

			Remove( trace.GameObject );
			Parent.ViewModel.Renderer.Set( "b_attack", true );

			return true;
		}

		return false;
	}

	[Broadcast]
	void Remove( GameObject g )
	{
		g.Destroy();

		Particles.MakeParticleSystem( "particles/physgun_freeze.vpcf", g.WorldTransform );
		Owner?.Controller?.Renderer?.Set( "b_attack", true );
	}
}
