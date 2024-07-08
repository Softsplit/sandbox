public class Physgun : Tool
{
	GameObject moveableObject;
	float distance;

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!IsUsing())
		{
			return;
		}

		if ( Input.Pressed( "attack1" ) )
		{
			var trace = DoTrace();
			if ( trace.Hit )
			{
				var prop = trace.GameObject.Components.Get<Prop>();
				if ( prop != null )
				{
					moveableObject = trace.GameObject;
					distance = trace.Distance;
					prop.IsStatic = true;
				}
				moveableObject = trace.GameObject;
				distance = trace.Distance;
			}
		}

		if ( Input.Released( "attack1" ) )
		{
			moveableObject = null;
		}

		if ( moveableObject != null && Input.Down("attack1") )
		{
			var startPos = Owner.EyePosition;
			var dir = Owner.EyeRotation.Forward;

			// This doesn't retain the object's velocity
			moveableObject.Transform.LerpTo( new Transform( startPos + dir * distance ), 0.4f );
		}
	}
}
