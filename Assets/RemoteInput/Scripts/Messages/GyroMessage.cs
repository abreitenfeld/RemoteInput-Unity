using UnityEngine;
using UnityEngine.Networking;

namespace RemoteInput.Messages
{
	#pragma warning disable CS0618
	public class GyroMessage : MessageBase
	{
		#region Defs

		public Quaternion attitude;
		public bool enabled;
		public Vector3 gravity;
		public Vector3 rotationRate;
		public Vector3 rotationRateUnbiased;
		public float updateInterval;
		public Vector3 userAcceleration;

		#endregion

		public override void Deserialize (NetworkReader reader)
		{
			this.attitude = reader.ReadQuaternion ();
			this.enabled = reader.ReadBoolean ();
			this.gravity = reader.ReadVector3 ();
			this.rotationRate = reader.ReadVector3 ();
			this.rotationRateUnbiased = reader.ReadVector3 ();
			this.updateInterval = reader.ReadSingle ();
			this.userAcceleration = reader.ReadVector3 ();
		}

		public override void Serialize (NetworkWriter writer)
		{
			writer.Write (this.attitude);
			writer.Write (this.enabled);
			writer.Write (this.gravity);
			writer.Write (this.rotationRate);
			writer.Write (this.rotationRateUnbiased);
			writer.Write (this.updateInterval);
			writer.Write (this.userAcceleration);
		}
	}
}