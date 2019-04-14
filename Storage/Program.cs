using Storage.Core;
using Storage.Core.Configuration;
using Storage.Core.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Storage
{
    public class Program
    {
        private static void Main()
        {
            var dpmConfig =
                new DataPageManagerConfig(
                    "DataPageManager-1", 
                    100, 
                    Path.Combine(Directory.GetCurrentDirectory())
                );

            using (var dataPageManager = new DataPageManager(dpmConfig))
            {
                var dog = new Dog
                {
                    Age = 5,
                    Id = 10420,
                    Name = "A dog"
                };
                var anotherDog = new Dog
                {
                    Age = 4,
                    Id = 10421,
                    Name = "Another dog"
                };
                var anotherOneDog = new Dog
                {
                    Age = 1,
                    Id = 10422,
                    Name = "Another one dog 3"
                };

                dataPageManager.Save(new DataRecord(1, dog.GetBytes()));
                dataPageManager.Save(new DataRecord(2, anotherDog.GetBytes()));
                dataPageManager.Save(new DataRecord(3, anotherOneDog.GetBytes()));

                Thread.Sleep(1000); // даём время на срабатывание автосохранения.

                var dataRecord1 = dataPageManager.Read(1);
                var dataRecord2 = dataPageManager.Read(2);
                var dataRecord3 = dataPageManager.Read(3);

                if (dataRecord1?.Body != null)
                {
                    Debug.Assert(dog.GetBytes().SequenceEqual(dataRecord1.Body));
                    var animal = Dog.ReadFrom(dataRecord1.Body);
                }

                if (dataRecord2?.Body != null)
                {
                    Debug.Assert(anotherDog.GetBytes().SequenceEqual(dataRecord2.Body));
                    var animal = Dog.ReadFrom(dataRecord2.Body);
                }

                if (dataRecord3?.Body != null)
                {
                    Debug.Assert(anotherOneDog.GetBytes().SequenceEqual(dataRecord3.Body));
                    var animal = Dog.ReadFrom(dataRecord3.Body);
                }

                foreach (var record in dataPageManager.AsEnumerable(1))
                {
                    var animal = Dog.ReadFrom(record.Body);
                }
            }

            Console.ReadLine();
        }

        /// <summary>
        /// Тестовый класс для проверки.
        /// </summary>
        [Serializable]
        private class Dog
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public int Id { get; set; }

            public byte[] GetBytes()
            {
                using (var memoryStream = new MemoryStream())
                {
                    var binFormat = new BinaryFormatter();

                    binFormat.Serialize(memoryStream, this);

                    return memoryStream.ToArray();
                }
            }

            public static Dog ReadFrom(byte[] bytes)
            {
                using (var memoryStream = new MemoryStream(bytes))
                {
                    memoryStream.Position = 0;
                    var binFormat = new BinaryFormatter();

                    return (Dog)binFormat.Deserialize(memoryStream);
                }
            }
        }
    }
}