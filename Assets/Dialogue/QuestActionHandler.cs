using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

using Mediation.PlanTools;
using Mediation.Interfaces;
using Mediation.Planners;

public class QuestActionHandler : MonoBehaviour
{
	// A flag to determine whether or not the player is in conversation.  All input must cease when that is the case.
	public bool playerInConversation;

	// A game object that is enabled/disabled in front of the UI Command Builder in order to
	// block click-based input from coming to it.
	public GameObject commandBuilderBlocker;

	// Sound to play when the player is given an item by an NPC.
	public AudioClip npcGiveSound;
	public AudioClip startDialogue;
	public AudioClip endDialogue;


	// Internal variables to track when the problem files have been re-written to reflect quest adoption.
	private bool equipQuestHasStarted;
	private bool equipQuestSpokeToBaroness;
	private bool equipQuestSpokeToBaron;
	private bool craftQuestHasStarted;
	private bool pilgrimageQuestHasStarted;
	private bool loveQuestWasPrompted;
	private bool loveQuestHasStarted;
	private bool loveQuestContinuedWithGift;
	private bool loveQuestContinuedWithContract;
	private bool loveQuestSpokeToMajordomo;
	private bool wisdomQuestHasStarted;
	private bool wisdomQuestContinuedWithSkull;
	private bool wisdomQuestContinuedWithCandle;

	private Mediator mediator;
	private StateManager stateManager;
	private Text command;

	void Awake()
	{
		mediator = GameObject.Find("Mediator").GetComponent<Mediator>();
		stateManager = GameObject.Find("Level").GetComponent<StateManager>();
		command = GameObject.Find("Command").GetComponent<Text>();

		equipQuestHasStarted = false;
		equipQuestSpokeToBaroness = false;
		equipQuestSpokeToBaron = false;
		craftQuestHasStarted = false;
		pilgrimageQuestHasStarted = false;
		loveQuestWasPrompted = false;
		loveQuestHasStarted = false;
		loveQuestContinuedWithGift = false;
		loveQuestContinuedWithContract = false;
		loveQuestSpokeToMajordomo = false;
		wisdomQuestHasStarted = false;
		wisdomQuestContinuedWithSkull = false;
		wisdomQuestContinuedWithCandle = false;

		playerInConversation = false;
	}

	// Use this for initialization
	void Start()
	{
		QuestLog.AddQuestStateObserver("Pilgrimage Quest", LuaWatchFrequency.EveryDialogueEntry, OnQuestStateChanged);
		QuestLog.AddQuestStateObserver("Equip Quest", LuaWatchFrequency.EveryDialogueEntry, OnQuestStateChanged);
		QuestLog.AddQuestStateObserver("Wisdom Quest", LuaWatchFrequency.EveryDialogueEntry, OnQuestStateChanged);
		QuestLog.AddQuestStateObserver("Fetch Quest", LuaWatchFrequency.EveryDialogueEntry, OnQuestStateChanged);
		QuestLog.AddQuestStateObserver("Love Quest", LuaWatchFrequency.EveryDialogueEntry, OnQuestStateChanged);
	}

	void OnQuestStateChanged(string title, QuestState newState)
	{
		if (newState == QuestState.Active)
		{
			// DialogueManager.StartConversation("Quest Adopted");
		} 
		// if quest is successfully completed, stop listening for quest state updates
		else if (newState == QuestState.Success)
		{
			// stop observing quest once it's completed
			QuestLog.RemoveQuestStateObserver(title, LuaWatchFrequency.EveryDialogueEntry, OnQuestStateChanged);

			// update count of completed quests
			int completedQuests = DialogueLua.GetVariable("NUMBER_OF_COMPLETED_QUESTS").AsInt;
			DialogueLua.SetVariable("NUMBER_OF_COMPLETED_QUESTS", completedQuests + 1);

			// notify the player that the quest is done
			// DialogueManager.StartConversation("Quest Completed");
		}
	}

	void OnConversationStart(Transform actor)
	{
		playerInConversation = true;
		stateManager.Player.Disable();
		commandBuilderBlocker.SetActive(true);

		if (stateManager.PlayerGameObject.GetComponent<AudioSource>().isActiveAndEnabled)
			stateManager.PlayerGameObject.GetComponent<AudioSource>().PlayOneShot(startDialogue);
	}

	void OnConversationEnd(Transform actor)
	{
		playerInConversation = false;
		stateManager.Player.Enable();
		commandBuilderBlocker.SetActive(false);
		command.text = "";

		if (stateManager.PlayerGameObject.GetComponent<AudioSource>().isActiveAndEnabled)
			stateManager.PlayerGameObject.GetComponent<AudioSource>().PlayOneShot(endDialogue);
	}

