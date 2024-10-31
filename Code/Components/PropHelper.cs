using Sandbox.Physics;

/// <summary>
/// A component to help deal with props.
/// </summary>
public sealed class PropHelper : Component, Component.ICollisionListener
{
	public struct BodyInfo
	{
		public PhysicsBodyType Type { get; set; }
		public Transform Transform { get; set; }
	}

	[Property, Sync] public float Health { get; set; } = 1f;
	[Property, Sync] public Vector3 Velocity { get; set; }

	[Sync] public Prop Prop { get; set; }
	[Sync] public ModelPhysics ModelPhysics { get; set; }
	[Sync] public Rigidbody Rigidbody { get; set; }
	[Sync] public NetDictionary<int, BodyInfo> NetworkedBodies { get; set; } = new();

	public List<Sandbox.Physics.FixedJoint> Welds { get; set; } = new();
	public List<PhysicsJoint> Joints { get; set; } = new();

	private Vector3 lastPosition = Vector3.Zero;

	protected override void OnStart()
	{
		Health = Prop?.Health ?? 0f;
		Velocity = 0f;

		lastPosition = Prop?.WorldPosition ?? WorldPosition;
	}

	[Broadcast]
	public void Damage( float amount )
	{
		if ( (Prop?.Health ?? 0f) <= 0f )
			return;

		if ( IsProxy )
			return;

		Health -= amount;

		if ( Health <= 0f )
			Kill();
	}

	public void Kill()
	{
		if ( IsProxy )
			return;

		var gibs = Prop?.CreateGibs();

		foreach ( var gib in gibs )
		{
			gib.Tags.Add( "debris" );
			gib.GameObject.NetworkSpawn();
			gib.Network.SetOrphanedMode( NetworkOrphaned.Host );
		}

		GameObject.DestroyImmediate();
	}

	protected override void OnFixedUpdate()
	{
		if ( Prop.IsValid() )
		{
			Velocity = (Prop.WorldPosition - lastPosition) / Time.Delta;
			lastPosition = Prop.WorldPosition;
		}

		UpdateNetworkedBodies();
	}

	private void UpdateNetworkedBodies()
	{
		if ( !ModelPhysics.IsValid() )
		{
			ModelPhysics = Components.Get<ModelPhysics>( FindMode.EverythingInSelf );
			Rigidbody = GetComponent<Rigidbody>();

			return;
		}

		if ( !Network.IsOwner )
		{
			var rootBody = FindRootBody();

			foreach ( var (groupId, info) in NetworkedBodies )
			{
				var group = ModelPhysics.PhysicsGroup.GetBody( groupId );
				if ( !group.IsValid() ) continue;

				group.Transform = info.Transform;
				group.BodyType = info.Type;
			}

			if ( rootBody.IsValid() )
				rootBody.Transform = ModelPhysics.Renderer.GameObject.WorldTransform;

			return;
		}

		foreach ( var body in ModelPhysics.PhysicsGroup.Bodies )
		{
			if ( body.GroupIndex == 0 )
				continue;

			var tx = body.GetLerpedTransform( Time.Now );
			NetworkedBodies[body.GroupIndex] = new BodyInfo
			{
				Type = body.BodyType,
				Transform = tx
			};
		}
	}

	private PhysicsBody FindRootBody()
	{
		var body = ModelPhysics.PhysicsGroup.Bodies.FirstOrDefault();
		if ( body == null )
			return null;

		while ( body.Parent.IsValid() )
			body = body.Parent;

		return body;
	}

	void ICollisionListener.OnCollisionStart( Collision collision )
	{
		if ( IsProxy )
			return;

		float minDamageVelocity = 750f;
		float relativeVelocity = collision.Contact.Speed.Length;

		if ( relativeVelocity > minDamageVelocity )
		{
			float impactForce = collision.Other.Body.Mass * relativeVelocity;
			float damage = (impactForce - minDamageVelocity) * 8f;

			Damage( damage );

			if ( collision.Other.GameObject.Components.TryGet<PropHelper>( out var prop ) )
			{
				prop.Damage( damage );
			}
			else if ( collision.Other.GameObject.Components.TryGet<Player>( out var player ) )
			{
				player.TakeDamage( damage );
			}
		}
	}

	[Broadcast]
	public void Weld (GameObject to, Vector3 fromPoint, Vector3 toPoint)
	{
		PropHelper propHelper = to.Components.Get<PropHelper>();
		Rigidbody connectedBody = to.Components.Get<Rigidbody>();
		
		if(!connectedBody.IsValid())
			return;

		var point1 = new PhysicsPoint(Rigidbody.PhysicsBody);
		var point2 = new PhysicsPoint(connectedBody.PhysicsBody, connectedBody.WorldTransform.PointToLocal(Rigidbody.PhysicsBody.MassCenter), connectedBody.WorldTransform.RotationToLocal(Rigidbody.WorldRotation));
		var fixedJoint = PhysicsJoint.CreateFixed(point1,point2);
		
		Welds.Add(fixedJoint);
		Joints.Add(fixedJoint);
		propHelper?.Welds.Add(fixedJoint);
		propHelper?.Joints.Add(fixedJoint);
	}

	[Broadcast]
	public void UnWeld()
	{
		foreach(var weld in Welds)
		{
			weld?.Remove();
		}

		Welds.RemoveAll(item => !item.IsValid());
		Joints.RemoveAll(item => !item.IsValid());
	}
}
