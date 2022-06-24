# LSB_Encoder_Unity
A simple Least Significant Bit encoder/decoder for storing information into PNG's using Unity's Texture2D. 
This can be used for making save files that are screenshots of the players current location, making mods or levels that are shared with just the image or even for sharing character creator properties using images of the characters themselves.

## Overview
To get setup and using the LSBEncoder you only need to grab the [LSBEncoder.cs](https://raw.githubusercontent.com/JamesAaronJohnson/LSB_Encoder_Unity/master/Assets/Code/LSBEncoder.cs) file from here and put into your project.
To see an example of how it works, clone this repo and check out the example scene and test component to get a feel for it.
To get up and running with the encoder check out the basic examples below.

### Encoding An Image
Encoding an image is as simple as using the `LSBEncoder.Encode (ref Texture2D, byte[] data)` method. You provide a texture reference to encode into and the data to encode and it'll return a formatted PNG with the data inside of it ready to write to disk.

```cs
// Convert the message to bytes.
byte[] messageData = Encoding.UTF8.GetBytes (_Message);

// Check to see if we can fit our message into our image size without issue.
if (LSBEncoder.CanEncode (_Texture, messageData) == false)
{
    Debug.LogWarning ("Message cannot be encoded as it is too large for destination texture.", this);
    return;
}

// Encode our message into our texture and return a png formatted image to write to disk.
byte[] pngData = LSBEncoder.Encode (ref _Texture, messageData);
File.WriteAllBytes (_ImagePath, pngData);
```

### Decoding An Image
Decoding is also simple. Simply read the png file from the disk and provide the raw data and it'll return an array of bytes of the data it decoded. There's also a helpful overload for reading string data directly, all you need to do is provide the character encoding.

```cs
// Check to see if our image exists on disk.
if (File.Exists (_ImagePath) == false)
{
    Debug.LogWarning ("No encoded image is found. Please ensure an image is encoded first.", this);
    return;
}

// Read our encoded image from disk and load it as a texture.
byte[] pngData = File.ReadAllBytes (_ImagePath);
var texture = new Texture2D (0, 0);
texture.LoadImage (pngData);

// Decode our message from the image using the same text encoding we encoded with originally.
string decodedMessage = LSBEncoder.Decode (texture, Encoding.UTF8);
```

# Licence
LSB_Encoder_Unity is licensed under the Mozilla Public Licence, Version 2.0. See
[LICENSE](https://github.com/JamesAaronJohnson/LSB_Encoder_Unity/blob/master/LICENSE) for the full
license text.
