using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Threading;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

using Mediation.StateSpace;
using Mediation.PlanTools;
using Mediation.Planners;
using Mediation.FileIO;
using Mediation.KnowledgeTools;
using Mediation.Enums;
using Mediation.Interfaces;
using Mediation.Utilities;

using PlanRecognition;
using PixelCrushers.DialogueSystem;


/// <summary>
/// The mediator is the bridge between the mediation engine and the Unity engine. It is responsible
/// for managing the (logical) game state and administering updates to the front-end game.
/// </summary>
public class Mediator : MonoBehaviour
{
    // The domain name.
    public string domainName = "hyrule";

    // The problem name
    public string problemName = "prob01";

    // The current mediation node.
    public StateSpaceNode root;

    // The mediation frontier.
    public Hashtable frontier;

    // The frontier thread.
    public Thread frontierThread;

    // The working domain object.
    [HideInInspector] public Domain domain;

    // The working problem object.
    [HideInInspector] public Problem problem;

    // The working plan object.
    [HideInInspector] public Plan plan;

    // The current state.
    [HideInInspector] public State state;

    // A log of player actions.
    [HideInInspector] public Queue<IOperator> playerLog;

    // The player's model of the environment.
    [HideInInspector] public EnvironmentModel playerEnvironmentModel;

    // The State Manager object used to answer state questions.
    private StateManager stateManager;

    // The edge in the state space denoting the player's action.
    private StateSpaceEdge playerActionEdge;

    // The Map Manager used to manage the world.
    private MapManager generator;

    // Is the game in a valid state?
    private bool validState;

    // Does the in-game state need to be updated?
    private bool stateNeedsUpdate = false;

    // The in-game planner.
    private Planner planner = Planner.SIWthenBFS;

    #region Experiment Setup Code

    // Requires using System.Security.Cryptography;
    public bool isLogging = true;

    // A log of all actions that have been attempted in the domain.
    [HideInInspector] public List<Tuple<string, string>> actionLog;

    // A more-reliable RNG
    private RNGCryptoServiceProvider participantIDGenerator;

    // Participant identification number.
    private int participantID;

    // The number of turns that the player has taken.
    private int playerTurnCounter;

    // The directory for writing the player's files.
    private string participantFolder;

    #endregion


    // Use this for initialization
    void Start( )
    {
        // Set the path to the top directory.
        Parser.path = Directory.GetParent(Application.dataPath).FullName + "/"; 

        // Parse the domain and problem files, and get the initial plan.
        domain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks/" + domainName + @"/domain.pddl", PlanType.StateSpace);
        problem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks/" + domainName + "/" + problemName + ".pddl");
        plan = PlannerInterface.Plan(planner, domain, problem);
		
        // A test for the planner.
        if (plan.Steps.Count > 0)
            Debug.Log("System loaded.");
        else
            Debug.Log("System not working or no plan exists.");

        state = plan.GetFirstState(); // Find the first state.
        validState = true; // Initialize the valid state.
        
        GameObject level = GameObject.Find("Level"); // Find the level game object.
        stateManager = level.GetComponent<StateManager>(); // Set the state manager.
        stateManager.Problem = problem;
        stateManager.Predicates = state.Predicates; // Set the state manager's predicates.

        generator = level.GetComponent<MapManager>(); // Set the level generator.
        generator.CreateLevel(); // Generate the level.

        // Create the initial node of mediation.
        root = StateSpaceMediator.BuildTree(planner, domain, problem, plan, state, 0);
        frontier = new Hashtable(); // Initialize the frontier.
        frontierThread = new Thread(ExpandFrontier); // Expand the frontier in a new thread.
        frontierThread.Start(); // Start the thread.

        // Initialize the player's environment model and action log.
        playerEnvironmentModel = new EnvironmentModel(stateManager.PlayerName, domain, problem);
        actionLog = new List<Tuple<string,string>>();

        // Initialize the player log with a decent size.
        playerLog = new Queue<IOperator>(3 * plan.Steps.Count); 

        #region Initialize the Experiment 

        // Generate the participant's ID
        participantIDGenerator = new RNGCryptoServiceProvider();

        byte[] idBytes = new byte[4];
        participantIDGenerator.GetBytes(idBytes);
        participantID = BitConverter.ToInt32(idBytes, 0);
		PlayerPrefs.SetInt("participantID", participantID);

        // Compute and create the participant's output folder.
        participantFolder = Parser.GetTopDirectory() + @"Benchmarks/" + domain.Name.ToLower() +
        @"/output/" + participantID + @"/";

        Directory.CreateDirectory(participantFolder);

        playerTurnCounter = 0;

        #endregion

        // Start the wizard conversation.
        DialogueManager.StartConversation("Wizard");
    }
	
