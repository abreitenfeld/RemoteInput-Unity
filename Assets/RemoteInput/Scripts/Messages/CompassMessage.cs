using UnityEngine;
using UnityEngine.Networking;

namespace RemoteInput.Messages
{
	#pragma warning disable CS0618
	public class CompassMessage : MessageBase
	{
		#region Defs

		public float trueHeading;
		public double timestamp;
		public float magneticHeading;
		public float headingAccuracy;
		public Vector3 rawVector;

		#endregion

		public override void Deserialize (NetworkReader reader)
		{
			this.trueHeading = reader.ReadSingle ();
			this.timestamp = reader.ReadDouble ();
			this.magneticHeading = reader.ReadSingle ();
			this.headingAccuracy = reader.ReadSingle ();
			this.rawVector = reader.ReadVector3 ();
		}

		public override void Serialize (NetworkWriter writer)
		{
			writer.Write (this.trueHeading);
			writer.Write (this.timestamp);
			writer.Write (this.magneticHeading);
			writer.Write (this.headingAccuracy);
			writer.Write (this.rawVector);
		}
	}
}