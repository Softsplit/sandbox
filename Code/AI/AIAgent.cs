using Sandbox;

public abstract class AIAgent : Component
{
    [Property] public AIStateMachine stateMachine { get; set; }
    [Property] public string initialState { get; set; }
    [Property] public NavMeshCharacter Controller { get; set; }

    protected override void OnStart()
    {
        Controller = Components.GetOrCreate<NavMeshCharacter>();
        Controller.currentTarget = Transform.Position;
        stateMachine = new AIStateMachine(this);
        SetStates();
        InitializeState();
    }

    protected virtual void SetStates()
    {
        // Implement state registration here
    }

    async void InitializeState()
    {
        await Task.Frame();
        stateMachine.ChangeState(initialState, false);
    }

    protected override void OnUpdate()
    {
        stateMachine.Update();
        Update();
    }

    protected virtual void Update()
    {
        // Custom update logic here
    }

    public void FaceThing(GameObject thing)
    {
        Angles angles = Rotation.LookAt(thing.Transform.Position - Transform.Position);
        angles.pitch = 0;
        angles.roll = 0;
        Transform.Rotation = angles;
    }
}
