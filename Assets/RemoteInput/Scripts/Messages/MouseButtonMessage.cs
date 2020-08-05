using UnityEngine;
using UnityEngine.Networking;

namespace RemoteInput.Messages
{
	#pragma warning disable CS0618
	public class MouseButtonMessage : MessageBase
	{

		#region Defs

		public int MouseButton;
		public bool ButtonState;

		#endregion

		public override void Deserialize (NetworkReader reader)
		{
			this.MouseButton = reader.ReadInt32 ();
			this.ButtonState = reader.ReadBoolean ();
		}

		public override void Serialize (NetworkWriter writer)
		{
			writer.Write (this.MouseButton);
			writer.Write (this.ButtonState);
		}

	}
}