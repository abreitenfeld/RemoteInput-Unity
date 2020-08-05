using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using RemoteInput.Messages;

namespace RemoteInput
{

	#pragma warning disable CS0618
	[AddComponentMenu ("Remote Input/RemoteInputSender")]
	public class RemoteInputSender : MonoBehaviour
	{

		#region Defs

		public SendFlags SendFlag = SendFlags.Buttons;

		[Tooltip ("Reconnect on connection loss.")]
		public bool AutoReconnect = false;

		[Tooltip ("Send rate of input updates per second.")]
		public int SendRate = 20;

		[Tooltip ("List of buttons specified by the name.")]
		public string[] Buttons = new string[] { "Fire1", "Fire2" };

		[Tooltip ("List of axis specified by the name.")]
		public string[] Axis = new string[] { "Horizontal", "Vertical" };

		[Tooltip ("List of keys specified by the name.")]
		public KeyCode[] Keys = new KeyCode[] { };

		[Tooltip ("List of mouse buttons specified by the number.")]
		public int[] MouseButtons = new int[] { };

		[System.Flags]
		public enum SendFlags
		{
			Buttons = 1,
			Keys = 2,
			MouseButtons = 4,
			Axis = 8,
			Acceleration = 16,
			Compass = 32,
			Gyroscope = 64
		}

		protected float _sendTimer;

		protected bool[] _buttonPressed;
		protected bool[] _keyPressed;
		protected bool[] _mouseButtonPressed;

		#endregion

		#region Properties

		public NetworkClient Client { get; protected set; }

