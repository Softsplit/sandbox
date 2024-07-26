using Sandbox;
using Sandbox.Navigation;
using Softsplit;

public sealed class GruntAI : AIAgent
{
    public CoverFinder CoverFinder;
    public EnemyWeaponDealer EnemyWeaponDealer;
    public FindChooseEnemy FindChooseEnemy;
    [Property] public AnimationHelper AnimationHelper {get;set;}
    AgroRelations agroRelations;
    HealthComponent healthComponent;
    float lastHealth;
    //public int Grenades {get;set;} = 3;
    //public float GrenadeTime {get;set;} = 30f;
    //public float GrenadeRange {get;set;} = 250f;
    [Property] public Vector3 EyePos {get;set;}
    [Property] public float SuppressPatience {get;set;} = 15f;
    [Property] public float Patience {get;set;} = 30f;
    [Property] public float AttackDistance {get;set;} = 450f;
    [Property] public float MajorDamageAmount {get;set;} = 20f;
    [Property] public float HealthMemory {get;set;} = 0.5f;
    [Property] public bool IsCrouching { get; set; }
    [Property] public float SmoothCrouchSpeed { get; set; } = 5.0f;
    [Property] public float HideTime { get; set; } = 5.0f;

    CoverContext currentCover;

    private float smoothCrouch;
    //float currentGrenadeTime;

    protected override void SetStates()
    {
        //currentGrenadeTime = GrenadeTime;
        healthComponent = Components.Get<HealthComponent>();
        CoverFinder = Scene.Components.GetInChildren<CoverFinder>();
        FindChooseEnemy = Components.GetOrCreate<FindChooseEnemy>();
        agroRelations = Components.Get<AgroRelations>();
        EnemyWeaponDealer = Components.Get<EnemyWeaponDealer>();
        initialState = "HIDE_AND_RELOAD";
        stateMachine.RegisterState(new SUPPRESS());
        stateMachine.RegisterState(new PRESS_ATTACK());
        stateMachine.RegisterState(new FIRE_DISTANCE());
        stateMachine.RegisterState(new COVER());
        stateMachine.RegisterState(new HIDE_AND_RELOAD());
        stateMachine.RegisterState(new CAUTION_COVER());
        stateMachine.RegisterState(new IDLE());
    }

    public PlayerGlobals Global => GetGlobal<PlayerGlobals>();

    protected override void Update()
    {
        //currentGrenadeTime+=Time.Delta;
        //lastHealth = MathX.Lerp(lastHealth,healthComponent.Health,Time.Delta*HealthMemory);
        float targetCrouch = IsCrouching ? 1.5f : 0.0f;
        smoothCrouch = MathX.Lerp(smoothCrouch, targetCrouch, Time.Delta * SmoothCrouchSpeed);

        Controller.Speed = FindChooseEnemy.Enemy.IsValid() ? Global.SprintingSpeed*0.75f : Global.WalkSpeed*0.75f;

        if ( AnimationHelper.IsValid() )
		{
			AnimationHelper.WithVelocity( Controller.velocity.IsNearlyZero() ? Vector3.Zero : Controller.velocity );
			//AnimationHelper.WithWishVelocity( Controller.characterController.Velocity );
			AnimationHelper.IsGrounded = Controller.useCharacterController ? Controller.characterController.IsOnGround : true;
			AnimationHelper.WithLook( 
                FindChooseEnemy.Enemy != null ? 

                    (FindChooseEnemy.Enemy.Transform.World.PointToWorld(FindChooseEnemy.EnemyRelations.attackPoint) - Transform.World.PointToWorld(EyePos))

                    : 
                    
                    Transform.World.Forward, 


                1, 1, 1.0f );
			AnimationHelper.MoveStyle = AnimationHelper.MoveStyles.Run;
			AnimationHelper.DuckLevel = smoothCrouch;
			AnimationHelper.HoldType = EnemyWeaponDealer.Weapon.GetHoldType();
			AnimationHelper.Handedness = EnemyWeaponDealer.Weapon.IsValid() ? EnemyWeaponDealer.Weapon.Handedness : AnimationHelper.Hand.Both;
			AnimationHelper.AimBodyWeight = 0.1f;
		}

        if(FindChooseEnemy.Enemy.IsValid())
        {
            stateMachine.ChangeState(FindCondition());
        }
        else
        {
            stateMachine.ChangeState("IDLE");
        }
        Log.Info(stateMachine.currentState);

        lastHealth = healthComponent.Health;

        EnemyWeaponDealer.Bullet.ForceShoot = false;
        EnemyWeaponDealer.Reload.ForceShoot = false;
    }

