using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using MoreLinq.Extensions;

namespace DirCrypt.Crypt
{
    public class OfbCryptoProvider : IDataCryptoProvider
    {
        private readonly IBlockCryptoProvider _block;
        private readonly RNGCryptoServiceProvider _rng;

        public OfbCryptoProvider
        (
            IBlockCryptoProvider block
        )
        {
            _block = block;
            _rng = new RNGCryptoServiceProvider();
        }

        public byte[] Encrypt(IReadOnlyCollection<byte> sourceData, IEnumerable<byte> key)
        {
            var blockKey = BuildBlockKey(key);
            var blocks = BuildBlocks(sourceData);
            var iv = BuildIv();
            var cipherData = new List<byte>(iv);


            foreach (var block in blocks)
            {
                var encryptedIv = _block.Encrypt(iv, blockKey);

                var cipherBlock = XorArrays(encryptedIv, block);

                cipherData.AddRange(cipherBlock);

                iv = encryptedIv;
            }

            return cipherData.ToArray();
        }

        public byte[] Decrypt(IReadOnlyCollection<byte> encryptedData, IEnumerable<byte> key)
        {
            var blockKey = BuildBlockKey(key);
            var blocks = encryptedData.Skip(_block.BlockSize).Batch(_block.BlockSize).Select(x => x.ToArray());
            var iv = encryptedData.Take(_block.BlockSize).ToArray();

            var sourceData = new List<byte>();

            foreach (var block in blocks)
            {
                var encryptedIv = _block.Encrypt(iv, blockKey);

                var sourceBlock = XorArrays(encryptedIv, block);

                sourceData.AddRange(sourceBlock);

                iv = encryptedIv;
            }

            var size = BitConverter.ToInt32(sourceData.Take(sizeof(int)).ToArray(), 0);

            return sourceData.Skip(sizeof(int)).Take(size).ToArray();
        }

        private byte[] BuildBlockKey(IEnumerable<byte> key)
        {
            return key.Take(_block.KeySize).ToArray();
        }

        private IEnumerable<byte[]> BuildBlocks(IReadOnlyCollection<byte> data)
        {
            var sizePart = BitConverter.GetBytes(data.Count);
            var dummyBytesCount = _block.BlockSize - (sizePart.Length + data.Count) % _block.BlockSize;
            var dummyBytes = new byte[dummyBytesCount];
            _rng.GetBytes(dummyBytes);

            return sizePart.Concat(data).Concat(dummyBytes)
                .Batch(_block.BlockSize)
                .Select(x => x.ToArray());
        }

        private byte[] BuildIv()
        {
            var iv = new byte[_block.BlockSize];
            _rng.GetBytes(iv);

            return iv;
        }

        private static byte[] XorArrays(IReadOnlyList<byte> first, IReadOnlyList<byte> second)
        {
            if (first.Count != second.Count)
            {
                throw new InvalidOperationException("Array length should be equal");
            }

            var result = new byte[first.Count];

            for (var i = 0; i < first.Count; i++)
            {
                result[i] = (byte)(first[i] ^ second[i]);
            }

            return result;
        }
    }
}
