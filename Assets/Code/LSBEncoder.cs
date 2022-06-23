using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Unity.Collections;

namespace LSBEncoding
{
    /// <summary>Utility class for encoding data into Texture2D png's using Least Significant Bit Insertion.</summary>
    public static class LSBEncoder
    {
        /// <summary>Checks to see if the given text can be encoded into the image.</summary>
        /// <param name="image">The image to test.</param>
        /// <param name="text">The text data to test.</param>
        /// <param name="encoding">The character encoding of the text.</param>
        /// <returns>True if image can be encoded, False if not.</returns>
        public static bool CanEncode (Texture2D image, string text, Encoding encoding)
        {
            byte[] data = encoding.GetBytes (text);
            return CanEncode (image, data);
        }

        /// <summary>Checks to see if the given data can be encoded into the image.</summary>
        /// <param name="image">The image to test.</param>
        /// <param name="data">The text data to test.</param>
        /// <returns>True if image can be encoded, False if not.</returns>
        public static bool CanEncode (Texture2D image, byte[] data)
        {
            return DataStoreSize (image) > data.Length;
        }

        /// <summary>Get's the number of bytes that can be encoded into the image.</summary>
        /// <param name="image">The image to check.</param>
        /// <returns>The number of bytes that can be encoded into the image.</returns>
        public static int DataStoreSize (Texture2D image)
        {
            return image.width * image.height / 3;
        }

        /// <summary>Get's the number of bytes left that can be encoded into the image.</summary>
        /// <param name="image">The image to check.</param>
        /// <param name="data">The data to compare.</param>
        /// <returns>The number of bytes left that can be encoded into the image.</returns>
        public static int BytesLeft (Texture2D image, byte[] data)
        {
            return DataStoreSize (image) - data.Length;
        }

        /// <summary>Encodes the given text into the image with Least Significant Bit insertion using the RGB color channels.</summary>
        /// <param name="image">The image to encode into.</param>
        /// <param name="text">The text to encode.</param>
        /// <param name="encoding">The character encoding to write as.</param>
        /// <returns>The encoded image data as a PNG format.</returns>
        public static byte[] Encode (ref Texture2D image, string text, Encoding encoding)
        {
            byte[] data = encoding.GetBytes (text);
            return Encode (ref image, data);
        }

        /// <summary>Encodes the given data into the image with Least Significant Bit insertion using the RGB color channels.</summary>
        /// <param name="image">The image to encode into.</param>
        /// <param name="data">The data to encode.</param>
        /// <returns>The encoded image data as a PNG format.</returns>
        public static byte[] Encode (ref Texture2D image, byte[] data)
        {
            byte bitCount = 0;
            int byteCount = 0;
            const int byteLength = 8;
            const int channelCount = 3;
            byte currentByte = data[byteCount];
            NativeArray<Color32> colorData = image.GetRawTextureData<Color32> ();

            // For each byte of data loop through every color channel and encode it bit by bit until done.
            for (int i = 0; i < data.Length * channelCount; i++)
            {
                Color32 color = colorData[i];
                for (int c = 0; c < channelCount; c++)
                {
                    // Only set the current color IF 
                    if (bitCount < byteLength)
                    {
                        // If the current bit is 1 or 0. Set the current color's LSB to the corresponding value.
                        if (currentByte.IsBitSet (bitCount))
                            color[c] = color[c].SetLSB (true);
                        else
                            color[c] = color[c].SetLSB (false);
                    }

                    // If we've reached the end of the current byte.
                    if (bitCount >= byteLength)
                    {
                        bitCount = 0;

                        // If there is room for another byte of data to encode.
                        if (byteCount + 1 < data.Length)
                        {
                            // Grab the next byte.
                            byteCount++;
                            currentByte = data[byteCount];

                            // Set this 9th color to 0 so that we know the message continues.
                            color[c] = color[c].SetLSB (false);
                        }
                        // If there is no room for another byte of data to encode.
                        else
                        {
                            // Reset the current byte
                            byteCount = 0;
                            currentByte = data[byteCount];

                            // Set this 9th pixel to 1 so we know that the message is finished.
                            color[c] = color[c].SetLSB (true);

                            // Apply the color changes and exit the loop as we've encoded the message.
                            image.Apply ();
                            return image.EncodeToPNG ();
                        }

                        continue;
                    }

                    bitCount++;
                }

                // Re-assign the encoded color data back to the image.
                colorData[i] = color;
            }

            // Re-assign the changed color values back to the image and apply them.
            image.Apply ();
            return image.EncodeToPNG ();
        }

        /// <summary>Decode any hidden data inside of the image using Least Significant Bit detection.</summary>
        /// <param name="image">The image to decode data from.</param>
        /// <param name="encoding">The character encoding to read as.</param>
        /// <returns>The decoded string data from the image.</returns>
        public static string Decode (Texture2D image, Encoding encoding)
        {
            byte[] data = Decode (image);
            return encoding.GetString (data);
        }

        /// <summary>Decode any hidden data inside of the image using Least Significant Bit detection.</summary>
        /// <param name="image">The image to decode data from.</param>
        /// <returns>The decoded data from the image.</returns>
        public static byte[] Decode (Texture2D image)
        {
            var colorData = image.GetPixels32 ();
            var dataBytes = new List<byte> (colorData.Length);

            byte bitCount = 0;
            int byteCount = 0;
            byte currentByte = 0;
            const int byteLength = 8;
            const int channelCount = 3;

            // For each byte of color data loop through every channel extracting every LSB until we encounter an end point.
            for (int i = 0; i < colorData.Length; i++)
            {
                var color = colorData[i];

                for (int c = 0; c < channelCount; c++)
                {
                    if (bitCount < byteLength)
                    {
                        // Set the current byte's bit to that of the current color's LSB.
                        currentByte = currentByte.SetBit (bitCount, color[c].IsLSBSet ());
                    }

                    if (bitCount >= byteLength)
                    {
                        bitCount = 0;
                        byteCount++;
                        dataBytes.Add (currentByte);

                        // Check to see if the 9th color is set.
                        // If it is, then we know the message has ended as we intentionally
                        // Set every 9th bit as 0.
                        if (color[c].IsLSBSet ())
                            return dataBytes.ToArray ();

                        continue;
                    }

                    bitCount++;
                }
            }

            return dataBytes.ToArray ();
        }

        #region Helper Extensions
        private static byte SetLSB (this byte b, bool value)
        {
            return SetBit (b, 0, value);
        }

        private static byte SetBit (this byte b, byte pos, bool value)
        {
            if (value)
                b |= (byte)( 1 << pos );
            else
                b &= (byte)~( 1 << pos );

            return b;
        }

        private static bool IsLSBSet (this byte b)
        {
            return IsBitSet (b, 0);
        }

        private static bool IsBitSet (this byte b, byte pos)
        {
            b >>= pos;
            return ( b & 1 ) > 0;
        }
        #endregion
    }
}