    // Runs every frame
    void Update( )
    {
        if (stateNeedsUpdate)
        {
            UpdateState();

            isLogging = true;
            if (isLogging)
            {
                Domain knownDomain = playerEnvironmentModel.CompileDomain() as Domain;
                Problem knownProblem = playerEnvironmentModel.CompileProblem() as Problem;
                List<IOperator> knownChronology = playerEnvironmentModel.CompileChronology();

                string domainFileName = participantFolder + @"domain_" +
                                        stateManager.PlayerName + playerTurnCounter + ".pddl";

                string problemFileName = participantFolder + @"problem_" +
                                         stateManager.PlayerName + playerTurnCounter + ".pddl";

                string chronologyFileName = participantFolder + @"chronology_" +
                                            stateManager.PlayerName + playerTurnCounter + ".pddl";

                string logFileName = participantFolder + @"log_" + participantID + ".csv";


                // Write all the log data out to file.
                Writer.DomainToPDDL(domainFileName, knownDomain);
                Writer.ProblemToPDDL(problemFileName, knownDomain, knownProblem, playerEnvironmentModel.KnownCurrentState.Predicates);
                Writer.PlanToPDDL(chronologyFileName, knownChronology);
                Writer.ActionLogToCSV(logFileName, actionLog);

                playerTurnCounter++;
            }
        }
    }

    // Updates the current world state.
    public void UpdateState( )
    {
        // Block from re-entry
        stateNeedsUpdate = false;

        // Set the state manager's predicates.
        stateManager.Problem = problem;
        stateManager.Predicates = state.Predicates;

        // Initialize the frontier and expand in a new thread.
        frontier = new Hashtable(); 
        frontierThread = new Thread(ExpandFrontier);  

        // Check for goal state.
        if (plan.Steps.Count == 0 && root.problem.Initial.Count > 0)
        {
            Debug.Log("GOAL STATE");
            validState = false;
        }

        // Check for error state.
        else if (plan.Steps.Count == 0 && root.problem.Initial.Count == 0)
        {
            Debug.Log("UNWINNABLE STATE");
            validState = false;
        }

        // Let the system take its turn.
        MediatorUpdate();

        // Ask the state manager to refresh the world.
        stateManager.Refresh();

        if (validState)
            stateManager.PlayerTurn = true; // Return control to the player.
    }

    // Updates the planning problem this mediator is tracking by removing the first literal and adding the second
    // literal from/to the problem's initial state.
    public void SwapProblemInitialStateLiterals(IPredicate initLiteralToRemove, IPredicate initLiteralToAdd)
    {
        // Remove the first literal.
        this.state = this.state.RemoveLiteralFromState(initLiteralToRemove);

        // Add the second literal.
        this.state = this.state.AddLiteralToState(initLiteralToAdd);

        // Update the initial state.
        this.problem.Initial = this.state.Predicates;

        // Force the system to come up with a new plan that includes all new information.
        this.plan = PlannerInterface.Plan(Mediation.Enums.Planner.SIWthenBFS, this.domain, this.problem);

        // Trigger a no-op for the player to give the system the first chance to respond.
        PlayerUpdate("(donothing " + this.problem.Player + ")");
    }

    // Updates the planning problem this mediator is tracking by adding the given literal to the problem's init state.
    public void ExpandInitialState(IPredicate newInitLiteral)
    {
        // Update the current world state.
        this.state = this.state.AddLiteralToState(newInitLiteral);

        // Update the initial state.
        this.problem.Initial = this.state.Predicates;

        // Force the system to come up with a new plan that includes all new information.
        this.plan = PlannerInterface.Plan(Mediation.Enums.Planner.SIWthenBFS, this.domain, this.problem);

        // Trigger a no-op for the player to give the system the first chance to respond.
        PlayerUpdate("(donothing " + this.problem.Player + ")");
    }

