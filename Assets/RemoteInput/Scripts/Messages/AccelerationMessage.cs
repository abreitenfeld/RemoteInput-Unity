using UnityEngine;
using UnityEngine.Networking;

namespace RemoteInput.Messages
{
	#pragma warning disable CS0618
	public class AccelerationMessage : MessageBase
	{
		#region Defs

		public Vector3 Acceleration;

		#endregion

		public override void Deserialize (NetworkReader reader)
		{
			this.Acceleration = reader.ReadVector3 ();
		}

		public override void Serialize (NetworkWriter writer)
		{
			writer.Write (this.Acceleration);
		}
	}
}

