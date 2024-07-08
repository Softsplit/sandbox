public class Physgun : Tool
{
	GameObject moveableObject;
	float distance;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Input.Pressed( "attack1" ) )
		{
			var trace = DoTrace();
			if ( trace.Hit )
			{
				moveableObject = trace.GameObject;
				distance = trace.Distance;
			}
		}

		if ( Input.Released( "attack1" ) )
		{
			moveableObject = null;
		}

		if ( moveableObject != null && Input.Down( "attack1" ) )
		{
			var startPos = Owner.EyePosition;
			var dir = Owner.EyeRotation.Forward;

			// This doesn't retain the object's velocity
			moveableObject.Transform.LerpTo( new Transform( startPos + dir * distance ), 0.2f );
		}
	}
}
