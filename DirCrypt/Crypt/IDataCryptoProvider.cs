using System.Collections.Generic;

namespace DirCrypt.Crypt
{
    public interface IDataCryptoProvider
    {
        byte[] Encrypt(IReadOnlyCollection<byte> sourceData, IEnumerable<byte> key);

        byte[] Decrypt(IReadOnlyCollection<byte> encryptedData, IEnumerable<byte> key);
    }
}