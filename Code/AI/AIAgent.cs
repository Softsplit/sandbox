using Sandbox;

public abstract class AIAgent : Component
{
    [Property] public AIStateMachine stateMachine { get; set; }
    public string initialState { get; set; }
    public NavMeshCharacter Controller { get; set; }
    public NavMeshAgent Agent { get; set; }

    Npcsettings npcsettings;

    protected override void OnStart()
    {
        if(!Networking.IsHost) Enabled = false;
        npcsettings = Scene.Components.GetInChildren<Npcsettings>();
        Controller = Components.GetOrCreate<NavMeshCharacter>();
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
		if(!npcsettings.Think) return;
        Update();
        if(!Networking.IsHost) return;
        stateMachine.Update();
        
    }

    protected virtual void Update()
    {
        
    }
}
