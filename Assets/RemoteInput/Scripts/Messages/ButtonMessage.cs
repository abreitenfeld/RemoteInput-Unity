using UnityEngine;
using UnityEngine.Networking;

namespace RemoteInput.Messages
{
	#pragma warning disable CS0618
	public class ButtonMessage : MessageBase
	{

		#region Defs

		public string ButtonName;
		public bool ButtonState;

		#endregion

		public override void Deserialize (NetworkReader reader)
		{
			this.ButtonName = reader.ReadString ();
			this.ButtonState = reader.ReadBoolean ();
		}

		public override void Serialize (NetworkWriter writer)
		{
			writer.Write (this.ButtonName);
			writer.Write (this.ButtonState);
		}

	}
}