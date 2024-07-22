using static Sandbox.Component;

namespace Softsplit;

public static class Explosion
{
	public static void AtPoint( Vector3 point, float radius, float baseDamage, Component attacker = null, Component inflictor = null, Curve falloff = default )
	{
		if ( falloff.Frames.Count == 0 )
		{
			falloff = new Curve( new Curve.Frame( 1.0f, 1.0f ), new Curve.Frame( 0.0f, 0.0f ) );
		}

		var scene = Game.ActiveScene;
		if ( !scene.IsValid() ) return;

		var objectsInArea = scene.FindInPhysics( new Sphere( point, radius ) );
		var inflictorRoot = inflictor?.GameObject?.Root;

		var trace = scene.Trace
			.WithoutTags( "trigger", "ragdoll" );

		if ( inflictorRoot.IsValid() )
			trace = trace.IgnoreGameObjectHierarchy( inflictorRoot );

		foreach ( var obj in objectsInArea )
		{
			Rigidbody rb = obj.Components.Get<Rigidbody>() ?? null;
			IDamageable pr = obj.Components.Get<IDamageable>() ?? null;
			HealthComponent hc = obj.Root.Components.Get<HealthComponent>( FindMode.EverythingInSelfAndDescendants ) ?? null;

			if ( rb == null && hc == null )
				continue;

			// If the object isn't in line of sight, fuck it off
			var tr = trace.Ray( point, obj.Transform.Position ).Run();
			if ( tr.Hit && tr.GameObject.IsValid() )
			{
				if ( !obj.Root.IsDescendant( tr.GameObject ) )
					continue;
			}

			var distance = obj.Transform.Position.Distance( point );
			var damage = baseDamage * falloff.Evaluate( distance / radius );
			var direction = (obj.Transform.Position - point).Normal;
			var force = direction * distance * 50f;

			rb?.ApplyImpulseAt( obj.Transform.Position, force * damage );
			hc?.TakeDamage( new DamageInfo( attacker, damage, inflictor, point, force, Flags: DamageFlags.Explosion ) );
			pr?.OnDamage( new Sandbox.DamageInfo( damage, attacker.GameObject, inflictor.GameObject ) );
		}
	}
}
