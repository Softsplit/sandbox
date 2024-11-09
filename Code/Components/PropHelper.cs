using Sandbox.ModelEditor.Nodes;

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
	[Property, Sync] public bool Invincible { get; set; } = false;

	[RequireComponent] public Prop Prop { get; set; }

	public ModelPhysics ModelPhysics { get; set; }
	public Rigidbody Rigidbody { get; set; }
	public NetDictionary<int, BodyInfo> NetworkedBodies { get; set; } = new();

	public List<FixedJoint> Welds { get; set; } = new();
	public List<Joint> Joints { get; set; } = new();

	private Vector3 lastPosition = Vector3.Zero;

	protected override void OnStart()
	{
		ModelPhysics ??= Components.Get<ModelPhysics>( FindMode.EverythingInSelf );
		Rigidbody ??= GetComponent<Rigidbody>();

		Health = Prop?.Health ?? 0f;
		Velocity = 0f;

		lastPosition = Prop?.WorldPosition ?? WorldPosition;
	}

	[Broadcast]
	public void Damage( float amount )
	{
		if ( !Prop.IsValid() )
			return;

		if ( (Prop?.Health ?? 0f) <= 0f )
			return;

		if ( IsProxy )
			return;

		Health -= amount;

		if ( Health <= 0f && !Invincible )
			Kill();
	}

	bool dead;

	public void Kill()
	{
		if ( IsProxy )
			return;

		if ( dead )
			return;

		dead = true;

		if ( !Prop.IsValid() )
			return;

		var gibs = Prop?.CreateGibs();
		if ( gibs == null )
			return;

		foreach ( var gib in gibs )
		{
			if ( !gib.IsValid() )
				continue;

			gib.Tint = Prop.Tint;
			gib.AddComponent<PropHelper>();
			gib.Tags.Add( "debris" );
			gib.GameObject.NetworkSpawn();
			gib.Network.SetOrphanedMode( NetworkOrphaned.Host );
		}

		if ( Prop.Model.TryGetData<ModelExplosionBehavior>( out var data ) )
		{
			Explosion( data.Effect, data.Sound, WorldPosition, data.Radius, data.Damage, data.Force );
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

	private ModelPropData GetModelPropData()
	{
		if ( Prop.Model.IsValid() && !Prop.Model.IsError && Prop.Model.TryGetData( out ModelPropData propData ) )
		{
			return propData;
		}

		ModelPropData defaultData = new()
		{
			Health = -1,
		};

		return defaultData;
	}

	void ICollisionListener.OnCollisionStart( Collision collision )
	{
		if ( IsProxy )
			return;

		var propData = GetModelPropData();
		if ( propData == null ) return;

		var minImpactSpeed = 500;
		if ( minImpactSpeed <= 0.0f ) minImpactSpeed = 500;

		var impactDmg = Rigidbody.IsValid() ? Rigidbody.Mass / 10 : ModelPhysics.PhysicsGroup.Mass / 10;
		if ( impactDmg <= 0.0f ) impactDmg = 10;

		float speed = collision.Contact.Speed.Length;

		if ( speed > minImpactSpeed )
		{
			// I take damage from high speed impacts
			if ( Health > 0 )
			{
				var damage = speed / minImpactSpeed * impactDmg;
				Damage( damage );
			}

			var other = collision.Other;

			// Whatever I hit takes more damage
			if ( other.GameObject.IsValid() && other.GameObject != GameObject )
			{
				var damage = speed / minImpactSpeed * impactDmg * 1.2f;

				if ( other.GameObject.Components.TryGet<PropHelper>( out var propHelper ) )
				{
					propHelper.Damage( damage );
				}
				else if ( other.GameObject.Components.TryGet<Player>( out var player ) )
				{
					player.TakeDamage( damage );
				}
			}
		}
	}

	public async void Explosion( string particle, string sound, Vector3 position, float radius, float damage, float forceScale )
	{
		await GameTask.Delay( Game.Random.Next( 50, 250 ) );

		var soundEvent = ResourceLibrary.Get<SoundEvent>( sound );

		if ( sound != null )
			BroadcastExplosion( soundEvent.IsValid() ? sound : "rust_pumpshotgun.shootdouble", position );

		Particles.CreateParticleSystem( particle, new Transform( position, Rotation.Identity ), 10 );

		// Damage, etc
		var overlaps = Game.ActiveScene.FindInPhysics( new Sphere( position, radius ) );

		foreach ( var obj in overlaps )
		{
			if ( !obj.Tags.Intersect( new TagSet() { "solid", "player", "npc", "glass" } ).Any() && obj.Tags.Intersect( new TagSet() { "player_hull" } ).Any() )
			{
				continue;
			}

			// If the object isn't in line of sight, fuck it off
			var tr = Game.ActiveScene.Trace.Ray( position, obj.WorldPosition )
				.WithoutTags( new TagSet() { "solid", "player", "npc", "glass" }.Append( "solid" ).ToArray() )
				.Run();

			if ( tr.Hit && tr.GameObject.IsValid() )
			{
				if ( !obj.Root.IsDescendant( tr.GameObject ) )
					continue;
			}

			var distance = tr.Hit ? tr.Distance : obj.WorldPosition.Distance( position );
			var distanceMul = 1.0f - Math.Clamp( distance / radius, 0.0f, 1.0f );

			var dmg = damage * distanceMul;

			foreach ( var propHelper in obj.Components.GetAll<PropHelper>().ToArray() )
			{
				if ( !propHelper.IsValid() ) continue;

				propHelper.Damage( dmg );
			}

			var force = (obj.WorldPosition - position).Normal * distanceMul * forceScale * 10000f;

			if ( obj.GetComponent<Player>().IsValid() )
				obj.GetComponent<Player>()?.TakeDamage( dmg );

			if ( obj.GetComponent<Rigidbody>().IsValid() )
				obj.GetComponent<Rigidbody>()?.ApplyImpulse( force );

			if ( obj.GetComponent<ModelPhysics>().IsValid() )
				obj.GetComponent<ModelPhysics>()?.PhysicsGroup.ApplyImpulse( force );
		}
	}

	[Broadcast]
	public void BroadcastExplosion( string path, Vector3 position )
	{
		Sound.Play( path, position );
	}


	[Broadcast]
	public void Weld( GameObject to )
	{
		PropHelper propHelper = to.Components.Get<PropHelper>();

		var fixedJoint = Components.Create<FixedJoint>();
		fixedJoint.Body = to;
		fixedJoint.LinearDamping = 0;
		fixedJoint.LinearFrequency = 0;
		fixedJoint.AngularDamping = 0;
		fixedJoint.AngularFrequency = 0;

		Welds.Add( fixedJoint );
		Joints.Add( fixedJoint );
		propHelper?.Welds.Add( fixedJoint );
		propHelper?.Joints.Add( fixedJoint );
	}

	[Broadcast]
	public void UnWeld()
	{
		foreach ( var weld in Welds )
		{
			weld?.Destroy();
		}

		Welds.RemoveAll( item => !item.IsValid() );
		Joints.RemoveAll( item => !item.IsValid() );
	}

	[Broadcast]
	public void Hinge( GameObject to, Vector3 position, Vector3 normal )
	{
		if ( !to.IsValid() ) return;

		PropHelper propHelper = to.Components.Get<PropHelper>();

		var go = new GameObject
		{
			WorldPosition = position,
			WorldRotation = Rotation.LookAt( Rotation.LookAt( normal ).Up )
		};

		go.SetParent( to );

		var hingeJoint = go.Components.Create<HingeJoint>();
		hingeJoint.Body = GameObject;

		Joints.Add( hingeJoint );

		propHelper?.Joints.Add( hingeJoint );
	}
}