		/// <summary>
		/// Gets a value indicating whether this remote input sender is connected to a receiver.
		/// </summary>
		/// <value><c>true</c> if is connected; otherwise, <c>false</c>.</value>
		public virtual bool isConnected { 
			get { 
				return this.Client != null ? this.Client.isConnected : false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this remote input sender is sending input updates.
		/// </summary>
		/// <value><c>true</c> if is sending; otherwise, <c>false</c>.</value>
		public virtual bool isSending { get; protected set; }

		/// <summary>
		/// Gets the remote reveiver ip.
		/// </summary>
		/// <value>The remote ip.</value>
		public string RemoteIp { get; protected set; }

		/// <summary>
		/// Gets the remote receiver port.
		/// </summary>
		/// <value>The remote port.</value>
		public int RemotePort { get; protected set; }

		#endregion

		#region Methods

		#region Connectivity

		/// <summary>
		/// Reconnects the client.
		/// </summary>
		protected virtual void Reconnect ()
		{
			this.Connect (this.RemoteIp, this.RemotePort);
		}

		/// <summary>
		/// Create a client and connect to the server port.
		/// </summary>
		/// <param name="ip">Ip.</param>
		/// <param name="port">Port.</param>
		public virtual void Connect (string ip, int port)
		{
			this.RemoteIp = ip;
			this.RemotePort = port;
			if (this.Client == null) {
				this.Client = new NetworkClient ();
				ConnectionConfig config = new ConnectionConfig ();
				config.MaxConnectionAttempt = 100;
				config.AddChannel (QosType.ReliableSequenced);
				this.Client.Configure (config, 1);

				// register handler
				this.Client.RegisterHandler (MsgType.Connect, this.OnServerConnect);   
				this.Client.RegisterHandler (MsgType.Disconnect, this.OnServerDisconnect);
				this.Client.RegisterHandler (MsgType.Error, this.OnClientError);

				this.Client.Connect (ip, port);
			} else {
				this.Client.ReconnectToNewHost (ip, port);
			}
		}

		/// <summary>
		/// Disconnect this instance from the receiver.
		/// </summary>
		public virtual void Shutdown ()
		{
			if (this.Client != null) {
				// unregister handler
				this.Client.UnregisterHandler (MsgType.Connect);   
				this.Client.UnregisterHandler (MsgType.Disconnect);
				this.Client.UnregisterHandler (MsgType.Error);
				this.Client.Disconnect ();
				this.Client.Shutdown ();
				this.Client = null;
				this.isSending = false;
			}
		}

		#endregion

		#region Input Tracking

		/// <summary>
		/// Tracks the compass.
		/// </summary>
		protected virtual void TrackCompass ()
		{
			if (!Input.compass.enabled) {
				Input.compass.enabled = true;
				Input.location.Start ();
			}
			this.SendCompass (Input.compass);
		}

		protected virtual void TrackGyroscope ()
		{
			if (!Input.gyro.enabled) {
				Input.gyro.enabled = true;
			}
			this.SendGyroscope (Input.gyro);
		}

		/// <summary>
		/// Tracks the mouse buttons.
		/// </summary>
		protected virtual void TrackMouseButtons ()
		{
			for (int i = 0; i < this.MouseButtons.Length; i++) {
				// check mpuse button press
				if (this._mouseButtonPressed [i] != Input.GetMouseButton (this.MouseButtons [i])) {
					this._mouseButtonPressed [i] = Input.GetMouseButton (this.MouseButtons [i]);
					this.SendMouseButtonData (this.MouseButtons [i], this._mouseButtonPressed [i]);
				}
			}
		}

		/// <summary>
		/// Tracks the accelerometer.
		/// </summary>
		protected virtual void TrackAcceleration ()
		{
			this.SendAcceleration (Input.acceleration);
		}

		/// <summary>
		/// Tracks the provided keys.
		/// </summary>
		protected virtual void TrackKeys ()
		{
			for (int i = 0; i < this.Keys.Length; i++) {
				// check button press
				if (this._keyPressed [i] != Input.GetKey (this.Keys [i])) {
					this._keyPressed [i] = Input.GetKey (this.Keys [i]);
					this.SendKeyData (this.Keys [i], this._keyPressed [i]);
				}
			}
		}

		/// <summary>
		/// Tracks the provided buttons.
		/// </summary>
		protected virtual void TrackButtons ()
		{
			for (int i = 0; i < this.Buttons.Length; i++) {
				// check button press
				if (this._buttonPressed [i] != Input.GetButton (this.Buttons [i])) {
					this._buttonPressed [i] = Input.GetButton (this.Buttons [i]);
					this.SendButtonData (this.Buttons [i], this._buttonPressed [i]);
				}
			}
		}

		/// <summary>
		/// Tracks the provided axis.
		/// </summary>
		protected virtual void TrackAxis ()
		{
			for (int i = 0; i < this.Axis.Length; i++) {
				this.SendAxisData (this.Axis [i], Input.GetAxis (this.Axis [i]));
			}
		}

		#endregion

		#region Messaging

		protected virtual void SendKeyData (KeyCode key, bool value)
		{
			// send button data
			KeyMessage msg = new KeyMessage ();
			msg.Key = key;
			msg.KeyState = value;
			this.Client.Send (InputMsgType.KeyPress, msg);
		}

		protected virtual void SendButtonData (string name, bool value)
		{
			// send button data
			ButtonMessage msg = new ButtonMessage ();
			msg.ButtonName = name;
			msg.ButtonState = value;
			this.Client.Send (InputMsgType.ButtonPress, msg);
		}

		protected virtual void SendMouseButtonData (int mouseButton, bool value)
		{
			// send button data
			MouseButtonMessage msg = new MouseButtonMessage ();
			msg.MouseButton = mouseButton;
			msg.ButtonState = value;
			this.Client.Send (InputMsgType.MouseButtonPress, msg);
		}

		protected virtual void SendAxisData (string name, float value)
		{
			// send axis data
			AxisMessage msg = new AxisMessage ();
			msg.AxisName = name;
			msg.AxisValue = value;
			this.Client.Send (InputMsgType.Axis, msg);
		}

		protected virtual void SendAcceleration (Vector3 acceleration)
		{
			// send acceleration data
			AccelerationMessage msg = new AccelerationMessage ();
			msg.Acceleration = acceleration;
			this.Client.Send (InputMsgType.Acceleration, msg);
		}
			
		protected virtual void SendCompass (Compass compass)
		{
			// send compass data
			CompassMessage msg = new CompassMessage ();
			msg.trueHeading = compass.trueHeading;
			msg.timestamp = compass.timestamp;
			msg.headingAccuracy = compass.headingAccuracy;
			msg.magneticHeading = compass.magneticHeading;
			msg.rawVector = compass.rawVector;
			this.Client.Send (InputMsgType.Compass, msg);		
		}

		protected virtual void SendGyroscope (Gyroscope gyro)
		{
			// send gyroscope data
			GyroMessage msg = new GyroMessage ();
			msg.attitude = gyro.attitude;
			msg.enabled = gyro.enabled;
			msg.rotationRate = gyro.rotationRate;
			msg.rotationRateUnbiased = gyro.rotationRateUnbiased;
			msg.updateInterval = gyro.updateInterval;
			msg.userAcceleration = gyro.userAcceleration;
			this.Client.Send (InputMsgType.Gyroscope, msg);		
		}

		#endregion

		#endregion

		#region Events

		protected virtual void Awake ()
		{
			// initialize data structures to track button and key pressed state
			this._buttonPressed = new bool[this.Buttons.Length];
			this._keyPressed = new bool[this.Keys.Length];
			this._mouseButtonPressed = new bool[this.MouseButtons.Length];
		}

		protected virtual void Update ()
		{
			if (this.isConnected && this.isSending) {
				if ((this.SendFlag & SendFlags.Buttons) != 0)
					this.TrackButtons ();

				if ((this.SendFlag & SendFlags.Keys) != 0)
					this.TrackKeys ();

				if ((this.SendFlag & SendFlags.MouseButtons) != 0)
					this.TrackMouseButtons ();
				
				// update timer
				this._sendTimer -= Time.deltaTime;
				// check if timer is elapsed
				if (this._sendTimer <= 0f) {
					if ((this.SendFlag & SendFlags.Acceleration) != 0)
						this.TrackAcceleration ();
					
					if ((this.SendFlag & SendFlags.Compass) != 0)
						this.TrackCompass ();

					if ((this.SendFlag & SendFlags.Gyroscope) != 0)
						this.TrackGyroscope ();

					if ((this.SendFlag & SendFlags.Axis) != 0)
						this.TrackAxis ();
				
					// restart timer
					this._sendTimer = (1f / (float)this.SendRate) + this._sendTimer;
				}
			}
		}

		protected virtual void OnServerConnect (NetworkMessage netMsg)
		{
			#if DEBUG
			Debug.Log ("Connected to server");
			#endif
			this.isSending = true;
			this._sendTimer = 0f;
		}

		protected virtual void OnServerDisconnect (NetworkMessage netMsg)
		{
			#if DEBUG
			Debug.Log ("Disconnected from server");
			#endif
			if (this.AutoReconnect) {
				this.Reconnect ();
			}
		}

		protected virtual void OnClientError (NetworkMessage netMsg)
		{

		}

		#endregion
	}

}