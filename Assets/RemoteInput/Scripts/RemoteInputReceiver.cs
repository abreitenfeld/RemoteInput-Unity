using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using RemoteInput.Messages;

namespace RemoteInput
{
	#pragma warning disable CS0618
	[AddComponentMenu ("Remote Input/RemoteInputReceiver")]
	public class RemoteInputReceiver : MonoBehaviour
	{

		#region Defs

		public bool InitializeOnLoad = true;
		public bool PreventDestroyOnLoad = true;
		public int Port = DefaultPort;

		protected bool _isReady;
		protected NetworkConnection _remoteConnection;

		private readonly Dictionary<string, InputState> _buttonState = new Dictionary<string, InputState> ();
		private readonly Dictionary<int, InputState> _mouseButtonState = new Dictionary<int, InputState> ();
		private readonly Dictionary<KeyCode, InputState> _keyState = new Dictionary<KeyCode, InputState> ();
		private readonly Dictionary<string, float> _axis = new Dictionary<string, float> ();

		public const int DefaultPort = 4444;

		protected enum InputState
		{
			None = 0,
			BecomePressed = 1,
			IsDown = 2,
			IsPressed = 4,
			IsUp = 8,
			BecomeIdle = 16,
		}

		#endregion

		#region Classes

		public sealed class Compass
		{
			#region Properties

			public float trueHeading { get; internal set; }

			public Vector3 rawVector { get; internal set; }

			public double timestamp { get; internal set; }

			public float magneticHeading { get; internal set; }

			public float headingAccuracy { get; internal set; }

			#endregion

			#region Internal Constructor

			internal Compass ()
			{
			}

			#endregion
		}

		public sealed class Gyroscope
		{
			#region Properties

			public Quaternion attitude { get; internal set; }

			public bool enabled { get; internal set; }

			public Vector3 gravity { get; internal set; }

			public Vector3 rotationRate { get; internal set; }

			public Vector3 rotationRateUnbiased { get; internal set; }

			public float updateInterval { get; internal set; }

			public Vector3 userAcceleration { get; internal set; }

			#endregion

			#region Internal Constructor

			internal Gyroscope ()
			{
			}

			#endregion
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the compass.
		/// </summary>
		/// <value>The compass.</value>
		public RemoteInputReceiver.Compass compass { get; protected set; }

		/// <summary>
		/// Gets the gyroscope.
		/// </summary>
		/// <value>The gyroscope.</value>
		public RemoteInputReceiver.Gyroscope gyro { get; protected set; }

		/// <summary>
		/// Gets a value indicating whether this remote input receiver is started.
		/// </summary>
		/// <value><c>true</c> if is started; otherwise, <c>false</c>.</value>
		public virtual bool isStarted { get; protected set; }

		/// <summary>
		/// Gets a value indicating whether this input receiver is ready to receive input signals.
		/// </summary>
		/// <value><c>true</c> if is ready; otherwise, <c>false</c>.</value>
		public virtual bool isReady { 
			get {
				return this._isReady;
			}
			set { 
				if (this._isReady != value) {
					this._isReady = value;
					if (this._isReady) {
						if (this._remoteConnection != null)
							NetworkServer.SetClientReady (this._remoteConnection);
					} else {
						NetworkServer.SetAllClientsNotReady ();
					}
				}
			} 
		}

		#endregion

		#region Methods

		#region Connectivity

		/// <summary>
		/// Setups the server and listen to an incomming connection.
		/// </summary>
		public void SetupServer ()
		{
			ConnectionConfig config = new ConnectionConfig ();
			config.AddChannel (QosType.ReliableSequenced);
			NetworkServer.Configure (config, 1);

			// register handler
			NetworkServer.RegisterHandler (MsgType.Connect, this.OnClientConnect);
			NetworkServer.RegisterHandler (MsgType.Disconnect, this.OnClientDisconnect);
			NetworkServer.RegisterHandler (MsgType.Error, this.OnServerError);

			// register application-specific handlers
			// buttons
			NetworkServer.RegisterHandler (InputMsgType.ButtonPress, this.OnReceiveButton);
			// mouse buttons
			NetworkServer.RegisterHandler (InputMsgType.MouseButtonPress, this.OnReceiveMouseButton);
			// keys
			NetworkServer.RegisterHandler (InputMsgType.KeyPress, this.OnReceiveKey);
			// axis
			NetworkServer.RegisterHandler (InputMsgType.Axis, this.OnReceiveAxis);
			// acceleration
			NetworkServer.RegisterHandler (InputMsgType.Acceleration, this.OnReceiveAcceleration);
			// compass
			NetworkServer.RegisterHandler (InputMsgType.Compass, this.OnReceiveCompass);
			// gyroscope
			NetworkServer.RegisterHandler (InputMsgType.Gyroscope, this.OnReceiveGyroscope);

			this.isStarted = NetworkServer.Listen (this.Port);

			#if UNITY_EDITOR
			Debug.LogFormat("Listening {0}@{1}", this.GetLocalIP(), this.Port);
			#endif
		}

