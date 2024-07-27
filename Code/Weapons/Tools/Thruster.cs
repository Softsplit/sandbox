using Sandbox.Physics;
using Softsplit.UI;

namespace Softsplit;
public class Thruster : ToolComponent
{
	ThrusterMenu thrusterMenu;
	protected override void Start()
	{
		thrusterMenu = Components.Get<ThrusterMenu>(true);
		ToolName = "Thruster";
		ToolDes = "Create Thrusters";
	}

	protected override void PrimaryAction()
	{
		if(IsProxy) return;

		var aim = Trace();
		if ( aim.GameObject == null )
			return;
		if ( aim.Body == null )
			return;
		if ( aim.Body.BodyType == PhysicsBodyType.Static || aim.GameObject.Components.GetInChildrenOrSelf<ThrusterEntity>() != null )
			return;
		GameObject thruster = new GameObject();
		thruster.Transform.Position = aim.HitPosition;
		thruster.Transform.Rotation = Rotation.LookAt( aim.Normal ) * Rotation.From( new Angles( 90, 0, 0 ) );

		ModelRenderer modelRenderer = thruster.Components.Create<ModelRenderer>();
		modelRenderer.Model = Model.Load( "models/thruster/thrusterprojector.vmdl" );

		ModelCollider modelCollider = thruster.Components.Create<ModelCollider>();
		modelCollider.Model = Model.Load( "models/thruster/thrusterprojector.vmdl" );

		ThrusterEntity thrusterEntity = thruster.Components.Create<ThrusterEntity>();
		thrusterEntity.Forward = thrusterMenu.ForwardBind;
		thrusterEntity.Backward = thrusterMenu.BackwardBind;
		thrusterEntity.Toggle = thrusterMenu.Toggle;
		thrusterEntity.Force = thrusterMenu.Force;
		thrusterEntity.localPawn = PlayerState.Local.Pawn.Id;

		Rigidbody rb = thruster.Components.Create<Rigidbody>();

		// Is dosnt work, idk how fix
		PhysicsBody body = modelCollider.KeyframeBody;

		thruster.NetworkSpawn();

		PlayerState.Thing thing = new()
		{
			gameObjects = new List<GameObject>{thruster}
		};

		PlayerState.Local?.SpawnedThings.Add(thing);

		Weld.CreateWeld( Player.GameObject, thruster, thruster.Transform.Position + thruster.Transform.Rotation.Down*20, aim.GameObject, aim.Body.Transform.PointToLocal( aim.HitPosition ) );
	}
}
