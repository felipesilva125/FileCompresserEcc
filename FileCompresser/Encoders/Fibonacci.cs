using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Linq;
using System.IO;

namespace FileCompresser
{
    public class Fibonacci : Ecc, IEncoder
    {
        public void Encode(string content, string fileName)
        {
            string path = Path.Combine(FileController.FILE_PATH, fileName);
            path = Path.ChangeExtension(path, FileController.COMPRESSING_EXTENSION);

            byte[] bytes = Encoding.ASCII.GetBytes(content);
            int[] bytesAsInts = Array.ConvertAll(bytes, c => Convert.ToInt32(c));

            int[] fib = new int[20];
            fib[0] = 0;
            fib[1] = 1;

            for (int i = 2; i < fib.Length; i++)
                fib[i] = fib[i - 1] + fib[i - 2];

            int count = 0;
            int number;
            List<int> fibLocations = new List<int>();
            List<string> codewords = new List<string>();

            while (count < bytesAsInts.Length)
            {
                number = bytesAsInts[count];
                while (number > 0)
                {
                    int aux = 0;
                    while (fib[aux] <= number)
                    {
                        aux++;
                    }
                    number = number - fib[aux - 1];
                    fibLocations.Add(aux - 1);
                }
                fibLocations.Add(0);                                    // Separates the locations for each number
                count++;
            }

            int[] codesAux = new int[20];
            string codewordAux = "";
            int positionAux = fibLocations[0];
            int totalLength = 0;

            for (int i = 0; i < fibLocations.Count; i++)                // generate codewords
            {
                if (i > 0)
                {
                    if (fibLocations[i - 1] == 0)
                    {
                        positionAux = fibLocations[i];
                    }
                }
                if (fibLocations[i] != 0)
                {
                    codesAux[fibLocations[i]] = 1;
                }
                else
                {
                    codesAux[positionAux + 1] = 1;                      // stop bit
                    for (int j = 2; j <= positionAux + 1; j++)
                    {
                        codewordAux += codesAux[j];
                    }
                    Array.Clear(codesAux, 0, codesAux.Length);
                    codewords.Add(codewordAux);
                    totalLength += codewordAux.Length;
                    codewordAux = "";
                }
            }

            BitArray bits = new BitArray(totalLength);
            BitArray bits8 = new BitArray(8);

            count = 0;
            for (int i = 0; i < codewords.Count; i++)               // fill bits array with codewords
            {
                var res = new BitArray(codewords[i].Select(c => c == '1').ToArray());
                for (int j = 0; j < res.Count; j++)
                {
                    bits[count++] = res[j];
                }
            }

            //byte[] toByte = bitToByte(bits, bits.Length);
            int tam = (int)Math.Ceiling(bits.Count / 8d);           // bit to byte
            int resto = bits.Count % 8;
            byte[] bitToByte = new byte[tam];

            count = 0;
            int bitCount = 0;
            for (int i = 0; i <= bits.Count; i++)
            {
                if (i % 8 == 0 && i != 0)
                {
                    bits8.CopyTo(bitToByte, bitCount++);
                    count = 0;
                    if (i != bits.Count)
                        bits8[count++] = bits[i];
                }
                else
                {
                    if (i != bits.Count)
                        bits8[count++] = bits[i];
                }
            }
            count = 0;
            tam = bits.Count - resto; //-8
            if (resto != 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (resto > 0)
                    {
                        bits8[count++] = bits[tam++];
                        resto--;
                    }
                    else
                    {
                        bits8[count++] = false;
                    }
                }
                bits8.CopyTo(bitToByte, bitCount);
            }


            byte[] shiftRight = new byte[bitToByte.Length + 2];
            for (int i = 0; i < bitToByte.Length; i++)
            {
                shiftRight[(i + 2) % shiftRight.Length] = bitToByte[i];
            }

            shiftRight[0] = 2;                                  // Fibonacci number
            shiftRight[1] = 0;                                  // Only for Golomb K

            byte[] header = new byte[2] { shiftRight[0], shiftRight[1] };
            File.WriteAllBytes(path, shiftRight);               // generate .cod

            EncodeECC(bitToByte, header, path);                 // crc, hamming and .ecc
        }

        public void EncodeECC(byte[] bytes, byte[] header, string filePath)
        {
            filePath = Path.ChangeExtension(filePath, FileController.ECC_EXTENSION);

            Ecc a = new Ecc();
            byte crc8 = a.Crc(header);
            byte[] crcHeader = new byte[3] { header[0], header[1], crc8 };

            BitArray hamming = a.Hamming(bytes);                // hamming bits
            BitArray head = new BitArray(crcHeader);            // header bits
            BitArray bits8 = new BitArray(8);                   // aux to perform bit to byte

            int tam = (crcHeader.Length * 8) + hamming.Count;      // total number of bits
            BitArray eccResult = new BitArray(tam);

            int index = 0;
            for (int i = 0; i < head.Length; i++)               // add header to eccResult BitArray
            {
                eccResult[index++] = head[i];
            }
            for (int i = index, j = 0; i < tam; i++, j++)        // add hamming codewords to eccResult BitArray
            {
                eccResult[index++] = hamming[j];
            }

            //byte[] eccBytes = bitToByte(eccResult, tam);
            int numBytes = (int)Math.Ceiling(tam / 8d);         // begin perform bit to byte
            int resto = tam % 8;
            byte[] bitToByte = new byte[numBytes];

            int count = 0;
            int bitCount = 0;
            for (int i = 0; i <= eccResult.Length; i++)
            {
                if (i % 8 == 0 && i != 0)
                {
                    bits8.CopyTo(bitToByte, bitCount++);
                    count = 0;
                    if (i != eccResult.Length)
                        bits8[count++] = eccResult[i];
                }
                else
                {
                    if (i != eccResult.Length)
                        bits8[count++] = eccResult[i];
                }
            }
            count = 0;
            tam = eccResult.Count - resto;
            if (resto != 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (resto > 0)
                    {
                        bits8[count++] = eccResult[tam++];
                        resto--;
                    }
                    else
                    {
                        bits8[count++] = false;
                    }
                }
                bits8.CopyTo(bitToByte, bitCount);              // end
            }