    public (CoverContext cover, float distance) CheckCover()
    {
        float dis = 1000;
        if(currentCover == null)
        {
            CoverContext newCover = CoverFinder.GetClosestCover(Transform.Position,FindChooseEnemy.Enemy.Transform.Position);
            if(newCover != null) ClaimCover(newCover);
        }
        else
        {
            if(!CoverFinder.IsValidCover(currentCover, FindChooseEnemy.Enemy.Transform.Position))
            {
                DropCover();
            }
        }
        
        if(currentCover!=null) dis = Vector3.DistanceBetween(Transform.Position,currentCover.Transform.Position);

        return (currentCover,dis);
    }


    public bool RetreatToFireLine()
    {
        return EnemyDistance() < AttackDistance;
    }

    public float EnemyDistance()
    {
        return Vector3.DistanceBetween(Transform.Position,FindChooseEnemy.Enemy.Transform.Position);
    }

    public void ClaimCover(CoverContext coverContext)
    {
        coverContext.owned = true;
        currentCover = coverContext;
    }

    public void DropCover()
    {
        currentCover.owned = false;
        currentCover = null;
    }

    public void CanSeeEnemy()
    {

    }

    string FindCondition()
    {
        if (FindChooseEnemy.NewEnemy)
        {
            GameObject pEnemy = FindChooseEnemy.Enemy;
            FindChooseEnemy.NewEnemy = false;
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
            Log.Info(EnemyWeaponDealer.Reload.AmmoComponent.MaxAmmo/10);
            return "HIDE_AND_RELOAD";
        }
        else if (stateMachine.currentState == "HIDE_AND_RELOAD")
        {
            return "COVER" ;
        }

        
        if (lastHealth-healthComponent.Health > MajorDamageAmount)
        {
            return "CAUTION_COVER";
        }
        
        if (MathF.Abs(EnemyDistance() - AttackDistance) < 50)
        {
            return "COVER";
        }

        // Default return, if no conditions matched
        return stateMachine.currentState;
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
public class IDLE : AIState
{
    GruntAI gruntAI;
	public void Enter( AIAgent agent )
	{
		gruntAI = agent.Components.Get<GruntAI>();
	}

	public void Exit( AIAgent agent )
	{
		
	}

	public string GetID()
	{
		return "IDLE";
	}
	
    float timeSinceLastCanShoot;
	public void Update( AIAgent agent )
	{
        agent.Controller.currentTarget = agent.Transform.Position;
	}
}
public class SUPPRESS : AIState
{
    GruntAI gruntAI;
	public void Enter( AIAgent agent )
	{
		gruntAI = agent.Components.Get<GruntAI>();
        gruntAI.IsCrouching = false;
	}

	public void Exit( AIAgent agent )
	{

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
            if(timeSinceLastCanShoot > gruntAI.SuppressPatience) agent.Controller.currentTarget = gruntAI.FindChooseEnemy.Enemy.Transform.Position;
        }
	}
}
public class PRESS_ATTACK : AIState
{
    GruntAI gruntAI;
	public void Enter( AIAgent agent )
	{
		gruntAI = agent.Components.Get<GruntAI>();
        gruntAI.IsCrouching = false;
	}

	public void Exit( AIAgent agent )
	{

	}

	public string GetID()
	{
		return "PRESS_ATTACK";
	}
	
