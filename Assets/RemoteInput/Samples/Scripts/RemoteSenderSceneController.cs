using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using RemoteInput;

public class RemoteSenderSceneController : MonoBehaviour {

	#region Defs

	public RemoteInputSender InputSender;
	public InputField IPField;
	public InputField PortField;
	public Text StatusLabel;
	public Button ConnectionButton;

	private const string IPSettingKey = "IP";
	private const string PortSettingKey = "Port";

	#endregion

	#region Methods

	public virtual void Connect()
	{
		if (!this.InputSender.isConnected) {
			// save settings
			PlayerPrefs.SetString (IPSettingKey, this.IPField.text);
			PlayerPrefs.SetInt (PortSettingKey,  int.Parse(this.PortField.text));
			PlayerPrefs.Save ();

			this.InputSender.Connect (this.IPField.text, int.Parse(this.PortField.text));
		}
		this.UpdateUI ();
	}

	public virtual void Disconnect()
	{
		if (this.InputSender.isConnected) {
			this.InputSender.Shutdown ();
			this.ConnectionButton.GetComponentInChildren<Text>().text = "Connect";
		}
		this.UpdateUI ();
	}

	protected virtual void UpdateUI ()
	{
		if (this.InputSender.isConnected) {
			this.ConnectionButton.GetComponentInChildren<Text>().text = "Disconnect";
		} else {
			this.ConnectionButton.GetComponentInChildren<Text> ().text = "Connect";
		}
	}

	protected virtual void UpdateStatusLabel() 
	{
		StringBuilder sb = new StringBuilder ();

		sb.AppendLine ("Connected: " + InputSender.isConnected.ToString ());
		if (InputSender.isConnected) {
			sb.AppendLine ("RTT: " + InputSender.Client.GetRTT ().ToString ());
		}

		string[] joysticks = Input.GetJoystickNames ();
		if (joysticks.Length > 0) {
			sb.AppendLine ("Connected josticks:");
			for (int i = 0; i < joysticks.Length; i++) {
				sb.AppendLine (joysticks [i]);
			}
		} 

		this.StatusLabel.text = sb.ToString ();
	}

	#endregion

	#region Events

	protected virtual void Awake()
	{
		// restore preferences
		this.IPField.text = PlayerPrefs.HasKey (IPSettingKey) ? PlayerPrefs.GetString (IPSettingKey) : string.Empty;
		this.PortField.text = (PlayerPrefs.HasKey (PortSettingKey) ? PlayerPrefs.GetInt (PortSettingKey) : RemoteInputReceiver.DefaultPort).ToString();

		// add click handler
		this.ConnectionButton.onClick.AddListener(() => {
			if (this.InputSender.isConnected) {
				this.Disconnect();
			}
			else {
				this.Connect();
			}
		});
	}

	protected virtual void Update()
	{
		this.UpdateUI ();
		this.UpdateStatusLabel ();
	}

	#endregion

}
