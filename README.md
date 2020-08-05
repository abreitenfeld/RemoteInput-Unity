# Remote Input for Unity3D

Now you are ready to develop VR games without any extra input hardware. The Remote Input kit enables you to control your mobile device via a second device by transferring input signals over UNET. Connect your desktop hardware to your smart phone, and vice versa to use your smart phone as motion controller for desktop VR.
This package is capable to transfer mouse and virtual buttons, keys, axis, acceleration and the compass to create a complex input scheme on a remote input receiver.

## Requirements

This package requires *HLAPI Multiplayer* package (deprecated) from Unity.

## Getting Started

The package provides two samples scenes to lean how to use the remote input scripts. The sample covers an scenario where a mobile device sends motion data using the compass and accelerometer to an receiving device. Just follow these steps:

1. In the Build Settings add the scene `RemoteInput/Samples/Scenes/RemoteSender` to **Scenes in Build**
2. Build and run the project on a mobile device
3. In the editor open the scene `RemoteInput/Samples/Scenes/RemoteReceiver`
4. Click **Run** in the editor (server will started automatically)
5. On the mobile device enter the IP of the receiving device and click Connect (status bar should signalize „Connected: True“ now)
6. Tilt the mobile device and see the object moving in the editor
