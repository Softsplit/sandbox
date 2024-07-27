using Sandbox;

namespace Softsplit;

public sealed class FindChooseEnemy : Component
{
	[Property] public GameObject Enemy {get;set;}
	[Property] public AgroRelations EnemyRelations {get;set;}
	[Property] public HealthComponent HealthComponent {get;set;}
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
		HealthComponent = Components.GetOrCreate<HealthComponent>();

		if(agroRelations.Faction == null || agroRelations.Enemies == null)
		{
			agroRelations.Faction = "Enemy";
			agroRelations.Enemies = new List<string>{"Player"};
		}
	}

	DamageInfo lastAttacker;

	protected override void OnFixedUpdate()
	{
		(bool isTrue, AgroRelations agroRelations) isEnemy(GameObject g)
		{
			if(g.Tags == null) return (false,null);	
			
			if(!g.Tags.Contains("relations")) return (false,null);

			if(g.Tags.Contains("player") && npcsettings.IgnorePlayers) return (false,null);
			
			AgroRelations gAgroRelations = g.Components.Get<AgroRelations>();
			
			if(gAgroRelations ==null) return (false, null);
			
			if(!agroRelations.Enemies.Contains(gAgroRelations.Faction))
			{
				return (false,gAgroRelations);
			} 

			return (true, gAgroRelations);
		}
		if(!Networking.IsHost) return;

		if(lastAttacker != HealthComponent.latestDamageInfo)
		{
			lastAttacker = HealthComponent.latestDamageInfo;
			GameObject g = HealthComponent.latestDamageInfo.Attacker.GameObject;
			
			(bool isTrue, AgroRelations gAgroRelations) = isEnemy(g);
			
			if(!agroRelations.Enemies.Contains(gAgroRelations.Faction))
			{
				isTrue = true;
				agroRelations.Enemies.Add(gAgroRelations.Faction);
			}

			if(isTrue)
			{
				Enemy = g;
				EnemyRelations = gAgroRelations;
				return;
			}
		}

		

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
			(bool isTrue, AgroRelations gAgroRelations) = isEnemy(g);
			if(!isTrue) continue;	
			
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
