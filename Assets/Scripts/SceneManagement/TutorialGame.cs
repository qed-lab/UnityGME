using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

using Mediation.Interfaces;
using Mediation.PlanTools;

public class TutorialGame : MonoBehaviour
{
	private Mediator mediator;

	void Awake()
	{
		mediator = GameObject.Find("Mediator").GetComponent<Mediator>();
	}

	// Called by the mediator when the player successfully executes an action.
	public void UpdateTutorialState(string playerAction)
	{
		// When the player first talks to the wizard, the talk to action has been completed.
		if (playerAction.Equals("(talk-to arthur mel storage)") &&
		    DialogueLua.GetVariable("WIZARD_TUTORIAL_TALK_TO_ACTION_COMPLETED").AsBool == false)
		{
			DialogueLua.SetVariable("WIZARD_TUTORIAL_TALK_TO_ACTION_COMPLETED", true);
			DialogueManager.StartConversation("Wizard");
		}

		// This condition happens when the player has picked up the bucket before talking to Mel.
		else if (playerAction.Equals("(pickup arthur basementbucket storage)") &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_PICK_UP_ACTION_STARTED").AsBool == false)
		{
			DialogueLua.SetVariable("WIZARD_TUTORIAL_PICK_UP_ACTION_COMPLETED", true);
		}

		// When the player picks up the bucket next to the wizard, the pickup action has been completed.
		else if (playerAction.Equals("(pickup arthur basementbucket storage)") &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_PICK_UP_ACTION_STARTED").AsBool == true &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_PICK_UP_ACTION_COMPLETED").AsBool == false)
		{
			DialogueLua.SetVariable("WIZARD_TUTORIAL_PICK_UP_ACTION_COMPLETED", true);
			DialogueManager.StartConversation("Wizard");
		}

		// When the player drops the bucket in the world, the drop action has been completed.
		else if (playerAction.Equals("(drop arthur basementbucket storage)") &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_DROP_ACTION_STARTED").AsBool == true &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_DROP_ACTION_COMPLETED").AsBool == false)
		{
			DialogueLua.SetVariable("WIZARD_TUTORIAL_DROP_ACTION_COMPLETED", true);
			DialogueLua.SetVariable("WIZARD_TUTORIAL_GIVE_ACTION_STARTED", true);

			// At this point we can add that the wizard wants the bucket to the initial state.
			// Create the literal: (wants-item mel basementbucket)
			IPredicate wants_basementbucket = Predicate.BuildPositiveGroundLiteral("wants-item", "mel", "basementbucket");

			// Update the problem.
			mediator.ExpandInitialState(wants_basementbucket);
			DialogueManager.StartConversation("Wizard");
		}

