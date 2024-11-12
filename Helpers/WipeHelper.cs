using System.Security.Cryptography;
using Spectre.Console;

namespace JustWipeIt.Helpers
{
    public abstract class WipeHelper
    {
        private const int BufferSize = 4096; // Buffer size for file write operations

        public static void OverwriteWithPattern(string driveLetter, long size, ProgressTask task, byte pattern)
        {
            var buffer = new byte[BufferSize];
            Array.Fill(buffer, pattern);

            using var fs = new FileStream($"{driveLetter}scrubbed.bin", FileMode.OpenOrCreate);
            while (fs.Position < size)
            {
                fs.Write(buffer, 0, buffer.Length);
                task.Increment(BufferSize);
            }
        }

        public static void OverwriteWithRandomData(string driveLetter, long size, ProgressTask task)
        {
            var random = new Random();
            var buffer = new byte[BufferSize];

            using var fs = new FileStream($"{driveLetter}scrubbed.bin", FileMode.OpenOrCreate);
            while (fs.Position < size)
            {
                random.NextBytes(buffer);
                fs.Write(buffer, 0, buffer.Length);
                task.Increment(BufferSize);
            }
        }

        public static void OverwriteWithSha256(string driveLetter, long size, ProgressTask task)
        {
            var buffer = new byte[BufferSize];
            using var fs = new FileStream($"{driveLetter}scrubbed.bin", FileMode.OpenOrCreate);
            while (fs.Position < size)
            {
                var hash = SHA256.HashData(buffer);
                fs.Write(hash, 0, hash.Length);
                task.Increment(hash.Length);
            }
        }

        public static void OverwriteWithAes(string driveLetter, long size, ProgressTask task)
        {
            using var aes = Aes.Create();
            aes.Key = new byte[32];
            aes.IV = new byte[16];

            var buffer = new byte[BufferSize];
            long totalBytesWritten = 0;

            using var cryptoStream = new CryptoStream(
                new FileStream($"{driveLetter}scrubbed.bin", FileMode.OpenOrCreate),
                aes.CreateEncryptor(),
                CryptoStreamMode.Write);

            while (totalBytesWritten < size)
            {
                cryptoStream.Write(buffer, 0, buffer.Length);
                totalBytesWritten += buffer.Length;
                task.Increment(buffer.Length);
            }
        }
    }
}