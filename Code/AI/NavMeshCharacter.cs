using System;
using Sandbox;
using Sandbox.Navigation;

public sealed class NavMeshCharacter : Component
{
	public CharacterController characterController {get;set;}
	[Category("Character Controller"), Property] public bool useCharacterController = true;
	[Category("Character Controller"), Property, Range(0,200)] public float Radius {get;set;} = 16f;
	[Category("Character Controller"), Property, Range(0,200)] public float Height {get;set;} = 64f;
	[Category("Character Controller"), Property, Range(0,50)] public float StepHeight {get;set;} = 18f;
	[Category("Character Controller"), Property, Range(0,90)] public float GroundAngle {get;set;} = 45f;
	[Category("Character Controller"), Property, Range(0,200)] public float Acceleration {get;set;} = 60f;
	[Category("Character Controller"), Property, Range(0,1)] public float Bounciness {get;set;} = 0.3f;
	[Category("Character Controller | Collision"), Property] public bool UseProjetcCollisionRules {get;set;} = false;
	[Category("Character Controller | Collision"), Property] public TagSet IgnoreLayers {get;set;}
	

	[Category("Agent"), Property] public List<Vector3> CurrentPath {get;set;}
	[Category("Agent"), Property] public float Speed {get;set;} = 120f;
	[Category("Agent"), Property, Range(0,10)] public float SpeedSmoothing {get;set;} = 20f;
	[Category("Agent"), Property] public bool UpdateRotation {get;set;} = true;
	[Category("Agent"), Property, Range(0,10)] public float PositionAccuracy {get;set;} = 5f;
	[Category("Debug"), Property] public bool Gizmos {get;set;}
	public BoxCollider boxCollider {get; set;}
	protected override void OnStart()
	{
		currentTarget = Vector3.One * 1.54626734562345f;
		GameObject g = new GameObject();
		g.SetParent(GameObject);
		g.Transform.Position = Transform.Position;
		g.Transform.Rotation = Transform.Rotation;
		g.Name = "Box Collider";
		g.Tags.Add("IgnoreBullet");
		boxCollider = g.Components.GetOrCreate<BoxCollider>();
		boxCollider.Scale = new Vector3(Radius*2,Radius*2,Height);
		boxCollider.Center = new Vector3(0,0,Height/2);
		if(useCharacterController)
		{
			characterController = Components.GetOrCreate<CharacterController>();
			characterController.Radius = Radius;
			characterController.Height = Height;
			characterController.StepHeight = StepHeight;
			characterController.GroundAngle = GroundAngle;
			characterController.Acceleration = Acceleration;
			characterController.Bounciness = Bounciness;
			characterController.UseCollisionRules = UseProjetcCollisionRules;
			characterController.IgnoreLayers = IgnoreLayers;
		}
		
		CurrentPath = new List<Vector3>(){Transform.Position};
	}
	bool AtTarget;
	public Vector3 velocity;

	Vector3 lastPos;
	protected override void OnUpdate()
	{
		MoveTo();
		AtTarget = Vector3.DistanceBetween(Transform.Position,CurrentPath[0]) < PositionAccuracy;
		if(!AtTarget)
		{
			Move();	
			var direction = (CurrentPath[0]-Transform.Position).Normal;
			velocity = useCharacterController ? characterController.Velocity : direction*Speed;
		}
		else
		{
			if(CurrentPath.Count > 1) CurrentPath.RemoveAt(0);
			velocity = Vector3.Zero;
		}
		
		//lastPos = Transform.Position;
	}
	void Move()
	{
		var gravity = Game.ActiveScene.PhysicsWorld.Gravity;
		var direction = (CurrentPath[0]-Transform.Position).Normal;
		if(useCharacterController)
		{
			characterController.Velocity.WithZ(0);
			characterController.Velocity = Vector3.Lerp(characterController.Velocity, direction*Speed, SpeedSmoothing*Time.Delta);
			Log.Info(gravity);
			if(!characterController.IsOnGround) characterController.Velocity += gravity;
			characterController.Move();
		}
		else
		{
			Transform.Position += direction*Speed*Time.Delta;
		}
		
		
		if(Gizmos)
		{
			Gizmo.Draw.Arrow(Transform.Position, Transform.Position+direction*50);
			foreach(Vector3 pos in CurrentPath)
			{
				Gizmo.Draw.Arrow(pos+Vector3.Up*10,pos);
			}
		}
	}
	

	public Vector3 TargetPosition()
	{
		return CurrentPath[CurrentPath.Count-1];
	}
	public float DistanceToTarget()
	{
		return Vector3.DistanceBetween(CurrentPath[CurrentPath.Count-1],Transform.Position);
	}
	[Property] public Vector3 lastTarget {get;set;}
	[Property] public Vector3 currentTarget {get;set;}
	public void MoveTo(bool ForceUpdate = false)
	{
		
		if(currentTarget != lastTarget)
		{
			lastTarget = currentTarget;
			List<Vector3> path = Scene.NavMesh.GetSimplePath(Scene.NavMesh.GetClosestPoint(Transform.Position).Value,Scene.NavMesh.GetClosestPoint(currentTarget).Value);
			path.RemoveAt(0);
			if(path.Count == 0) path.Add(Transform.Position);
			CurrentPath = path;
		}
	}
}
