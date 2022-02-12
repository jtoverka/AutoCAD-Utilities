using ACADE_Utilities;
using Autodesk.AutoCAD.Geometry;
using NUnit.Framework;
using System.Collections.Generic;

namespace ACADE_Tests
{
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void CheckWireNumberTest()
		{
			Dictionary<string, string> wdm = new();
			Dictionary<string, string> wd_mlr = new();
			Point3d point;
			AeLadder ladder = new();
			
			Assert.Pass();
		}
	}
}