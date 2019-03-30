using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirCrypt.Crypt;

namespace DirCrypt
{
    internal class Program
    {
        private static IDataCryptoProvider Provider { get; } = BuildProvider();

        private static async Task Main(string[] args)
        {
            var (files, tasksCount, target, operation, key) = ParseArgs(args);

            if (files == null)
            {
                return;
            }

            Console.WriteLine($"{files.Length} files found");

            var queue = new ConcurrentQueue<string>(files);

            var sw = new Stopwatch();
            sw.Start();
            var tasks = Enumerable
                .Range(0, tasksCount)
                .Select(x => ProcessQueue(queue, operation, key, target));
            await Task.WhenAll(tasks);
            sw.Stop();
            
            Console.WriteLine($"Done in {sw.ElapsedMilliseconds:F2} ms");
        }

        private static async Task ProcessQueue(ConcurrentQueue<string> queue, Operation op, byte[] key, string target)
        {
            while (queue.TryDequeue(out var path))
            {
                var bytes = await File.ReadAllBytesAsync(path);
                var name = Path.GetFileName(path);

                byte[] result;
                string resultName;

                if (op == Operation.Encrypt)
                {
                    result = Provider.Encrypt(bytes, key);
                    resultName = name + ".aes";
                }
                else
                {
                    result = Provider.Decrypt(bytes, key);
                    resultName = Path.ChangeExtension(name, "");
                }

                var resultPath = Path.Combine(target, resultName);
                await File.WriteAllBytesAsync(resultPath, result);
            }
        }

        private static (string[] files, int tasksCount, string target, Operation operation, byte[] key) ParseArgs(string[] args)
        {
            if (args.Length != 5)
            {
                Console.WriteLine("Usage: [-e/-d] [TasksCount] [SourceDirectory] [TargetDirectory] [key]");

                return (null, 0, null, Operation.Decrypt, null);
            }

            Operation op;

            switch (args[0])
            {
                case "-d":
                    op = Operation.Decrypt;
                    break;
                case "-e":
                    op = Operation.Encrypt;
                    break;
                default:
                    return (null, 0, null, Operation.Decrypt, null);
            }

            if (!int.TryParse(args[1], out var tasksCount))
            {
                Console.WriteLine($"Incorrect tasks count: {args[1]}");
                return (null, 0, null, Operation.Decrypt, null);
            }

            if (tasksCount < 1)
            {
                Console.WriteLine("TasksCount should be > 0");
                return (null, 0, null, Operation.Decrypt, null);
            }

            string[] files;
            var target = args[3];

            try
            {
                files = Directory.GetFiles(args[2]);

                if (!Directory.Exists(target))
                {
                    Directory.CreateDirectory(target);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to access to directory: {ex.Message}");
                
                return (null, 0, null, op, null);
            }

            var key = Encoding.UTF8.GetBytes(args[4]);

            if (key.Length > 32)
            {
                key = key.Take(32).ToArray();
            }
            else if(key.Length < 32)
            {
                var dummySize = 32 - key.Length;
                var dummyBytes = Enumerable.Range(0, dummySize).Select(x => new byte());

                key = key.Concat(dummyBytes).ToArray();
            }
            
            return  (files, tasksCount, target, op, key);
        }


        private static IDataCryptoProvider BuildProvider()
        {
            return new OfbCryptoProvider(new MultBlockCryptoProvider(new AesCryptoProvider(), 10000));
        }

        private enum Operation
        {
            Encrypt,
            Decrypt
        }
    }
}
