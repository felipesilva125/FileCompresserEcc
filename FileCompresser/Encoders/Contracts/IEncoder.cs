using System;
using System.Collections.Generic;
using System.Text;

namespace FileCompresser
{
    public interface IEncoder
    {
        void Encode(string content, string fileName);
        // void EncodeECC();
        void Decode (byte[] bytes, string fileName);
        // void DecodeECC();
    }
}