    // Updates the planning problem this mediator is tracking by adding the given literal to the problem's goal state.
    public void ExpandGoalState(IPredicate newGoalStateLiteral)
    {
        // Add the literal to the goal state.
        if (!this.problem.Goal.Contains(newGoalStateLiteral))
            this.problem.Goal.Add(newGoalStateLiteral);

        // Force the system to come up with a new plan that includes all new information.
        this.plan = PlannerInterface.Plan(Mediation.Enums.Planner.SIWthenBFS, this.domain, this.problem);

        // Trigger a no-op for the player to give the system the first chance to respond.
        PlayerUpdate("(donothing " + this.problem.Player + ")");
    }

    // Updates the planning problem this mediator is tracking by removing the given literal from the problem's goal state.
    public void ContractGoalState(IPredicate goalLiteralToRemove)
    {
        // Update the goal literals accordingly.
        if (this.problem.Goal.Contains(goalLiteralToRemove))
            this.problem.Goal.Remove(goalLiteralToRemove);

        // Force the system to come up with a new plan that includes all new information.
        this.plan = PlannerInterface.Plan(Mediation.Enums.Planner.SIWthenBFS, this.domain, this.problem);

        // Trigger a no-op for the player to give the system the first chance to respond.
        PlayerUpdate("(donothing " + this.problem.Player + ")");	
    }

    // Updates the planning problem this mediator is tracking with two new literals,
    // one new literal for the initial state and one for the goal state.
    public void ExpandProblem(IPredicate newInitLiteral, IPredicate newGoalLiteral)
    {
        // Update the current world state.
        this.state = this.state.AddLiteralToState(newInitLiteral);

        // Update the initial state.
        this.problem.Initial = this.state.Predicates;

        // Update the goal state.
        this.problem.Goal.Add(newGoalLiteral);

        // Force the system to come up with a new plan that includes all new information.
        this.plan = PlannerInterface.Plan(Mediation.Enums.Planner.SIWthenBFS, this.domain, this.problem);

        // Trigger a no-op for the player to give the system the first chance to respond.
        PlayerUpdate("(donothing " + this.problem.Player + ")");

        // FIXME: The above "no-op" action should be encoded as part of the system's internals,
        // but it is currently dependent on the planning domain file.
    }

    // Updates the planning problem this mediator is tracking with new literals for the initial state.
    public void ExpandProblem(List<IPredicate> newInitLiterals)
    {
        foreach (IPredicate literal in newInitLiterals)
        {
            // Update the current world state.
            this.state = this.state.AddLiteralToState(literal);

            // Update the initial state.
            this.problem.Initial = this.state.Predicates;
        }

        // Force the system to come up with a new plan.
        this.plan = PlannerInterface.Plan(Mediation.Enums.Planner.SIWthenBFS, this.domain, this.problem);

        // Trigger a no-op for the player to give the system the first chance to respond.
        PlayerUpdate("(donothing " + this.problem.Player + ")");
    }
        
    // Updates the planning problem this mediator is tracking with new literals for the initial state
    // and for the goal state.
    public void ExpandProblem(List<IPredicate> newInitLiterals, List<IPredicate> newGoalLiterals)
    {
        // Add each init literal to the initial state.
        foreach (IPredicate literal in newInitLiterals)
        {
            // Update the current world state.
            this.state = this.state.AddLiteralToState(literal);

            // Update the initial state.
            this.problem.Initial = this.state.Predicates;
        }

        // Add each goal literal to the goal state.
        foreach (IPredicate literal in newGoalLiterals)
        {
            if (!this.problem.Goal.Contains(literal))
                this.problem.Goal.Add(literal);
        }

        // Force the system to come up with a new plan that includes all new information.
        this.plan = PlannerInterface.Plan(Mediation.Enums.Planner.SIWthenBFS, this.domain, this.problem);

        // Trigger a no-op for the player to give the system the first chance to respond.
        PlayerUpdate("(donothing " + this.problem.Player + ")");
    }