            //File.WriteAllBytes(filePath, eccBytes);
            File.WriteAllBytes(filePath, bitToByte);
        }

        public void Decode(byte[] bytes, string fileName)
        {
            string path = Path.Combine(FileController.FILE_PATH, fileName);
            //path = Path.ChangeExtension(path, FileController.DECOMPRESSING_EXTENSION);

            DecodeECC(fileName, path, bytes);

            var fileContent = FileController.ReadFileContent(fileName, FileController.DECECC_EXTENSION); //COM

            byte[] bytesAux = new byte[bytes.Length - 2];       // remove heading
            Buffer.BlockCopy(fileContent, 2, bytesAux, 0, bytesAux.Length);

            path = Path.ChangeExtension(path, FileController.DECOMPRESSING_EXTENSION);

            BitArray bits = new BitArray(bytesAux);
            List<string> codewords = new List<string>();
            List<int> intCodes = new List<int>();
            List<char> charCodes = new List<char>();

            int[] fib = new int[20];
            fib[0] = 0;
            fib[1] = 1;
            for (int i = 2; i < fib.Length; i++)
                fib[i] = fib[i - 1] + fib[i - 2];

            string codesAux = "";
            bool a;
            bool b;
            for (int i = 0; i < bits.Count; i++)
            {
                if (i < bits.Count - 1)
                {
                    a = bits[i];
                    b = bits[i + 1];
                    if (a == true && b == true)
                    {
                        codesAux += "1";                        // codeword with no stop bit
                        codewords.Add(codesAux);
                        codesAux = "";
                        ++i;
                    }
                    else
                    {
                        if (bits[i] == false)
                            codesAux += "0";
                        else
                            codesAux += "1";
                    }
                }
                else
                {
                    if (bits[i] == false)
                        codesAux += "0";
                    else
                        codesAux += "1";
                }
            }

            int sum = 0;
            char numberAux;
            for (int i = 0; i < codewords.Count; i++)
            {
                for (int j = 0; j < codewords[i].Length; j++)
                {
                    numberAux = codewords[i][j];
                    if (numberAux == '1')
                    {
                        sum += fib[j + 2];
                    }
                }
                intCodes.Add(sum);
                sum = 0;
            }

            byte[] ret = new byte[intCodes.Count];
            for (int i = 0; i < intCodes.Count; i++)
            {
                ret[i] = Convert.ToByte(intCodes[i]);
            }

            File.WriteAllBytes(path, ret);
        }

        public void DecodeECC(string fileName, string filePath, byte[] bytes)
        {
            filePath = Path.ChangeExtension(filePath, FileController.DECECC_EXTENSION); // COM
            // first need to generate .cod from ecc

            Ecc a = new Ecc();
            byte[] header = new byte[2] { 2, 0 };
            byte crc8 = a.Crc(header);

            if (!crc8.Equals(bytes[2]))                        // check crc
            {
                Console.WriteLine("CRC ERROR: header is incorrect or corrupted! Ending file compressor...");
                return;
            }
            
            byte[] bytesAux = new byte[bytes.Length - 3];      // remove heading
            Buffer.BlockCopy(bytes, 3, bytesAux, 0, bytesAux.Length);

            BitArray hammingBits = new BitArray(bytesAux);
            BitArray hammingDec = a.HammingDec(hammingBits);
            BitArray bits8 = new BitArray(8);

            byte[] codedBytes = bitToByte(hammingDec, hammingDec.Length);
            
            byte[] shiftRight = new byte[codedBytes.Length + 3]; //codedBytes
            for (int i = 0; i < codedBytes.Length; i++)
            {
                shiftRight[(i + 3) % shiftRight.Length] = codedBytes[i];
            }

            shiftRight[0] = 2;                                  // Fibonacci number
            shiftRight[1] = 0;                                  // Only for Golomb K
            shiftRight[2] = crc8;
            File.WriteAllBytes(filePath, shiftRight);
            // }
            //verificar crc8 se esta correto

            //hammingdec;
            //gerar byte arrey e jogar em .cod
        }

        public byte[] bitToByte(BitArray bits, int tam)
        {
            BitArray bits8 = new BitArray(8);                       // aux to perform bit to byte

            int numBytes = (int)Math.Ceiling(tam / 8d);             // begin perform bit to byte
            int resto = tam % 8;
            byte[] bitToByte = new byte[numBytes];

            int count = 0;
            int bitCount = 0;
            for (int i = count; i <= bits.Length; i++)
            {
                if (i % 8 == 0 && i != 0)
                {
                    bits8.CopyTo(bitToByte, bitCount++);
                    count = 0;
                    bits8[count++] = bits[i];
                }
                else
                {
                    if (i != bits.Length)
                        bits8[count++] = bits[i];
                }
            }
            count = 0;
            tam = bits.Count - 8;
            if (resto != 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (resto > 0)
                    {
                        bits8[count++] = bits[tam++];
                        resto--;
                    }
                    else
                    {
                        bits8[count++] = false;
                    }
                }
                bits8.CopyTo(bitToByte, bitCount);              // end
            }

            return bitToByte;
        }
    }
}
