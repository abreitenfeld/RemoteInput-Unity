using UnityEngine;
using UnityEngine.Networking;

namespace RemoteInput.Messages
{
	#pragma warning disable CS0618
	public class KeyMessage : MessageBase
	{

		#region Defs

		public KeyCode Key;
		public bool KeyState;

		#endregion

		public override void Deserialize (NetworkReader reader)
		{
			this.Key = (KeyCode)System.Enum.Parse (typeof(KeyCode), reader.ReadString ());
			this.KeyState = reader.ReadBoolean ();
		}

		public override void Serialize (NetworkWriter writer)
		{
			writer.Write (this.Key.ToString ());
			writer.Write (this.KeyState);
		}

	}
}