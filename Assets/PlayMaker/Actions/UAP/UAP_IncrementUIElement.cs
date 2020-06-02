using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{

	[ActionCategory("UAP")]
	[Tooltip("This is a custom Playmaker Action that works ONLY with UAP Accessibility Pluging")]
	public class UAP_IncrementUIElement : FsmStateAction
	{

		// Code that runs on entering the state.
		public override void OnEnter()
		{
			Finish();
		}


	}

}
