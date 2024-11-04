[Library( "tool_remover", Description = "Remove entities", Group = "construction" )]
public class Remover : BaseTool
{
	public override bool Primary( SceneTraceResult trace )
	{
		if ( trace.GameObject.Components.Get<PropHelper>() == null )
			return false;

		Remove( trace.GameObject );

		return true;
	}

	[Broadcast]
	void Remove( GameObject g )
	{
		g.Destroy();

		Particles.MakeParticleSystem( "particles/physgun_freeze.vpcf", g.WorldTransform );
	}
}
