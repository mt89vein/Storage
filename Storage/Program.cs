using Storage.Core;
using Storage.Core.Models;
using System;
using System.IO;

namespace Storage
{
	class Program
	{
		private static DataPageManager _dataPageManager;

		static void Main()
		{
            var dpmConfig = new DataPageManagerConfig("DataPageManager-1", 15 * 1024 * 1024, Path.Combine(Directory.GetCurrentDirectory()));
			_dataPageManager = new DataPageManager(dpmConfig);

			_dataPageManager.Save(new DataRecord(1, BitConverter.GetBytes(421)));
			_dataPageManager.Save(new DataRecord(4, BitConverter.GetBytes(3333)));

			var dataRecord1 = _dataPageManager.Read(1);
			var dataRecord2 = _dataPageManager.Read(2);
			var dataRecord3 = _dataPageManager.Read(3);
			var dataRecord4 = _dataPageManager.Read(4);

			if (dataRecord1?.Body != null)
			{
				var d1 = BitConverter.ToInt32(dataRecord1.Body.ToByteArray());
			}

			if (dataRecord2?.Body != null)
			{
				var d2 = BitConverter.ToInt32(dataRecord2.Body.ToByteArray());
			}


			if (dataRecord3?.Body != null)
			{
				var d3 = BitConverter.ToInt32(dataRecord3.Body.ToByteArray());
			}

			if (dataRecord4?.Body != null)
			{
				var d4 = BitConverter.ToInt32(dataRecord4.Body.ToByteArray());
			}
			Console.ReadLine();
		}
	}
}
