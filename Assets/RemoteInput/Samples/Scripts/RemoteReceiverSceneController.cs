using UnityEngine;
using RemoteInput;
using System.Collections;

public class RemoteReceiverSceneController : MonoBehaviour {

	#region Defs

	public RemoteInput.RemoteInputReceiver InputReceiver;
	public Transform Gun;
	public Transform Mesh;
	public ParticleSystem Fire;

	#endregion

	#region Events

	protected void Update () {
		if (this.InputReceiver.isReady) {
			// track mouse button
			if (this.InputReceiver.GetMouseButtonDown (0)) {
				this.Fire.Play ();
			} else if (this.InputReceiver.GetMouseButtonUp (0)) {
				this.Fire.Stop ();
			}

			if (this.InputReceiver.GetMouseButton (0)) {
				float duration = 0.2f;
				float offset = Mathf.Sin(((Time.time % duration) / duration) * 2f * Mathf.PI) * 0.03f;
				this.Mesh.transform.localPosition = Vector3.forward * offset;
			}
			// track acceleration and compass
			Vector3 euler = Vector3.zero;
			euler.y = this.InputReceiver.compass.trueHeading;
			euler.z = -this.InputReceiver.gyro.attitude.eulerAngles.y;
			euler.x = -this.InputReceiver.gyro.attitude.eulerAngles.x;
			this.Gun.transform.localEulerAngles = euler;
		}
	}

	#endregion
}
