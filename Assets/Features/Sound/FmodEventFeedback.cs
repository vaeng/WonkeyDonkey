using UnityEngine;
using FMODUnity;
using MoreMountains.Tools;


namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("")]
	[FeedbackHelp("Play an fmod Oneshot")]
	[FeedbackPath("Audio/fmod one shot")]
	public class FmodEventFeedback : MMF_Feedbacks
	{
		public EventReference EventName;

		protected override void CustomPlayFeedback(Vector3 position, float intensity = 1.0f)
		{
			if (!EventName.IsNull)
				FMODUnity.RuntimeManager.PlayOneShot(EventName, position);
		}
	}
}
