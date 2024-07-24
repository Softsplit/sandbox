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

	protected override void OnStart()
	{
		if(!Networking.IsHost) Enabled = false;
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

		if(Enemy == null)
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
