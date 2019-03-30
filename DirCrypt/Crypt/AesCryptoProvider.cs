using System;
using System.Collections.Generic;
using System.Linq;

namespace DirCrypt.Crypt
{
    public class AesCryptoProvider : IBlockCryptoProvider
    {
        public int BlockSize => Aes.BlockSize;
        public int KeySize => Aes.KeySize;

        public byte[] Encrypt(IReadOnlyCollection<byte> block, IReadOnlyCollection<byte> key)
        {
            Validate(block, key);

            var aes = new Aes(key.ToArray());
            var result = new byte[BlockSize];
            aes.EncryptBlock(block.ToArray(), 0, BlockSize, result, 0);

            return result;
        }

        public byte[] Decrypt(IReadOnlyCollection<byte> block, IReadOnlyCollection<byte> key)
        {
            Validate(block, key);

            var aes = new Aes(key.ToArray());
            var result = new byte[BlockSize];
            aes.DecryptBlock(block.ToArray(), 0, BlockSize, result, 0);

            return result;
        }

        private void Validate(IReadOnlyCollection<byte> block, IReadOnlyCollection<byte> key)
        {
            if (block.Count != BlockSize)
            {
                throw new ArgumentException($"{nameof(block)} size should be {BlockSize} bytes", nameof(block));
            }

            if (key.Count != KeySize)
            {
                throw new ArgumentException($"{nameof(key)} size should be {KeySize} bytes", nameof(key));
            }
        }
    }
}