		/// <summary>
		/// Disconnects the connection to remote input sender.
		/// </summary>
		public virtual void DisconnectRemote ()
		{
			if (this.isStarted) {
				NetworkServer.DisconnectAll ();

				// unregister handler
				NetworkServer.UnregisterHandler (MsgType.Connect);
				NetworkServer.UnregisterHandler (MsgType.Disconnect);
				NetworkServer.UnregisterHandler (MsgType.Error);

				// unregister application-specific handlers
				// buttons
				NetworkServer.UnregisterHandler (InputMsgType.ButtonPress);
				// mouse buttons
				NetworkServer.UnregisterHandler (InputMsgType.MouseButtonPress);
				// keys
				NetworkServer.UnregisterHandler (InputMsgType.KeyPress);
				// axis
				NetworkServer.UnregisterHandler (InputMsgType.Axis);
				// acceleration
				NetworkServer.UnregisterHandler (InputMsgType.Acceleration);
				// compass
				NetworkServer.UnregisterHandler (InputMsgType.Compass);
				// gyroscope
				NetworkServer.UnregisterHandler (InputMsgType.Gyroscope);
			}
		}

		/// <summary>
		/// Gets the local IP of the remote input receiver.
		/// </summary>
		/// <returns>The IP as string</returns>
		public virtual string GetLocalIP()
		{
			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress ip in host.AddressList) {
				if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
				{
					return ip.ToString();
				}
			}
			return string.Empty;
		}

		#endregion

		#region Input Access

		/// <summary>
		/// Gets the linear acceleration.
		/// </summary>
		/// <value>The acceleration.</value>
		public virtual Vector3 acceleration { get; protected set; }

		/// <summary>
		/// Returns true during the frame the user starts pressing down the key identified by name.
		/// </summary>
		/// <returns><c>true</c>, if key down was gotten, <c>false</c> otherwise.</returns>
		/// <param name="name">Name.</param>
		public virtual bool GetKeyDown (string name)
		{
			return this.GetKeyDown ((KeyCode)System.Enum.Parse (typeof(KeyCode), name));
		}

		/// <summary>
		/// Returns true during the frame the user starts pressing down the key identified by key code.
		/// </summary>
		/// <returns><c>true</c>, if key down was gotten, <c>false</c> otherwise.</returns>
		/// <param name="key">Key.</param>
		public virtual bool GetKeyDown (KeyCode key)
		{
			return this._keyState.ContainsKey (key) ? (this._keyState [key] & InputState.IsDown) != 0 : false;
		}

		/// <summary>
		/// Returns true while the user holds down the key identified by name.
		/// </summary>
		/// <returns><c>true</c>, if key was gotten, <c>false</c> otherwise.</returns>
		/// <param name="name">Name.</param>
		public virtual bool GetKey (string name)
		{
			return this.GetKey ((KeyCode)System.Enum.Parse (typeof(KeyCode), name));
		}

		/// <summary>
		/// Returns true while the user holds down the key identified by key code.
		/// </summary>
		/// <returns><c>true</c>, if key was gotten, <c>false</c> otherwise.</returns>
		/// <param name="key">Key.</param>
		public virtual bool GetKey (KeyCode key)
		{
			return this._keyState.ContainsKey (key) ? (this._keyState [key] & InputState.IsPressed) != 0 : false;
		}

		/// <summary>
		/// Returns true during the frame the user releases the key identified by name.
		/// </summary>
		/// <returns><c>true</c>, if key up was gotten, <c>false</c> otherwise.</returns>
		/// <param name="name">Name.</param>
		public virtual bool GetKeyUp (string name)
		{
			return this.GetKeyUp ((KeyCode)System.Enum.Parse (typeof(KeyCode), name));
		}

