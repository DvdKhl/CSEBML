//Mod. BSD License (See LICENSE file) DvdKhl (DvdKhl@web.de)
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using CSEBML.DataSource;
using CSEBML.DocTypes.Matroska;
using CSEBML;
using CSEBML.DocTypes;
using System.Text;

namespace CSEBMLTest {
	[TestClass]
	public class EBMLReaderTests {
		[TestMethod]
		public void ParseMatroskaTestSuite() {
			var matroskaTestSuitePath = Path.Combine("TestFiles", "MatroskaTestSuite");

			if(!Directory.Exists(matroskaTestSuitePath)) {
				Assert.Inconclusive("Matroska Test Suite not found ({0})", matroskaTestSuitePath);
			}

			bool allPassed = true;
			StringBuilder result = new StringBuilder();
			foreach(var filePath in Directory.EnumerateFiles(matroskaTestSuitePath, "*.mkv")) {
				try {
					ParseMatroskaTestSuite_ParseFile(filePath);
					result.AppendLine( filePath + ": Passed");
				} catch(Exception) {
					result.AppendLine(filePath + ": Failed");
					allPassed = false;
				}
			}

			Assert.IsTrue(allPassed, result.ToString());
		}
		public void ParseMatroskaTestSuite_ParseFile(string filePath) {
			Stream src;
			try {
				src = File.OpenRead(filePath);
			} catch(Exception) {
				Assert.Inconclusive("Couldn't open Matroska Test File ({0})", filePath);
				return;
			}

			var ebmlSrc = new EBMLStreamDataSource(src);
			var ebmlDoc = new MatroskaDocType(CSEBML.DocTypes.Matroska.MatroskaVersion.V3);
			var ebmlReader = new EBMLReader(ebmlSrc, new EBMLDocType());

			Recurse(ebmlReader, true);
		}


		[TestMethod]
		public void GetBaseStream() {
			var dataSrc = new EBMLBlockDataSource(new byte[][] { new byte[0] }, 0);
			var docType = new EBMLDocType();
			var reader = new EBMLReader(dataSrc, docType);

			Assert.AreEqual(dataSrc, reader.BaseStream);
		}



		private static void Recurse(EBMLReader reader, bool readValues) {
			ElementInfo elemInfo;
			while((elemInfo = reader.Next())!=null) {
				if(elemInfo.DocElement.Type == EBMLElementType.Master) {
					using(reader.EnterElement(elemInfo)) {
						Recurse(reader, readValues);
					}
				} else {
					var obj = reader.RetrieveValue(elemInfo);
				}
			}
		}
	}
}
