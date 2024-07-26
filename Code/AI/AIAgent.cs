using Sandbox;

public abstract class AIAgent : Component
{
    public AIStateMachine stateMachine { get; set; }
    public string initialState { get; set; }
    public NavMeshCharacter Controller { get; set; }
    public NavMeshAgent Agent { get; set; }

    protected override void OnStart()
    {
        if(!Networking.IsHost) Enabled = false;
        
        Controller = Components.GetOrCreate<NavMeshCharacter>();
        Agent = Components.GetOrCreate<NavMeshAgent>();
        Controller.currentTarget = Transform.Position;
        stateMachine = new AIStateMachine(this);
        SetStates();
        InitializeState();
    }

    protected virtual void SetStates()
    {
        
    }

    async void InitializeState()
    {
        await Task.Frame();
        stateMachine.ChangeState(initialState, false);
    }

    protected override void OnUpdate()
    {
		
        Update();
        stateMachine.Update();
        
    }

    protected virtual void Update()
    {
        
    }

    public void FaceThing(GameObject thing)
    {
        Angles angles = Rotation.LookAt(thing.Transform.Position - Transform.Position);
        angles.pitch = 0;
        angles.roll = 0;
        Transform.Rotation = angles;
    }
}
