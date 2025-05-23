using Distance.LevelSelectAdditions.Helpers;
using UnityEngine;

namespace Distance.LevelSelectAdditions.Scripts
{
	public class LevelSelectWorkshopRateButtonLogic : MonoBehaviour
	{
		private LevelSelectMenuLogic levelSelectMenu_;
		private GameObject visitButton_;
		private GameObject rateButton_;
		private UILabel votingLabel_;

		// Preserve this information if the user has the rate button disabled in the config.
		private float visitButtonNewPositionX_;
		private int visitButtonNewWidth_;
		private float visitButtonOldPositionX_;
		private int visitButtonOldWidth_;


		// Call during LevelSelectMenuLogic.Initialize to match the current `EnableRateWorkshopLevelButton` setting.
		public void Initialize()
		{
			if (!levelSelectMenu_)
			{
				return; // Failed initialization, don't do anything.
			}

			if (visitButton_ != null && rateButton_ != null)
			{
				UIWidget visitWidget = visitButton_.GetComponent<UIWidget>();

				if (Mod.EnableRateWorkshopLevelButton.Value)
				{
					visitButton_.transform.SetGlobalPosX(visitButtonNewPositionX_);
					visitWidget.width = visitButtonNewWidth_;
					rateButton_.SetActive(true);
				}
				else
				{
					visitButton_.transform.SetGlobalPosX(visitButtonOldPositionX_);
					visitWidget.width = visitButtonOldWidth_;
					rateButton_.SetActive(false);
				}
			}
		}


		private void Awake()
		{
			levelSelectMenu_ = GetComponentInParent<LevelSelectMenuLogic>();
			if (!levelSelectMenu_)
			{
				Mod.Log.LogError(nameof(LevelSelectMenuLogic) + " component not found");
				return;
			}

			SetupWorkshopRateButton();
			Initialize(); // Reflect the current `EnableRateWorkshopLevelButton` setting.
		}

		private void Update()
		{
			if (levelSelectMenu_ == null)
			{
				return; // Failed initialization, don't do anything.
			}

			// LAZY MODE:
			// We *could* choose to only update the label when the rating is changed by handling OnRateLevelPanelPop,
			//  (and when choosing a new entry with SelectEntry)... But that's extra hooking that isn't necessary,
			//  so just update the label every cycle.
			SetRatingText();


			if (levelSelectMenu_.IsMenuActiveAndTop_)
			{
				UpdateInput();
			}
		}

		private void UpdateInput()
		{
			if (levelSelectMenu_.ignoreMenuInputForOneFrame_ || !G.Sys.MenuPanelManager_.MenuInputEnabled_ ||
				levelSelectMenu_.SearchButtonSelected_)
			{
				return;
			}

			// TODO: If we want a keybind bound to rating the level, handle input for it here.
		}

