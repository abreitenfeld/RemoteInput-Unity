using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;

namespace RemoteInput
{
	#pragma warning disable CS0618
	[CustomEditor (typeof(RemoteInputSender))]
	public class RemoteInputSenderEditor : Editor
	{

		#region Defs

		private SerializedProperty _sendFlag;
		private AnimBool _showButtonFields;
		private AnimBool _showMouseButtonFields;
		private AnimBool _showKeysFields;
		private AnimBool _showAxisFields;

		#endregion

		private static bool FlagIsSet (SerializedProperty prop, RemoteInputSender.SendFlags flag)
		{
			return (prop.intValue & (int)flag) != 0;
		}

		protected void OnEnable ()
		{
			this._sendFlag = serializedObject.FindProperty ("SendFlag");
			this._showButtonFields = new AnimBool (FlagIsSet (this._sendFlag, RemoteInputSender.SendFlags.Buttons));
			this._showMouseButtonFields = new AnimBool (FlagIsSet (this._sendFlag, RemoteInputSender.SendFlags.MouseButtons));
			this._showKeysFields = new AnimBool (FlagIsSet (this._sendFlag, RemoteInputSender.SendFlags.Keys));
			this._showAxisFields = new AnimBool (FlagIsSet (this._sendFlag, RemoteInputSender.SendFlags.Axis));

			this._showButtonFields.valueChanged.AddListener (this.Repaint);
			this._showMouseButtonFields.valueChanged.AddListener (this.Repaint);
			this._showKeysFields.valueChanged.AddListener (this.Repaint);
			this._showAxisFields.valueChanged.AddListener (this.Repaint);
		}

		public override void OnInspectorGUI ()
		{
			// Update the serializedProperty
			serializedObject.Update ();

			this._sendFlag.intValue = (int)((RemoteInputSender.SendFlags)EditorGUILayout.EnumMaskField (
				"Send Flags", (RemoteInputSender.SendFlags)this._sendFlag.intValue));

			EditorGUILayout.IntSlider (serializedObject.FindProperty ("SendRate"), 1, 100);
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("AutoReconnect"));

			this._showButtonFields.target = FlagIsSet (this._sendFlag, RemoteInputSender.SendFlags.Buttons);
			this._showMouseButtonFields.target = FlagIsSet (this._sendFlag, RemoteInputSender.SendFlags.MouseButtons);
			this._showKeysFields.target = FlagIsSet (this._sendFlag, RemoteInputSender.SendFlags.Keys);
			this._showAxisFields.target = FlagIsSet (this._sendFlag, RemoteInputSender.SendFlags.Axis);

			// button list
			if (EditorGUILayout.BeginFadeGroup (this._showButtonFields.faded)) {
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("Buttons"), true);
			}
			EditorGUILayout.EndFadeGroup ();

			// mouse button list
			if (EditorGUILayout.BeginFadeGroup (this._showMouseButtonFields.faded)) {
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("MouseButtons"), true);
			}
			EditorGUILayout.EndFadeGroup ();

			// axis list
			if (EditorGUILayout.BeginFadeGroup (this._showAxisFields.faded)) {
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("Axis"), true);
			}
			EditorGUILayout.EndFadeGroup ();

			// keys list
			if (EditorGUILayout.BeginFadeGroup (this._showKeysFields.faded)) {
				EditorGUILayout.PropertyField (serializedObject.FindProperty ("Keys"), true);
			}
			EditorGUILayout.EndFadeGroup ();

			// Apply changes to the serializedProperty
			serializedObject.ApplyModifiedProperties ();
		}
		
	}

}