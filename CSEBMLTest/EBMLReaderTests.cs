using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using CSEBML.DataSource;
using CSEBML.DocTypes.Matroska;
using CSEBML;
using CSEBML.DocTypes.EBML;
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
			var ebmlReader = new EBMLReader(ebmlSrc, new EBMLDocType(ebmlDoc));

			Recurse(ebmlReader, true);
		}

		private static void Recurse(EBMLReader reader, bool readValues) {
			ElementInfo elemInfo;
			while((elemInfo = reader.NextElementInfo())!=null) {
				if(elemInfo.DocElement.Type == EBMLElementType.Master) {
					reader.EnterMasterElement();
					Recurse(reader, readValues);
					reader.LeaveMasterElement();
				} else {
					var obj = reader.RetrieveValue();
				}
			}
		}
	}
}
