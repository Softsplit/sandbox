using Sandbox;
using Sandbox.Navigation;
using Softsplit;

public sealed class GruntAI : AIAgent
{
    public EnemyWeaponDealer EnemyWeaponDealer;
    public FindChooseEnemy FindChooseEnemy;
    AgroRelations agroRelations;
    HealthComponent healthComponent;
    float lastHealth;
    //public int Grenades {get;set;} = 3;
    //public float GrenadeTime {get;set;} = 30f;
    //public float GrenadeRange {get;set;} = 250f;
    public float Patience {get;set;} = 30f;
    public float AttackDistance {get;set;} = 450f;
    public float MajorDamageAmount {get;set;} = 20f;
    public float HealthMemory {get;set;} = 0.5f;
    public bool IsCrouching;
    //float currentGrenadeTime;

    protected override void SetStates()
    {
        //currentGrenadeTime = GrenadeTime;
        FindChooseEnemy = Components.GetOrCreate<FindChooseEnemy>();
        agroRelations = Components.Get<AgroRelations>();
        EnemyWeaponDealer = Components.Get<EnemyWeaponDealer>();
        initialState = "Idle";
    }

    protected override void Update()
    {
        //currentGrenadeTime+=Time.Delta;
        lastHealth = MathX.Lerp(lastHealth,healthComponent.Health,Time.Delta*HealthMemory);
    }
    public float EnemyDistance;
    string FindCondition()
    {
        EnemyDistance = Vector3.DistanceBetween(Transform.Position,FindChooseEnemy.Enemy.Transform.Position);
        if (FindChooseEnemy.NewEnemy)
        {
            GameObject pEnemy = FindChooseEnemy.Enemy;
            bool bFirstContact = false;
            float flTimeSinceFirstSeen = FindChooseEnemy.TimeSinceSeen; // Assuming Time.Now is current time

            if (flTimeSinceFirstSeen < 3.0f)
                bFirstContact = true;

            if ( pEnemy != null)
            {

                if (!bFirstContact)
                {
                    if (Game.Random.Next(0, 100) < 60) 
                    {
                        return "FIRE_DISTANCE";
                    }
                    else
                    {
                        return "PRESS_ATTACK";
                    }
                }

                return "COVER";
            }
        }

        if (EnemyWeaponDealer.Reload.AmmoComponent.Ammo == 0 || EnemyWeaponDealer.Reload.AmmoComponent.Ammo < EnemyWeaponDealer.Reload.AmmoComponent.MaxAmmo/10)
        {
            return "HIDE_AND_RELOAD";
        }

        
        if (lastHealth - healthComponent.Health < MajorDamageAmount)
        {
            /*
            if (!IsCrouching)
            {
                if (GetEnemy() != null && Random.Next(0, 100) < 50 && CouldShootIfCrouching(GetEnemy()))
                {
                    Crouch();
                }
                else
                {
                    return "SCHED_TAKE_COVER_FROM_ENEMY";
                }
            }
            Implement This After Testing
            */

            return "CAUTION_COVER";
        }
        
        if (MathF.Abs(EnemyDistance - AttackDistance) < 50)
        {
            return "COVER";
        }

        // Default return, if no conditions matched
        return string.Empty;
    }

}
/*
"SUPPRESS"
"FIRE_DISTANCE"
"PRESS_ATTACK"
"COVER"
"HIDE_AND_RELOAD"
"CAUTION_COVER"
*/

public class SUPPRESS : AIState
{
    GruntAI gruntAI;
	public void Enter( AIAgent agent )
	{
		gruntAI = agent.Components.Get<GruntAI>();
	}

	public void Exit( AIAgent agent )
	{
		gruntAI.EnemyWeaponDealer.Bullet.ForceShoot = false;
	}

	public string GetID()
	{
		return "SUPPRESS";
	}
	
    float timeSinceLastCanShoot;
	public void Update( AIAgent agent )
	{
        timeSinceLastCanShoot++;
        agent.Controller.currentTarget = agent.Transform.Position;
		if(gruntAI.EnemyWeaponDealer.WeaponHitsTarget(gruntAI.FindChooseEnemy.Enemy))
        {
            timeSinceLastCanShoot = 0;
            gruntAI.EnemyWeaponDealer.Bullet.ForceShoot = true;
        }
        else
        {
            gruntAI.EnemyWeaponDealer.Bullet.ForceShoot = false;
            if(timeSinceLastCanShoot > gruntAI.Patience) agent.Controller.currentTarget = gruntAI.FindChooseEnemy.Enemy.Transform.Position;
        }
	}
}
public class FIRE_DISTANCE : AIState
{
    GruntAI gruntAI;
	public void Enter( AIAgent agent )
	{
		gruntAI = agent.Components.Get<GruntAI>();
	}

	public void Exit( AIAgent agent )
	{
		gruntAI.EnemyWeaponDealer.Bullet.ForceShoot = false;
	}

	public string GetID()
	{
		return "FIRE_DISTANCE";
	}
	
	public void Update( AIAgent agent )
	{
		if(gruntAI.EnemyWeaponDealer.WeaponHitsTarget(gruntAI.FindChooseEnemy.Enemy))
        {
            gruntAI.EnemyWeaponDealer.Bullet.ForceShoot = true;
        }
        else
        {
            gruntAI.EnemyWeaponDealer.Bullet.ForceShoot = false;
        }
        if(gruntAI.EnemyDistance > gruntAI.AttackDistance) 
        {
            agent.Controller.currentTarget = agent.Transform.Position;
        }
        else
        {
            agent.stateMachine.ChangeState("SUPPRESS");
        }
	}
}
public class COVER : AIState
{
    GruntAI gruntAI;
	public void Enter( AIAgent agent )
	{
		gruntAI = agent.Components.Get<GruntAI>();
	}

	public void Exit( AIAgent agent )
	{
		gruntAI.EnemyWeaponDealer.Bullet.ForceShoot = false;
	}

	public string GetID()
	{
		return "COVER";
	}
	public void Update( AIAgent agent )
	{

		if(gruntAI.EnemyWeaponDealer.WeaponHitsTarget(gruntAI.FindChooseEnemy.Enemy))
        {
            gruntAI.EnemyWeaponDealer.Bullet.ForceShoot = true;
        }
        else
        {
            gruntAI.EnemyWeaponDealer.Bullet.ForceShoot = false;
        }
	}
}