	void OnConversationCancelled(Transform actor)
	{
        
	}

	// called when specific conversation line is started
	// @TODO: research Subtitle class
	// Ref: http://www.pixelcrushers.com/dialogue_system/manual/html/class_pixel_crushers_1_1_dialogue_system_1_1_subtitle.html
	void OnConversationLine(Subtitle subtitle)
	{
		if (subtitle.dialogueEntry.conversationID == 6 &&
		    subtitle.dialogueEntry.id == 6)
		{
			stateManager.Player.Disable();
		}


		#region Equip Quest

		// Quartermaster says: The blacksmith at the forge should have a sword ready. 
		// I'm not sure if he's gotten around to making shields yet...
		if (subtitle.dialogueEntry.conversationID == 1 &&
		    subtitle.dialogueEntry.id == 4 &&
		    !equipQuestHasStarted)
		{
			// When the quartermaster says this line, it reflects that he is willing 
			// to receive the sword and shield items.
			equipQuestHasStarted = true;

			// Update the player's mental model.  This dialogue utterance informs the player
			// that the blacksmith is at the forge and that there should be a sword there.
			List<IPredicate> perlocutionaryEffects = new List<IPredicate>();
			perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("at", "peter", "forge"));
			perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("at", "knightsword", "forge"));
			mediator.playerEnvironmentModel.InformLiterals(perlocutionaryEffects);

			// Create the literals:
			// (wants-item ian knightsword)
			// (wants-item ian knightshield)
			// These literals will be added to the initial state of the problem.
			List<IPredicate> newInitLiterals = new List<IPredicate>();
			newInitLiterals.Add(Predicate.BuildPositiveGroundLiteral("wants-item", "ian", "knightsword"));
			newInitLiterals.Add(Predicate.BuildPositiveGroundLiteral("wants-item", "ian", "knightshield"));

			// Create the literals:
			// (has ian knightsword)
			// (has ian knightshield)
			// These literals will be added to the goal state of the problem.
			List<IPredicate> newGoalLiterals = new List<IPredicate>();
			newGoalLiterals.Add(Predicate.BuildPositiveGroundLiteral("has", "ian", "knightsword"));
			newGoalLiterals.Add(Predicate.BuildPositiveGroundLiteral("has", "ian", "knightshield"));

