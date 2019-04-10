using Storage.Core;
using Storage.Core.Configuration;
using Storage.Core.Models;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Storage
{
    class Program
	{
		private static DataPageManager _dataPageManager;

		static void Main()
		{
            var dpmConfig = new DataPageManagerConfig("DataPageManager-1", 100000, Path.Combine(Directory.GetCurrentDirectory()));
			_dataPageManager = new DataPageManager(dpmConfig);

            _dataPageManager.Save(new DataRecord(1, Encoding.UTF8.GetBytes(new string('1', 1250))));
            _dataPageManager.Save(new DataRecord(2, Encoding.UTF8.GetBytes(new string('2', 12000))));
            _dataPageManager.Save(new DataRecord(3, Encoding.UTF8.GetBytes(new string('3', 412))));
            _dataPageManager.Save(new DataRecord(4, Encoding.UTF8.GetBytes(new string('4', 421124))));

            Thread.Sleep(1000); // даём время на срабатывание автосохранения.
            var dataRecord1 = _dataPageManager.Read(1);
			var dataRecord2 = _dataPageManager.Read(2);
            var dataRecord3 = _dataPageManager.Read(3);
			var dataRecord4 = _dataPageManager.Read(4);

			if (dataRecord1?.Body != null)
			{
				var d1 = Encoding.UTF8.GetString(dataRecord1.Body);
			}

			if (dataRecord2?.Body != null)
			{
				var d2 = Encoding.UTF8.GetString(dataRecord2.Body);
			}


            if (dataRecord3?.Body != null)
            {
                var d3 = Encoding.UTF8.GetString(dataRecord3.Body);
            }

            if (dataRecord4?.Body != null)
			{
				var d4 = Encoding.UTF8.GetString(dataRecord4.Body);
			}
			Console.ReadLine();
		}
	}
}
