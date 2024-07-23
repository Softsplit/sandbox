using Sandbox;

public sealed class FindChooseEnemy : Component
{
	[Property] public GameObject Enemy {get;set;}
	[Property] public bool NewEnemy {get;set;}
	[Property] public float TimeSinceSeen {get;set;}
	[Property] public float DetectRange {get;set;} = 700f;
	[Property] public float ForceTargetRange {get;set;} = 300f;

	AgroRelations agroRelations;

	protected override void OnStart()
	{
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

		IEnumerable<GameObject> Detected = Scene.FindInPhysics(new Sphere(Transform.Position,DetectRange));

		if (Detected == null || Detected.Count() < 1) return;

		GameObject closest = null;
		float closestRange = DetectRange;
		foreach(GameObject g in Detected)
		{
			if(!g.Tags.Contains("relations") && !g.Tags.Contains("player")) break;

			AgroRelations gAgroRelations = g.Components.Get<AgroRelations>();

			bool doBreak = true;
			foreach(string s in gAgroRelations.Enemies)
			{
				if(agroRelations.Enemies.Contains(s))
				{
					doBreak = false; 
					break;
				} 
			}
			if(doBreak) break;

			float distance = Vector3.DistanceBetween(g.Transform.Position,Transform.Position);
			if(distance < closestRange)
			{
				closest = g;
				closestRange = distance;
			}
		}

		if (closest == null) return;

		if(Enemy==null)
		{
			Enemy = closest;
			NewEnemy = true;
			TimeSinceSeen = 0;
		}
		else return;


		if(closestRange < ForceTargetRange)
		{
			Enemy = closest;
			NewEnemy = true;
			TimeSinceSeen = 0;
		}
	}

}
