public class Physgun : Tool
{
	GameObject movable_object;
	float distance;
	public override void OnUpdate()
	{
		base.OnUpdate();
		if (Input.Pressed("attack1"))
		{
			var trace = DoTrace();
			if ( trace.Hit )
			{
				movable_object = trace.GameObject;
				distance = trace.Distance;
			}
		}
		if ( Input.Released( "attack1" ) )
		{
			movable_object = null;
		}
		if (movable_object != null && Input.Down("attack1") )
		{
			var startPos = Owner.EyePosition;
			var dir = Owner.EyeRotation.Forward;
			movable_object.Transform.LerpTo( new global::Transform( startPos + dir * distance ), 0.2f );
		}
	}
}