		// | --------------------------------------------- |
		// |  Personal Best: N/A                           |
		// |  Updated: 3/2/2021 unplayed                   |
		// |  Difficulty: Advanced                         |
		// |  Workshop Rating: # # # # *                   |
		// |  Votes: 456 (87.65% positive)                 |
		// |_______________________________________________|
		//  _______________________________________________
		// |___________|  Visit Workshop page  |___________|
		//
		//           V  change bottom bar into  V
		//  _______________________________________________
		// |_| My Rating:  None ||  Visit Workshop page  |_|
		//
		private void SetupWorkshopRateButton()
		{
			// Full path: "LevelSelectRoot/Panel - Level Select/Anchor - Center/Left Panel/WorkshopItemControlsPanel/VisitWorkshopButton"
			GameObject visitButton = transform.Find("Panel - Level Select/Anchor - Center/Left Panel/WorkshopItemControlsPanel/VisitWorkshopButton")?.gameObject;
			if (!visitButton)
			{
				Mod.Log.LogError("\"VisitWorkshopButton\" game object not found");
				return;
			}
			visitButton_ = visitButton;

			// Create a copy of VisitWorkshopButton to be our new rate button.
			GameObject rateButton = Instantiate(visitButton, visitButton.transform.parent);
			rateButton.name = "RateWorkshopLevelButton";
			rateButton_ = rateButton;

			// Reposition the button (to the right) to make room for our new button.
			// NOTE: Unlike with the Quick Playlist Rename Button, the positions used here are global, and not tied to the parent panel shape.
			//       Additionally, the width field is an int, so we have less flexibility with resizing. So use precalculated values.
			//  _______________________________________________
			// |___________|  Visit Workshop page  |___________|
			//
			//           V  change bottom bar into  V
			//  _______________________________________________
			// |_| My Rating:  None ||  Visit Workshop page  |_|

			Vector3 visitPos = visitButton.transform.position;
			Vector3 ratePos = visitPos;// rateButton.transform.position;
			// initial x pos: -0.56f

			visitButtonOldPositionX_ = visitPos.x;

			visitPos.x -= (-0.56f - -0.365f); // (+= 0.195f) panel center => far right
			ratePos.x  -= (-0.56f - -0.795f); // (-= 0.235f) panel center => far left

			visitButtonNewPositionX_ = visitPos.x;

			visitButton.transform.position = visitPos;
			rateButton.transform.position = ratePos;


			// Shrink the rate button width so that both buttons fit inside the panel area.
			// NGUI widgets handle all aspects of resizing button components (...thankfully).
			UIWidget visitWidget = visitButton.GetComponent<UIWidget>();
			UIWidget rateWidget = rateButton.GetComponent<UIWidget>();
			// initial width:  184 (~247px)
			// initial height:  24 ( ~28px)

			visitButtonOldWidth_ = visitWidget.width;

			rateWidget.width = 157; // (~209px)

			visitButtonNewWidth_ = visitWidget.width; // Unchanged at the moment


			// What does this actually do? Is it necessary?
			//  (This is coppied from Gsl.Centrifuge.Distance's `VersionNumber`)
			rateButton.ForEachChildObjectDepthFirstRecursive((obj) => {
				obj.SetActive(true);
			});

			// Change the label text for our button.
			// This function does the same thing as the 2 lines below it.
			//LevelSelectMenuLogic.SetChildLabelText(rateButton, "My Rating:  None");
			UILabel rateLabel = rateButton.GetComponentInChildren<UILabel>();
			// Use this as dummy text, so that SetRatingText can choose to only function while the rate button setting is enabled.
			rateLabel.text = "Rate this level"; // OLD BUTTON TEXT that was used before this feature was removed.
			votingLabel_ = rateLabel; // Store the label so that we can update its text at any time.
			SetRatingText(); // Set the initial text for the rate button label.


			// Change the `onClick` event for our button.
			// There are two UIButton components in here (for foreground and background).
			// We want to find the one with the `onClick` event assigned.
			foreach (var button in rateButton.GetComponents<UIButton>())
			{
				if (button.onClick != null && button.onClick.Count > 0)
				{
					button.onClick.Clear();
					button.onClick.Add(new EventDelegate(OnWorkshopRateButtonClicked));

					break;
				}
			}
		}

		private void OnWorkshopRateButtonClicked()
		{
			// Rating is already handled by this *previously-unused* level select function.
			// The `RateLevel` function handles all safety checks like whether this is really
			//  a workshop level or not. So we don't have to do anything extra.
			levelSelectMenu_.RateLevel();
		}

		/// <summary>
		/// Code duplication: <see cref="FinishMenuLogic.SetRatingText"/>
		/// </summary>
		private void SetRatingText()
		{
			// Make sure to honor the Workshop Rating Privacy Mode setting.
			// Note that this is actually handled much earlier on by `WorkshopLevelInfo.WorkshopVote_`,
			//  which returns 'No vote' when in privacy mode, but we want to follow convention with `FinishMenuLogic`.
			if (G.Sys.OptionsManager_.General_.WorkshopRatingPrivacyMode_)
			{
				votingLabel_.text = "Rating: Hidden"; // Interestingly, the "My" is excluded only for this.
				return;
			}

			var entry = levelSelectMenu_.selectedEntry_;
			// CHANGE: Use `[c][/c]` tags to ensure we get the exact color we want.
			switch (entry?.myWorkshopVoteIndex_ ?? SteamworksHelper.VoteIndex_None) // Fallback to 'No vote' if no entry is selected.
			{
			case SteamworksHelper.VoteIndex_For: // Vote for
				votingLabel_.text = "My Rating:  [c][96FFB1]" + GConstants.upVoteChar_ + "[-][/c]";
				break;

			case SteamworksHelper.VoteIndex_Against: // Vote against
				votingLabel_.text = "My Rating:  [c][FF9796]" + GConstants.downVoteChar_ + "[-][/c]";
				break;

			//case SteamworksHelper.VoteIndex_None: // No vote
			default:
				votingLabel_.text = "My Rating:  None";
				break;
			}
		}
	}
}