		/// <summary>
		/// Returns true during the frame the user releases the key identified by key code.
		/// </summary>
		/// <returns><c>true</c>, if key up was gotten, <c>false</c> otherwise.</returns>
		/// <param name="key">Key.</param>
		public virtual bool GetKeyUp (KeyCode key)
		{
			return this._keyState.ContainsKey (key) ? (this._keyState [key] & InputState.IsUp) != 0 : false;
		}

		/// <summary>
		/// Returns true during the frame the user pressed down the virtual button identified by buttonName.
		/// </summary>
		/// <returns><c>true</c>, if button down was gotten, <c>false</c> otherwise.</returns>
		/// <param name="button">Button.</param>
		public virtual bool GetButtonDown (string button)
		{
			return this._buttonState.ContainsKey (button) ? (this._buttonState [button] & InputState.IsDown) != 0 : false;
		}

		/// <summary>
		/// Returns true while the virtual button identified by buttonName is held down.
		/// </summary>
		/// <returns><c>true</c>, if button was gotten, <c>false</c> otherwise.</returns>
		/// <param name="button">Button.</param>
		public virtual bool GetButton (string button)
		{
			return this._buttonState.ContainsKey (button) ? (this._buttonState [button] & InputState.IsPressed) != 0 : false;
		}

		/// <summary>
		/// Returns true the first frame the user releases the virtual button identified by buttonName.
		/// </summary>
		/// <returns><c>true</c>, if button up was gotten, <c>false</c> otherwise.</returns>
		/// <param name="button">Button.</param>
		public virtual bool GetButtonUp (string button)
		{
			return this._buttonState.ContainsKey (button) ? (this._buttonState [button] & InputState.IsUp) != 0 : false;
		}

		/// <summary>
		/// Returns true during the frame the user pressed down the virtual button identified by buttonName.
		/// </summary>
		/// <returns><c>true</c>, if button down was gotten, <c>false</c> otherwise.</returns>
		/// <param name="button">Button.</param>
		public virtual bool GetMouseButtonDown (int button)
		{
			return this._mouseButtonState.ContainsKey (button) ? (this._mouseButtonState [button] & InputState.IsDown) != 0 : false;
		}

		/// <summary>
		/// Returns true while the virtual button identified by buttonName is held down.
		/// </summary>
		/// <returns><c>true</c>, if button was gotten, <c>false</c> otherwise.</returns>
		/// <param name="button">Button.</param>
		public virtual bool GetMouseButton (int button)
		{
			return this._mouseButtonState.ContainsKey (button) ? (this._mouseButtonState [button] & InputState.IsPressed) != 0 : false;
		}

		/// <summary>
		/// Returns true the first frame the user releases the virtual button identified by buttonName.
		/// </summary>
		/// <returns><c>true</c>, if button up was gotten, <c>false</c> otherwise.</returns>
		/// <param name="button">Button.</param>
		public virtual bool GetMouseButtonUp (int button)
		{
			return this._mouseButtonState.ContainsKey (button) ? (this._mouseButtonState [button] & InputState.IsUp) != 0 : false;
		}

		/// <summary>
		/// Returns the value of the virtual axis identified by axisName.
		/// </summary>
		/// <returns>The axis.</returns>
		/// <param name="axis">Axis.</param>
		public virtual float GetAxis (string axis)
		{
			return this._axis.ContainsKey (axis) ? this._axis [axis] : 0f;
		}

		#endregion

		/// <summary>
		/// Coroutine to update the input states of virtual buttons and keys.
		/// </summary>
		/// <returns>The state.</returns>
		IEnumerator UpdateState ()
		{	
			while (true) {
				yield return new WaitForEndOfFrame ();
				if (this._keyState.Count > 0)
					this.UpdateInputState<KeyCode> (this._keyState);
				if (this._buttonState.Count > 0)
					this.UpdateInputState<string> (this._buttonState);
				if (this._mouseButtonState.Count > 0)
					this.UpdateInputState<int> (this._mouseButtonState);
			}
		}

		protected virtual void UpdateInputState<T> (Dictionary<T,InputState> inputs)
		{
			var keys = new List<T> (inputs.Keys);
			foreach (var key in keys) {
				InputState state = inputs [key];
				// remove up state from last frame
				if ((state & InputState.IsUp) != 0) {
					state &= ~InputState.IsUp;
				}
				// remove down state from last frame
				if ((state & InputState.IsDown) != 0) {
					state &= ~InputState.IsDown;
				}

				if ((state & InputState.BecomePressed) != 0) {
					state &= ~InputState.BecomePressed;
					state |= InputState.IsDown;
					state |= InputState.IsPressed;
				}

				if ((state & InputState.BecomeIdle) != 0) {
					state &= ~InputState.BecomeIdle;
					state |= InputState.IsUp;
					state &= ~InputState.IsPressed;
				}
				inputs [key] = state;
			}
		}

