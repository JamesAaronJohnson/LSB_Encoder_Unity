This Unity project is for demonstration purposes.
Included is an example scene showing the usage of the LSBEncoder in a Unity project. 
Feel free to just take the LSBEncoder class as-is and use it for your own projects.

The example project contains the LSBEncoder class and a TestComponent which uses ContextMenu methods for demonstrating interaction. 
The example scene can be utilised in the following way:
> Load the Example scene.
> Look at the inspector for the Test Component
> Assign one of the given textures or one of your own to the component's "Texture" field.
> Write your own message into the "Message" field.
> Right click the Test Component and hit the "Encode Message" item.
> Right cick the Test Component and hit the "Decode Message" item.

And that's it! The image should be encoded with the message and decoded back successfully. If there are any errors along the way the console should help describe what went wrong.