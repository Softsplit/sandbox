using Sandbox;

public sealed class FindChooseEnemy : Component
{
	[Property] public GameObject Enemy {get;set;}
	[Property] public AgroRelations EnemyRelations {get;set;}
	[Property] public bool NewEnemy {get;set;}
	[Property] public float TimeSinceSeen {get;set;}
	[Property] public float DetectRange {get;set;} = 700f;
	[Property] public float ForceTargetRange {get;set;} = 300f;

	AgroRelations agroRelations;
	Npcsettings npcsettings;

	protected override void OnStart()
	{
		if(!Networking.IsHost) Enabled = false;
		npcsettings = Scene.Components.GetInChildren<Npcsettings>();
		agroRelations = Components.GetOrCreate<AgroRelations>();

		if(agroRelations.Faction == null || agroRelations.Enemies == null)
		{
			agroRelations.Faction = "Enemy";
			agroRelations.Enemies = new List<string>{"Player"};
		}
	}
	protected override void OnFixedUpdate()
	{
		
		if(!Networking.IsHost) return;

		if(npcsettings.IgnorePlayers && Enemy!=null )
		{
			if(Enemy.Tags.Contains("player")) Enemy = null;
		}

		TimeSinceSeen+=Time.Delta;

		List<GameObject> Detected = Scene.FindInPhysics(new Sphere(Transform.Position,DetectRange)).ToList();
		if (Detected == null || Detected.Count() < 1) return;
		GameObject closest = null;
		AgroRelations closestRelations = null;
		float closestRange = DetectRange;
		foreach(GameObject g in Detected)
		{

			if(g.Tags == null) continue;	
			
			if(!g.Tags.Contains("relations")) continue;

			if(g.Tags.Contains("player") && npcsettings.IgnorePlayers) continue;
			
			AgroRelations gAgroRelations = g.Components.Get<AgroRelations>();
			if(gAgroRelations ==null) continue;
			
			if(!agroRelations.Enemies.Contains(gAgroRelations.Faction))
			{
				
				continue;
			} 
			
			float distance = Vector3.DistanceBetween(g.Transform.Position,Transform.Position);
			if(distance < closestRange)
			{
				closest = g;
				closestRelations = gAgroRelations;
				closestRange = distance;
			}
		}
		
		if (closest == null) return;

		if(!Enemy.IsValid())
		{
			Enemy = closest;
			EnemyRelations = closestRelations;
			NewEnemy = true;
			TimeSinceSeen = 0;
			return;
		}


		if(closestRange < ForceTargetRange && Enemy != closest)
		{
			Enemy = closest;
			EnemyRelations = closestRelations;
			NewEnemy = true;
			TimeSinceSeen = 0;
		}
	}

}
