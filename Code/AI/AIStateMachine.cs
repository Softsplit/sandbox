using System.Collections.Generic;

public class AIStateMachine
{
    [Property] public Dictionary<string, AIState> states;
    public AIAgent agent;
    [Property] public string currentState { get; set; }

    public AIStateMachine(AIAgent agent)
    {
        this.agent = agent;
        states = new Dictionary<string, AIState>();
    }

    public void RegisterState(AIState state)
    {
        string stateID = state.GetID();
        states.Add(stateID, state);
    }

    public AIState GetState(string stateID)
    {
        states.TryGetValue(stateID, out AIState state);
        return state;
    }

    public void Update()
    {
        if(currentState!=null) GetState(currentState)?.Update(agent);
        
    }

    public void ChangeState(string newState, bool overrideState = false)
    {
        if (newState == currentState && !overrideState) return;
        if(currentState!=null) GetState(currentState).Exit(agent);
        
        currentState = newState;
        GetState(currentState)?.Enter(agent);
    }
}
