using System.IO;
using System.Text;
using UnityEngine;
using LSBEncoding;

namespace Assets.Code
{
    public class TestComponent : MonoBehaviour
    {
        [SerializeField, Tooltip ("The texture to encode with the given message.")]
        private Texture2D _Texture = null;
        [SerializeField, Tooltip ("The path of the final stored image. This is non-editable for the example and will reset every time once changed through the OnValidate method.")]
        private string _ImagePath = "";
        [SerializeField, Tooltip ("The message to encode into the texture.")]
        private string _Message = "Example test message written to an image using Least Significant Bit Insertion";

        private void OnValidate ()
        {
            _ImagePath = Application.persistentDataPath + "/encoded_image.png";

            if (_Texture == null)
                return;

            if (_Texture.isReadable == false)
                Debug.LogError ("Please ensure that the texture provided has the Read/Write box enabled.", this);
        }

        [ContextMenu("Encode Message")]
        private void EncodeTexture ()
        {
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
            Debug.Log ($"<color=#33FF11>Message Encoded to:</color>  {_ImagePath}");
        }

        [ContextMenu ("Decode Message")]
        private void DecodeTexture ()
        {
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
            Debug.Log ($"<color=#33FF11>Message Decoded:</color> {decodedMessage}");
        }
    }
}