    // Updates the planning problem this mediator is tracking by adding the first goal literal
    // and removing the second.
    public void ExpandInitialStateAndSwapProblemGoalLiteral(IPredicate newInitLiteral, 
                                                            IPredicate newGoalLiteral, 
                                                            IPredicate goalLiteralToRemove)
    {
        // Update the goal literals accordingly.
        if (this.problem.Goal.Contains(goalLiteralToRemove))
            this.problem.Goal.Remove(goalLiteralToRemove);

        if (!this.problem.Goal.Contains(newGoalLiteral))
            this.problem.Goal.Add(newGoalLiteral);

        // Update the initial literal
        this.state = this.state.AddLiteralToState(newInitLiteral);
        this.problem.Initial = this.state.Predicates;

        // Force the system to come up with a new plan that includes all new information.
        this.plan = PlannerInterface.Plan(Mediation.Enums.Planner.SIWthenBFS, this.domain, this.problem);

        // Trigger a no-op for the player to give the system the first chance to respond.
        PlayerUpdate("(donothing " + this.problem.Player + ")");
    }
        
    // Checks whether the given string that represents the player's action is applicable in the current state.
    public bool IsApplicable(string playerAction)
    {
        foreach (StateSpaceEdge edge in root.outgoing)
        {
            // If we find the edge, it is applicable.
            if (edge.Action.ToString().Equals(playerAction))
            {
                return true;
            }
        }

        return false;
    }

    // Called when the player requests a move.
    public bool PlayerUpdate(string playerAction)
    {		
        //Debug.Log (playerAction);
		
        // A record for whether this is a valid action.
        bool validAction = false;
        stateManager.PlayerTurn = false;
		
        foreach (StateSpaceEdge edge in root.outgoing)
        {
            // If we find the edge, record it and log it.
            if (edge.Action.ToString().Equals(playerAction))
            {
                playerActionEdge = edge;
                playerLog.Enqueue(edge.Action);
                validAction = true;
                break;
            }
        }
			
        // If we have detected a valid action, process it through the mediation engine.
        if (validAction)
        {
            Thread updatePlayerPlan = new Thread(UpdatePlayerPlan);
            updatePlayerPlan.Start();
        }
        else
        {
            stateManager.PlayerTurn = true;
        }
		
        return validAction;
    }
	
    // Called when it's the computer's turn.
    private void MediatorUpdate( )
    {
        // Actions that took place.
        List<IOperator> executedActions = root.systemActions;
		
        // Loop through the system actions that have already happened...
        foreach (IOperator executedAction in executedActions)
        {	
            // Update the player model with the system's executed action and the current state.
            playerEnvironmentModel.UpdateModel(executedAction, state.Predicates);

            // Update the action log.
            actionLog.Add(Tuple.New(executedAction.ToString(), "success"));
        }
		
        frontierThread.Start();
    }

    // Updates the plan that solves the planning problem from the player's current state.
    private void UpdatePlayerPlan( )
    {
        // Cache the old root
        StateSpaceNode oldRoot = root;

        // Update the root: if we've already computed it, get it, otherwise, expand (i.e. compute it).
        if (frontier.ContainsKey(playerActionEdge) && !playerActionEdge.Action.Name.Equals("donothing"))
        {
            root = frontier[playerActionEdge] as StateSpaceNode;
        }
        else
        {
            frontierThread.Abort();
            root = StateSpaceMediator.ExpandTree(planner, domain, problem, plan, state, playerActionEdge, 0);
        }

        // Set the root's parent.
        root.parent = oldRoot;

        // Cache the new root's properties.
        problem = root.problem;
        plan = root.plan;
        state = root.state;

        // Update the player model with the player's action and the resulting state predicates.
        playerEnvironmentModel.UpdateModel(playerActionEdge.Action, state.Predicates);

        // Let the mediator take its turn.
        stateNeedsUpdate = true;
    }
	
    // Expand the frontier.
    private void ExpandFrontier( )
    {	
        foreach (StateSpaceEdge edge in root.outgoing)
        {
            StateSpaceNode newNode = StateSpaceMediator.ExpandTree(planner, domain, problem, plan, state, edge, 0);
            frontier.Add(edge, newNode);
        }
    }
}