		#endregion

		#region Events

		protected virtual void Awake ()
		{
			if (this.PreventDestroyOnLoad) {
				MonoBehaviour.DontDestroyOnLoad (this);
			}
			this.compass = new Compass ();
			this.gyro = new Gyroscope ();
		}

		protected virtual void Start ()
		{
			if (this.InitializeOnLoad) {
				this.SetupServer ();
			}
		}

		protected virtual void OnClientConnect (NetworkMessage netMsg)
		{
			this._remoteConnection = netMsg.conn;
			this.isReady = true;
		}

		protected virtual void OnClientDisconnect (NetworkMessage netMsg)
		{
			this._remoteConnection = null;
		}

		protected virtual void OnServerError (NetworkMessage netMsg)
		{

		}

		protected virtual void OnReceiveButton (NetworkMessage netMsg)
		{
			ButtonMessage msg = netMsg.ReadMessage<ButtonMessage> ();
			if (!this._buttonState.ContainsKey (msg.ButtonName)) {
				this._buttonState [msg.ButtonName] = InputState.None;
			}

			if (msg.ButtonState) {
				this._buttonState [msg.ButtonName] |= InputState.BecomePressed;
			} else {
				this._buttonState [msg.ButtonName] |= InputState.BecomeIdle;
			}
		}

		protected virtual void OnReceiveMouseButton (NetworkMessage netMsg)
		{
			MouseButtonMessage msg = netMsg.ReadMessage<MouseButtonMessage> ();
			if (!this._mouseButtonState.ContainsKey (msg.MouseButton)) {
				this._mouseButtonState [msg.MouseButton] = InputState.None;
			}

			if (msg.ButtonState) {
				this._mouseButtonState [msg.MouseButton] |= InputState.BecomePressed;
			} else {
				this._mouseButtonState [msg.MouseButton] |= InputState.BecomeIdle;
			}
		}

		protected virtual void OnReceiveKey (NetworkMessage netMsg)
		{
			KeyMessage msg = netMsg.ReadMessage<KeyMessage> ();
			if (!this._keyState.ContainsKey (msg.Key)) {
				this._keyState [msg.Key] = InputState.None;
			}

			if (msg.KeyState) {
				this._keyState [msg.Key] |= InputState.BecomePressed;
			} else {
				this._keyState [msg.Key] |= InputState.BecomeIdle;
			}
		}

		protected virtual void OnReceiveAcceleration (NetworkMessage netMsg)
		{
			AccelerationMessage msg = netMsg.ReadMessage<AccelerationMessage> ();
			this.acceleration = msg.Acceleration;
		}

		protected virtual void OnReceiveCompass (NetworkMessage netMsg)
		{
			CompassMessage msg = netMsg.ReadMessage<CompassMessage> ();
			this.compass.trueHeading = msg.trueHeading;
			this.compass.timestamp = msg.timestamp;
			this.compass.headingAccuracy = msg.headingAccuracy;
			this.compass.magneticHeading = msg.magneticHeading;
			this.compass.rawVector = msg.rawVector;
		}

		protected virtual void OnReceiveGyroscope (NetworkMessage netMsg)
		{
			GyroMessage msg = netMsg.ReadMessage<GyroMessage> ();
			this.gyro.attitude = msg.attitude;
			this.gyro.enabled = msg.enabled;
			this.gyro.gravity = msg.gravity;
			this.gyro.rotationRate = msg.rotationRate;
			this.gyro.rotationRateUnbiased = msg.rotationRateUnbiased;
			this.gyro.updateInterval = msg.updateInterval;
			this.gyro.userAcceleration = msg.userAcceleration;
		}

		protected virtual void OnReceiveAxis (NetworkMessage netMsg)
		{
			AxisMessage msg = netMsg.ReadMessage<AxisMessage> ();
			this._axis [msg.AxisName] = msg.AxisValue;
		}

		protected virtual void OnEnable ()
		{
			this.StartCoroutine (this.UpdateState ());
		}

		protected virtual void OnDisable ()
		{
			this.StopAllCoroutines ();
		}

		#endregion
	}

}