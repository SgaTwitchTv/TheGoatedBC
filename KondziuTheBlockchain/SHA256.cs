using System;
using System.Collections.Generic;
using System.Text;

namespace KondziuTheBlockchain
{
    public class Sha256BitLevel
    {
        private static uint RotateRight(uint x, int n) => (x >> n) | (x << (32 - n));
        private static uint Ch(uint x, uint y, uint z) => (x & y) ^ (~x & z);
        private static uint Maj(uint x, uint y, uint z) => (x & y) ^ (x & z) ^ (y & z);
        private static uint SIGMA0(uint x) => RotateRight(x, 2) ^ RotateRight(x, 13) ^ RotateRight(x, 22);
        private static uint SIGMA1(uint x) => RotateRight(x, 6) ^ RotateRight(x, 11) ^ RotateRight(x, 25);
        private static uint sigma0(uint x) => RotateRight(x, 7) ^ RotateRight(x, 18) ^ (x >> 3);
        private static uint sigma1(uint x) => RotateRight(x, 17) ^ RotateRight(x, 19) ^ (x >> 10);

        private static readonly uint[] K = {
            0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
            0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
            0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
            0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
            0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
            0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
            0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
            0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
        };

        public static string ComputeHash(string input)
        {
            byte[] data = Encoding.UTF8.GetBytes(input);
            ulong bitLen = (ulong)data.Length * 8;

            // Padding - preporcessing
            var padded = new List<byte>(data);
            padded.Add(0x80);
            while ((padded.Count * 8) % 512 != 448)
            {
                padded.Add(0x00);
            }

            for (int i = 7; i >= 0; i--)
            {
                padded.Add((byte)(bitLen >> (i * 8)));
            }

            // Initial hash values
            uint[] H = {
                0x6a09e667, 0xbb67ae85, 0x3c6ef372, 0xa54ff53a,
                0x510e527f, 0x9b05688c, 0x1f83d9ab, 0x5be0cd19
            };

            // A message to 512bit chunks (the jondro of SHA)
            for (int i = 0; i < padded.Count; i += 64)
            {
                uint[] W = new uint[64];
                for (int j = 0; j < 16; j++)
                {
                    int offset = i + j * 4;
                    W[j] = (uint)(padded[offset] << 24 |
                                  padded[offset + 1] << 16 |
                                  padded[offset + 2] << 8 |
                                  padded[offset + 3]);
                }

                for (int j = 16; j < 64; j++)
                {
                    W[j] = sigma1(W[j - 2]) + W[j - 7] + sigma0(W[j - 15]) + W[j - 16];
                }

                uint a = H[0], b = H[1], c = H[2], d = H[3];
                uint e = H[4], f = H[5], g = H[6], h = H[7];

                for (int j = 0; j < 64; j++)
                {
                    uint T1 = h + SIGMA1(e) + Ch(e, f, g) + K[j] + W[j];
                    uint T2 = SIGMA0(a) + Maj(a, b, c);
                    h = g; g = f; f = e; e = d + T1;
                    d = c; c = b; b = a; a = T1 + T2;
                }

                H[0] += a; H[1] += b; H[2] += c; H[3] += d;
                H[4] += e; H[5] += f; H[6] += g; H[7] += h;
            }

            return string.Concat(Array.ConvertAll(H, h => h.ToString("x8")));
        }
    }
}