			// Update the problem.
			mediator.ExpandProblem(newInitLiterals, newGoalLiterals);
		}

        // Baroness says: "This shield? Got it from the shop in the townsquare."
        else if (subtitle.dialogueEntry.conversationID == 11 &&
		               subtitle.dialogueEntry.id == 1 &&
		               !equipQuestSpokeToBaroness)
		{
			equipQuestSpokeToBaroness = true;

			// Update the player's mental model.  This dialogue utterance informs the player
			// of the location of the knightshield needed for this quest.
			IPredicate perlocutionaryEffect = Predicate.BuildPositiveGroundLiteral("at", "knightshield", "shop");
			mediator.playerEnvironmentModel.InformLiteral(perlocutionaryEffect);
		}

        // Baron says: "If you are looking for a sword, you should find one at 
        // the forge in the townarch.  I hear the blacksmith is very friendly."
        else if (subtitle.dialogueEntry.conversationID == 11 &&
		               subtitle.dialogueEntry.id == 1 &&
		               !equipQuestSpokeToBaron)
		{
			equipQuestSpokeToBaron = true;

			// Update the player's mental model.  This dialogue utterance informs the player
			// of the location of the knightsword needed for this quest.
			IPredicate perlocutionaryEffect = Predicate.BuildPositiveGroundLiteral("at", "knightsword", "forge");
			mediator.playerEnvironmentModel.InformLiteral(perlocutionaryEffect);
		}
            
        #endregion

        #region Fetch Quest

        // Appraiser says: I hope you're able to bring the item soon!
        else if (subtitle.dialogueEntry.conversationID == 9 &&
		               subtitle.dialogueEntry.id == 8 &&
		               !craftQuestHasStarted)
		{
			// When the appraiser says this line, it reflects that the appraiser
			// is now willing to accept items.
			craftQuestHasStarted = true;

			// Create the literal: (wants-item giovanna hairtonic)
			// This literal will be added to the initial state of the problem.
			IPredicate wants_hairtonic = Predicate.BuildPositiveGroundLiteral("wants-item", "giovanna", "hairtonic");

			// Create the literal: (has giovanna hairtonic)
			// This literal will be added to the goal state of the problem.
			IPredicate has_hairtonic = Predicate.BuildPositiveGroundLiteral("has", "giovanna", "hairtonic");

			// Update the problem.
			mediator.ExpandProblem(wants_hairtonic, has_hairtonic);
		} 

        #endregion

        #region Pilgrimage Quest

        // Orc says: "All trolls eat cats. If you have a problem with it, bring me something else to eat. I hear the fortuneteller's house is full of tasty treats..."
        else if (subtitle.dialogueEntry.conversationID == 3 &&
		               subtitle.dialogueEntry.id == 3 &&
		               !pilgrimageQuestHasStarted)
		{
			// When the orc says this line, it reflects that the orc is now willing to accept other foods.
			pilgrimageQuestHasStarted = true;

			// Update the player's mental model.  This dialogue utterance informs the player
			// that the cupcake is at the hut.
			List<IPredicate> perlocutionaryEffects = new List<IPredicate>();
			perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("at", "tastycupcake", "hut"));
			perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("at", "hutexit", "hut"));
			perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("leadsto", "hutexit", "townarch"));
			mediator.playerEnvironmentModel.InformLiterals(perlocutionaryEffects);

			// Create the literal: (wants-item alli tastycupcake)
			// This literal will be added to the initial state of the problem.
			IPredicate wants_tastycupcake = Predicate.BuildPositiveGroundLiteral("wants-item", "alli", "tastycupcake");

			// Create the literal: (has alli tastycupcake)
			// This literal will be added to the goal state of the problem.
			IPredicate has_tastycupcake = Predicate.BuildPositiveGroundLiteral("has", "alli", "tastycupcake");

			// Update the problem.
			mediator.ExpandProblem(wants_tastycupcake, has_tastycupcake);
		}

        #endregion

        #region Love Quest

        // Lover A says: "Would you help a romantic in need by delivering this letter to my love? 
        // She is on duty at the Governor's Mansion at the cliff beyond the townsquare."
        else if (subtitle.dialogueEntry.conversationID == 7 &&
		               subtitle.dialogueEntry.id == 1 &&
		               !loveQuestWasPrompted)
		{
			loveQuestWasPrompted = true;

			// Update the player's mental model.  This dialogue utterance informs the player
			// of the lover's location and the mansion's location.
			List<IPredicate> perlocutionaryEffects = new List<IPredicate>();

			perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("at", "jordan", "mansion"));
			perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("at", "mansionentrance", "cliff"));
			perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("leadsto", "mansionentrance", "mansion"));
			// perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("at", "mansionexit", "mansion"));
			// perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("leadsto", "mansionexit", "cliff"));
			perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("connected", "townsquare", "cliff"));
			perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("connected", "cliff", "townsquare"));
			perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("doorway", "townsquare", "cliff"));
			perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("doorway", "cliff", "townsqaure"));
			perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("wants-item", "jordan", "loveletter"));

			mediator.playerEnvironmentModel.InformLiterals(perlocutionaryEffects);
		}
            

        // Lover A says: "What luck! Please make sure it arrives quickly."
        else if (subtitle.dialogueEntry.conversationID == 7 &&
		               subtitle.dialogueEntry.id == 11 &&
		               !loveQuestHasStarted)
		{
			// When the lover A says this line, it reflects that the orc is willing to part
			// with its item "loveletter".
			loveQuestHasStarted = true;

			// Create the literal: (willing-to-give-item dorian loveletter)
			// This literal will be added to the initial state of the problem.
			IPredicate willing_to_give_item = Predicate.BuildPositiveGroundLiteral("willing-to-give-item", "dorian", "loveletter");

			// Create the literal: (has jordan loveletter)
			// This literal will be added to the goal state of the problem.
			IPredicate has = Predicate.BuildPositiveGroundLiteral("has", "jordan", "loveletter");

			// Update the problem.
			mediator.ExpandProblem(willing_to_give_item, has);

			// The above update should trigger dorian to give the player the love letter.
			// Play a sound to that effect.
			stateManager.PlayerGameObject.GetComponent<AudioSource>().PlayOneShot(npcGiveSound);
		}


        // Lover B says: "Wonderful! A bouquet of flowers or a ring will do."
        else if (subtitle.dialogueEntry.conversationID == 8 &&
		               subtitle.dialogueEntry.id == 6)
		{
			// Update the player's mental model.  This dialogue utterance informs the player
			// of the lover B's desire for the bouquet or the ring.
			List<IPredicate> perlocutionaryEffects = new List<IPredicate>();

			perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("wants-item", "dorian", "bouquet"));
			perlocutionaryEffects.Add(Predicate.BuildPositiveGroundLiteral("wants-item", "dorian", "rubyring"));

			mediator.playerEnvironmentModel.InformLiterals(perlocutionaryEffects);
		}


        // The Majordomo says: "I heard that the shop in the townsquare recently 
        // acquired a one-of-a-kind ring. I'll have to check it out!"
        else if (subtitle.dialogueEntry.conversationID == 13 &&
		               subtitle.dialogueEntry.id == 1 &&
		               !loveQuestSpokeToMajordomo)
		{
			loveQuestSpokeToMajordomo = true;

			// Update the player's mental model.  This dialogue utterance informs the player
			// of the location of the ring needed for this quest.
			IPredicate perlocutionaryEffect = Predicate.BuildPositiveGroundLiteral("at", "rubyring", "shop");
			mediator.playerEnvironmentModel.InformLiteral(perlocutionaryEffect);
		}

        #endregion

        #region Wisdom Quest

        // Riddler says: What has a head, a tail, is gold, and has no legs? Bring it to me.
        else if (subtitle.dialogueEntry.conversationID == 4 &&
		               subtitle.dialogueEntry.id == 6 &&
		               !wisdomQuestHasStarted)
		{
			// When the riddler says this line, it reflects that the riddler is now willing
			// to accept the item: coin.
			wisdomQuestHasStarted = true;

			// Create the literal: (wants-item james coin)
			// This literal will be added to the initial state of the problem.
			IPredicate wants_coin = Predicate.BuildPositiveGroundLiteral("wants-item", "james", "coin");

			// Create the literal: (has james coin)
			// This literal will be added to the goal state of the problem.
			IPredicate has_coin = Predicate.BuildPositiveGroundLiteral("has", "james", "coin");

			// Update the problem.
			mediator.ExpandProblem(wants_coin, has_coin);
		}




       
		#endregion
	}

	void OnConversationLineEnd(Subtitle subtitle)
	{
		// NOTE: The dialogues that are handled here are done so in this method because they are activated
		// in tandem with the player performing a "give" action.  A player action causes the planner to run,
		// and the code here causes the planner to run.  Thus, it makes sense to wait until the end of the
		// conversation line, such that it gives the planner enough time to run through the first time
		// (the planner needs about 0.016 seconds at the MOST.


		#region Tutorial

		// Mel says: I just created a key that will allow us to get out of here. Here, take it.
		if (subtitle.dialogueEntry.conversationID == 6 &&
		    subtitle.dialogueEntry.id == 31)
		{
			// Create the literal: (wants-item arthur basementexitkey)
			// This literal will be added to the initial state of the problem.
			IPredicate wants_basementexitkey = Predicate.BuildPositiveGroundLiteral("wants-item", stateManager.PlayerName, "basementexitkey");

			// Create the literal: (not (locked basementexit))
			// This literal will be added to the goal state of the problem.
			IPredicate not_locked_basementexit = Predicate.BuildNegativeGroundLiteral("locked", "basementexit");

			// Update the problem.
			mediator.ExpandProblem(wants_basementexitkey, not_locked_basementexit);

			// The above update should trigger Mel giving the player the basementexit key. Play the give sound!
			stateManager.PlayerGameObject.GetComponent<AudioSource>().PlayOneShot(npcGiveSound);
		}
			
		#endregion


		#region Pilgrimage Quest

		// Pilgrimage Quest
		// Orc says: "I LOVE CUPCAKES! This is MUCH better than eating cat. Here, you can have it."
		if (subtitle.dialogueEntry.conversationID == 3 && subtitle.dialogueEntry.id == 5)
		{
			// When the orc says this line, it reflects that the orc is willing to part with its item
			// "ash".  Therefore, change the planning problem to reflect this.

			// Create the literal:  (willing-to-give-item alli ash)
			// This literal will be added to the initial state of the problem.
			IPredicate willing_to_give_item = Predicate.BuildPositiveGroundLiteral("willing-to-give-item", "alli", "ash");


			// Create the literal: (has player ash)
			// This literal will be added to the goal state of the problem.
			IPredicate has = Predicate.BuildPositiveGroundLiteral("has", mediator.problem.Player, "ash");

			// Update the problem.
			mediator.ExpandProblem(willing_to_give_item, has);

			// The above update should trigger the NPC to give the player the cat.
			// Thus, play the appropriate sound.
			stateManager.PlayerGameObject.GetComponent<AudioSource>().PlayOneShot(npcGiveSound);
		}

        #endregion

        #region Wisdom Quest

        // Wisdom Quest
        // Riddler says: Now bring this: I don't have eyes, but once I did see. 
        // Once I had thoughts, but now I'm white and empty.
        else if (subtitle.dialogueEntry.conversationID == 4 &&
		               subtitle.dialogueEntry.id == 8 &&
		               !wisdomQuestContinuedWithSkull)
		{
			// When the riddler says this line, it reflects that the riddler is now willing
			// to accept the item: humanskull
			wisdomQuestContinuedWithSkull = true;

			// Create the literal: (wants-item james humanskull)
			// This literal will be added to the initial state of the problem.
			IPredicate wants_humanskull = Predicate.BuildPositiveGroundLiteral("wants-item", "james", "humanskull");

			// Create the literal: (has james humanskull)
			// This literal will be added to the goal state of the problem.
			IPredicate has_humanskull = Predicate.BuildPositiveGroundLiteral("has", "james", "humanskull");

			// Update the problem.
			mediator.ExpandProblem(wants_humanskull, has_humanskull);
		}

        // Wisdom Quest
        // Riddler says: Lastly, bring an item like this: It is tall when it’s young, 
        // It is short when it’s old. What is it?
        else if (subtitle.dialogueEntry.conversationID == 4 &&
		               subtitle.dialogueEntry.id == 11 &&
		               !wisdomQuestContinuedWithCandle)
		{
			// When the riddler says this line, it reflects that the riddler is now willing
			// to accept the item: candle
			wisdomQuestContinuedWithCandle = true;

			// Create the literla: (wants-item james candle)
			// This literal will be added to the initial state of the problem.
			IPredicate wants_candle = Predicate.BuildPositiveGroundLiteral("wants-item", "james", "candle");

			// Create the literal: (has james candle)
			// This literal will be added to the goal state of the problem.
			IPredicate has_candle = Predicate.BuildPositiveGroundLiteral("has", "james", "candle");

			// Update the problem.
			mediator.ExpandProblem(wants_candle, has_candle);
		}


        #endregion

        #region Love Quest

        // Love Quest
        // Lover B says: Wonderful!  While you find and deliver a gift, I'll go pick out a tuxedo!
        else if (subtitle.dialogueEntry.conversationID == 8 &&
		               subtitle.dialogueEntry.id == 6 &&
		               !loveQuestContinuedWithGift)
		{
			// When the lover B says this line, it reflects that the lover A is now willing
			// to accept gifts.
			loveQuestContinuedWithGift = true;

			// Create the literals:
			// (wants-item dorian rubyring)
			// (wants-item dorian bouquet)
			// These literals will be added to the initial state of the problem.
			IPredicate wants_rubyring = Predicate.BuildPositiveGroundLiteral("wants-item", "dorian", "rubyring");
			IPredicate wants_bouquet = Predicate.BuildPositiveGroundLiteral("wants-item", "dorian", "bouquet");

			// Update the problem.
			List<IPredicate> newInitLiterals = new List<IPredicate>();
			newInitLiterals.Add(wants_rubyring);
			newInitLiterals.Add(wants_bouquet);
			mediator.ExpandProblem(newInitLiterals);

		}

        // Love Quest
        // Lover A says: Wonderful! Next time we speak, I will be officially married.
        else if (subtitle.dialogueEntry.conversationID == 7 &&
		               subtitle.dialogueEntry.id == 15 &&
		               !loveQuestContinuedWithContract)
		{
			// When the lover A says this line, it reflects that lover A is willing to part with
			// its item "lovecontract"
			loveQuestContinuedWithContract = true;

			// Create the literal: (willing-to-give-item dorian lovecontract)
			// This literal will be added to the initial state of the problem.
			IPredicate willing_to_give_item = Predicate.BuildPositiveGroundLiteral("willing-to-give-item", "dorian", "lovecontract");

			// Create the literal: (has jordan lovecontract)
			// This literal will be added to the goal state of the problem.
			IPredicate has = Predicate.BuildPositiveGroundLiteral("has", "jordan", "lovecontract");

			// Update the problem.
			mediator.ExpandProblem(willing_to_give_item, has);

			// The above update should trigger dorian to give the player the love contract.
			// Play a sound to that effect.
			stateManager.PlayerGameObject.GetComponent<AudioSource>().PlayOneShot(npcGiveSound);

		}

        #endregion


        #region End Game


        else if (subtitle.dialogueEntry.conversationID == 6 && subtitle.dialogueEntry.id == 10)
		{
			// When the wizard says this line, the player has won the game!
			// Transition to the end scene.

			SceneManager.LoadScene("End Screen");
		}

		#endregion

	}

	void OnConversationLineCancelled(Subtitle subtitle)
	{	
		Debug.Log("OnConversationLineCancelled");
	}
}
