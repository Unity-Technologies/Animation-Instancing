using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentSample : MonoBehaviour 
{
	public GameObject attachment = null;
	private bool initialize = false;
	void Update()
	{
		// the attaching should be after the master's initialize, so we put it in the first update.
		if (!initialize)
		{
			initialize = true;
			AnimationInstancing.AnimationInstancing instance = GetComponent<AnimationInstancing.AnimationInstancing>();
			if (instance)
            {  
				int count = instance.GetAnimationCount();
				instance.PlayAnimation(Random.Range(0, count));
                AnimationInstancing.AnimationInstancing attachmentScript = attachment.GetComponent<AnimationInstancing.AnimationInstancing>();
				instance.Attach("ik_hand_r", attachmentScript);

				// Deattach the attachment
				// instance.Deattach(attachmentScript);
			}						
		}
	}
}