		// When the player gives the bucket to the wizard, the give action has been completed.
		else if (playerAction.Equals("(give arthur basementbucket mel storage)") &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_GIVE_ACTION_STARTED").AsBool == true &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_GIVE_ACTION_COMPLETED").AsBool == false)
		{
			DialogueLua.SetVariable("WIZARD_TUTORIAL_GIVE_ACTION_COMPLETED", true);
			DialogueManager.StartConversation("Wizard");
		}

		// When the player moves through the doorway toward the basement, the go to action has been completed.
		else if (playerAction.Equals("(move-through-doorway arthur storage basement)") &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_GOTO_ACTION_STARTED").AsBool == true &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_GOTO_ACTION_COMPLETED").AsBool == false)
		{
			DialogueLua.SetVariable("WIZARD_TUTORIAL_GOTO_ACTION_COMPLETED", true);
			DialogueLua.SetVariable("WIZARD_TUTORIAL_LOOKAT_ACTION_STARTED", true);

			// Now we have to move merlin over to the other room.
			// Swap two literals in the initial state to make that happen:
			// (at mel storage) for (at mel basement) 

			IPredicate at_storage = Predicate.BuildPositiveGroundLiteral("at", "mel", "storage");
			IPredicate at_basement = Predicate.BuildPositiveGroundLiteral("at", "mel", "basement");

			// Update the planning problem,
			mediator.SwapProblemInitialStateLiterals(at_storage, at_basement);

			// Start the wizard dialogue - which should be at the point the player has entered the room.
			DialogueManager.StartConversation("Wizard");
		}

		// When the player looks at the locked entrance, the look at action has been completed.
		else if (playerAction.Equals("(look-at arthur basementexit basement)") &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_LOOKAT_ACTION_STARTED").AsBool == true &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_LOOKAT_ACTION_COMPLETED").AsBool == false)
		{
			DialogueLua.SetVariable("WIZARD_TUTORIAL_LOOKAT_ACTION_COMPLETED", true);
			DialogueLua.SetVariable("WIZARD_TUTORIAL_USE_ACTION_STARTED", true);
			DialogueManager.StartConversation("Wizard");
		}

		// When the player uses the key on the entrance, the use action has been completed.
		else if (playerAction.Equals("(unlock-entrance arthur basementexitkey basementexit basement)") &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_USE_ACTION_STARTED").AsBool == true &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_USE_ACTION_COMPLETED").AsBool == false)
		{
			DialogueLua.SetVariable("WIZARD_TUTORIAL_USE_ACTION_COMPLETED", true);
			DialogueLua.SetVariable("WIZARD_TUTORIAL_OPEN_ACTION_STARTED", true);
			DialogueManager.StartConversation("Wizard");
		}

		// When the player opens the entrance, the open action has been completed.
		else if (playerAction.Equals("(open arthur basementexit basement)") &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_OPEN_ACTION_STARTED").AsBool == true &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_OPEN_ACTION_COMPLETED").AsBool == false)
		{
			DialogueLua.SetVariable("WIZARD_TUTORIAL_OPEN_ACTION_COMPLETED", true);
			DialogueLua.SetVariable("WIZARD_TUTORIAL_GOTOENTRANCE_ACTION_STARTED", true);
			DialogueManager.StartConversation("Wizard");
		}

		// When the player goes to the entrance, the go to action has been completed.
		else if (playerAction.Equals("(move-through-entrance arthur basement basementexit bar)") &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_GOTOENTRANCE_ACTION_STARTED").AsBool == true &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_GOTOENTRANCE_ACTION_COMPLETED").AsBool == false)
		{
			DialogueLua.SetVariable("WIZARD_TUTORIAL_GOTOENTRANCE_ACTION_COMPLETED", true);


			// Now we have to move merlin over to the bar room.
			// Swap two literals in the initial state to make that happen:
			// (at mel storage) for (at mel basement) 

			IPredicate at_storage = Predicate.BuildPositiveGroundLiteral("at", "mel", "basement");
			IPredicate at_basement = Predicate.BuildPositiveGroundLiteral("at", "mel", "bar");

			// Update the planning problem,
			mediator.SwapProblemInitialStateLiterals(at_storage, at_basement);


			DialogueManager.StartConversation("Wizard");
		}

		// When the player closes the entrance, the close action has been completed.
		else if (playerAction.Equals("(close arthur basemententrance bar)") &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_CLOSE_ACTION_STARTED").AsBool == true &&
		         DialogueLua.GetVariable("WIZARD_TUTORIAL_CLOSE_ACTION_COMPLETED").AsBool == false)
		{
			DialogueLua.SetVariable("WIZARD_TUTORIAL_CLOSE_ACTION_COMPLETED", true);
			DialogueLua.SetVariable("TUTORIAL_FINISHED", true);

			// Here, we need to remove the literal from the goal that was artificially added to disallow the player
			// from leaving the bar.  Create the literal: (at arthur bar)
			IPredicate at_bar = Predicate.BuildPositiveGroundLiteral("at", "arthur", "bar");

			// Update the problem.
			mediator.ContractGoalState(at_bar);

			// Begin game dialogue!
			DialogueManager.StartConversation("Wizard");
		}	

	}
}
