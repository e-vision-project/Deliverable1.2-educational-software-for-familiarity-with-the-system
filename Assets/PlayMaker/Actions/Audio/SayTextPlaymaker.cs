using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{

	[ActionCategory(ActionCategory.Audio)]
	[Tooltip("Uses the UAP voice to say a selected text.")]
	public class SayTextPlaymaker : FsmStateAction
	{
        public string TextToSay;

		// Code that runs on entering the state.
		public override void OnEnter()
		{
          //  UAP_AccessibilityManager.StopSpeaking();
            UAP_AccessibilityManager.Say(TextToSay);
			Finish();
		}

        

    }

}
