using System.Text;
using System.IO;

namespace FileCompresser
{
    public class Delta : Ecc, IEncoder
    {
        public void Encode(string content, string fileName)
        {
            string path = Path.Combine(FileController.FILE_PATH, fileName);
            path = Path.ChangeExtension(path, FileController.COMPRESSING_EXTENSION);

            byte[] bytes = Encoding.ASCII.GetBytes(content);

            byte last = 0;
            byte original;
            int i;
            for (i = 0; i < bytes.Length; i++)
            {
                original = bytes[i];
                bytes[i] -= last;
                last = original;
            }

            byte[] shiftRight = new byte[bytes.Length + 2];
            for (i = 0; i < bytes.Length; i++)
            {
                shiftRight[(i + 2) % shiftRight.Length] = bytes[i];
            }
            
            shiftRight[0] = 4;                  // Delta number
            shiftRight[1] = 0;                  // Only for Golomb K

            byte[] header = new byte[2] { shiftRight[0], shiftRight[1] };

            File.WriteAllBytes(path, shiftRight);           // .cod

            EncodeECC(bytes, header, path);
        }

        public void Decode(byte[] bytes, string fileName)
        {
            string path = Path.Combine(FileController.FILE_PATH, fileName);

            DecodeECC(path, bytes);

            path = Path.ChangeExtension(path, FileController.DECOMPRESSING_EXTENSION);

            var fileContent = FileController.ReadFileContent(fileName, FileController.COMPRESSING_EXTENSION);
            
            byte[] arqBytes = fileContent;
            byte[] decoded = new byte[arqBytes.Length - 3];    // heading is not needed

            byte last = 0;
            int count = 0;
            for (int i = 3; i < arqBytes.Length; i++)          // skip the first 3 elements (heading)
            {
                arqBytes[i] += last;
                last = arqBytes[i];
                decoded[count++] = last;
            }

            File.WriteAllBytes(path, decoded);
        }
    }
}
