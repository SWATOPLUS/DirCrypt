using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DirCrypt.Crypt
{
    public class MultBlockCryptoProvider : IBlockCryptoProvider
    {
        private readonly IBlockCryptoProvider _block;
        private readonly int _count;

        public MultBlockCryptoProvider(IBlockCryptoProvider block, int count)
        {
            _block = block;
            _count = count;
        }

        public int BlockSize => _block.BlockSize;
        public int KeySize => _block.KeySize;
        public byte[] Encrypt(IReadOnlyCollection<byte> block, IReadOnlyCollection<byte> key)
        {
            foreach (var x in Enumerable.Range(0, _count))
            {
                block = _block.Encrypt(block, key);
            }

            return block.ToArray();
        }

        public byte[] Decrypt(IReadOnlyCollection<byte> block, IReadOnlyCollection<byte> key)
        {
            foreach (var x in Enumerable.Range(0, _count))
            {
                block = _block.Decrypt(block, key);
            }

            return block.ToArray();
        }
    }
}
