using UnityEngine;
using UnityEngine.Networking;

namespace RemoteInput.Messages
{
	#pragma warning disable CS0618
	public class AxisMessage : MessageBase
	{

		#region Defs

		public string AxisName;
		public float AxisValue;

		#endregion

		public override void Deserialize (NetworkReader reader)
		{
			this.AxisName = reader.ReadString ();
			this.AxisValue = reader.ReadSingle ();
		}

		public override void Serialize (NetworkWriter writer)
		{
			writer.Write (this.AxisName);
			writer.Write (this.AxisValue);
		}

	}
}
