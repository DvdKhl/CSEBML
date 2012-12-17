//Mod. BSD License (See LICENSE file) DvdKhl (DvdKhl@web.de)

using CSEBML;
using CSEBML.DataSource;
using CSEBML.DocTypes;
using CSEBML.DocTypes.Matroska;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
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
					result.AppendLine(filePath + ": Passed");
				} catch(Exception ex) {
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
				//src = File.OpenRead(@"C:\Users\Arokh\Projects\Visual Studio 2012\Projects\CSEBML\CSEBMLTest\bin\Release\TestFiles\MatroskaTestSuite\test7.mkv");
			} catch(Exception) {
				Assert.Inconclusive("Couldn't open Matroska Test File ({0})", filePath);
				return;
			}

			var ebmlSrc = new EBMLStreamDataSource(src);
			var matroskaDoc = new MatroskaDocType(CSEBML.DocTypes.Matroska.MatroskaVersion.V3);
			var ebmlReader = new EBMLReader(ebmlSrc, matroskaDoc);


			Action<bool> recurse = readValues => {
				try {
					ebmlReader.Reset();
					Recurse(ebmlReader, readValues);
				} catch(Exception ex) {
					ex.Data.Add("ReadValues", readValues);
					throw;
				}
			};

			recurse(true);
			recurse(false);
		}


		[TestMethod]
		public void BaseStream_Equals() {
			var dataSrc = new EBMLBlockDataSource(new byte[][] { new byte[0] }, 0);
			var docType = new EBMLDocType();
			var reader = new EBMLReader(dataSrc, docType);

			Assert.AreEqual(dataSrc, reader.BaseStream);
		}

		[TestMethod]
		public void DocType_Equals() {
			var dataSrc = new EBMLBlockDataSource(new byte[][] { new byte[0] }, 0);
			var docType = new EBMLDocType();
			var reader = new EBMLReader(dataSrc, docType);

			Assert.AreEqual(docType, reader.DocType);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void RetrieveValue_NullArgument() {
			var dataSrc = new EBMLBlockDataSource(new byte[][] { new byte[0] }, 0);
			var docType = new EBMLDocType();
			var reader = new EBMLReader(dataSrc, docType);
			reader.RetrieveValue(null);
		}


		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void RetrieveValue_UnknownLength() {
			var dataSrc = new EBMLBlockDataSource(new byte[][] { new byte[0] }, 0);
			var docType = new EBMLDocType();
			var reader = new EBMLReader(dataSrc, docType);
			reader.RetrieveValue(new ElementInfo(EBMLDocElement.Unknown, 1, 1, 0, null));
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void RetrieveValue_WrongPosition() {
			var dataSrc = new EBMLBlockDataSource(new byte[][] { new byte[0] }, 0);
			var docType = new EBMLDocType();
			var reader = new EBMLReader(dataSrc, docType);
			reader.RetrieveValue(new ElementInfo(EBMLDocElement.Unknown, 1, 2, 3, 1));
		}

		[TestMethod]
		public void RetrieveValue_ZeroLength() {
			var dataSrc = new EBMLBlockDataSource(new byte[][] { new byte[0] }, 0);
			var docType = new EBMLDocType();
			var reader = new EBMLReader(dataSrc, docType);

			reader.RetrieveValue(new ElementInfo(EBMLDocElement.Unknown, 0, 0, 0, 0));
			reader.RetrieveValue(new ElementInfo(EBMLDocElement.Unknown, 1, 1, 1, 0));
		}

		[TestMethod]
		public void Reset() {
			var dataSrc = new EBMLFixedByteArrayDataSource(new byte[] { 0x1A, 0x45, 0xDF, 0xA3, 0x80 });
			var docType = new EBMLDocType();
			var reader = new EBMLReader(dataSrc, docType);

			var elemInfo = reader.Next();
			using(reader.EnterElement(elemInfo)) {
				elemInfo = reader.Next();
			}

			reader.Reset();
			Assert.IsTrue(reader.BaseStream.Position == 0, "Bastream was not reset");
		}


		[TestMethod]
		public void JumpToElementAt() {
			var dataSrc = new EBMLFixedByteArrayDataSource(new byte[] { 0x1A, 0x45, 0xDF, 0xA3, 0x80 });
			var docType = new EBMLDocType();
			var reader = new EBMLReader(dataSrc, docType);

			var elemInfo = reader.Next();
			using(reader.EnterElement(elemInfo)) {
				reader.Next();
			}

			Assert.IsTrue(reader.JumpToElementAt(0).IdPos == elemInfo.IdPos, "Bastream was not reset");
		}


		private static void Recurse(EBMLReader reader, bool readValues) {
			ElementInfo elemInfo;
			while((elemInfo = reader.Next()) != null) {
				if(elemInfo.DocElement.Type == EBMLElementType.Master) {
					using(reader.EnterElement(elemInfo)) {
						Recurse(reader, readValues);
					}
				} else if(readValues) {
					var obj = reader.RetrieveValue(elemInfo);
				}
			}
		}
	}
}