	public void Update( AIAgent agent )
	{
        gruntAI.FaceThing(gruntAI.FindChooseEnemy.Enemy);
        agent.Controller.currentTarget = gruntAI.EnemyDistance() > 100 ? gruntAI.FindChooseEnemy.Enemy.Transform.Position : agent.Transform.Position;
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
public class FIRE_DISTANCE : AIState
{
    GruntAI gruntAI;
	public void Enter( AIAgent agent )
	{
		gruntAI = agent.Components.Get<GruntAI>();
        gruntAI.IsCrouching = false;
	}

	public void Exit( AIAgent agent )
	{
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
        if(gruntAI.EnemyDistance() > gruntAI.AttackDistance) 
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

    float coverValue = 0.5f;
    float patience;
	public void Enter( AIAgent agent )
	{
        patience = 0;
		gruntAI = agent.Components.Get<GruntAI>();
	}

	public void Exit( AIAgent agent )
	{
	}

	public string GetID()
	{
		return "COVER";
	}
    bool moveOutOfCover = true;
	public void Update( AIAgent agent )
	{
        (CoverContext currentCover, float distance) = gruntAI.CheckCover();

        patience+=Time.Delta;
        GameObject enemy = gruntAI.FindChooseEnemy.Enemy;
        bool unCover = currentCover != null ? Noise.Perlin(Time.Now*20,currentCover.Transform.Position.Length) > 0.5f : false;
        
        gruntAI.FaceThing(enemy);

        
        
        if (currentCover == null)
        {
            if (gruntAI.RetreatToFireLine())
            {
                gruntAI.Controller.currentTarget = gruntAI.Transform.Position - (enemy.Transform.Position - gruntAI.Transform.Position).Normal * 150f;
            }
            else
            {
                gruntAI.Controller.currentTarget = gruntAI.Transform.Position;
            }
        }
        else
        {
            if (!currentCover.wall)
            {
                gruntAI.Controller.currentTarget = currentCover.Transform.Position;
            }
            else
            {
                if (unCover)
                {
                    if(moveOutOfCover)
                        gruntAI.Controller.currentTarget = enemy.Transform.Position;
                    else
                        gruntAI.Controller.currentTarget = currentCover.Transform.Position;
                }
                else
                {
                    moveOutOfCover = true;
                    gruntAI.Controller.currentTarget = currentCover.Transform.Position;
                }
            }
        }
        
        
        if(currentCover != null)
            gruntAI.IsCrouching = currentCover.wall ? false : unCover && (distance < 20);
        else
            gruntAI.IsCrouching = false;
        
        
		if(gruntAI.EnemyWeaponDealer.WeaponHitsTarget(gruntAI.FindChooseEnemy.Enemy))
        {
            moveOutOfCover = false;
            gruntAI.EnemyWeaponDealer.Bullet.ForceShoot = true;
            patience = 0;
        }
        else
        {
            gruntAI.EnemyWeaponDealer.Bullet.ForceShoot = false;
        }

        if(patience > gruntAI.Patience) agent.stateMachine.ChangeState("SUPPRESS");
        
	}
    
}
public class HIDE_AND_RELOAD : AIState
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
		return "HIDE_AND_RELOAD";
	}
	public void Update( AIAgent agent )
	{
        GameObject enemy = gruntAI.FindChooseEnemy.Enemy;

        gruntAI.FaceThing(enemy);

        (CoverContext currentCover, float distance) = gruntAI.CheckCover();

        gruntAI.Controller.currentTarget = currentCover == null ? 
        
            gruntAI.Transform.Position - (enemy.Transform.Position - gruntAI.Transform.Position).Normal*150f
            
            :
            
            currentCover.Transform.Position;
        
        gruntAI.IsCrouching = distance < 20;
        
        gruntAI.EnemyWeaponDealer.Reload.ForceShoot = true;
	}
}
public class CAUTION_COVER : AIState
{
    GruntAI gruntAI;


	public void Enter( AIAgent agent )
	{
        hideTime = 0;
		gruntAI = agent.Components.Get<GruntAI>();
	}

	public void Exit( AIAgent agent )
	{

	}

	public string GetID()
	{
		return "CAUTION_COVER";
	}
    float hideTime;
	public void Update( AIAgent agent )
	{
        hideTime += Time.Delta;
        
        GameObject enemy = gruntAI.FindChooseEnemy.Enemy;

        gruntAI.FaceThing(enemy);

        (CoverContext currentCover, float distance) = gruntAI.CheckCover();
        
        gruntAI.Controller.currentTarget = currentCover == null ? 
        
            (gruntAI.Transform.Position - (enemy.Transform.Position - gruntAI.Transform.Position).Normal*150f)
            
            :
            
            currentCover.Transform.Position;
        
        gruntAI.IsCrouching = (distance < 20);
        
        
		if(gruntAI.EnemyWeaponDealer.WeaponHitsTarget(gruntAI.FindChooseEnemy.Enemy))
        {
            gruntAI.EnemyWeaponDealer.Bullet.ForceShoot = true;
        }
        else
        {
            gruntAI.EnemyWeaponDealer.Bullet.ForceShoot = false;
        }

        if(hideTime>gruntAI.HideTime) agent.stateMachine.ChangeState("COVER");
	}
    
}