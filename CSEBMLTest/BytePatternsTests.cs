using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Diagnostics;

namespace CSEBMLTest {
	[TestClass]
	public class BytePatternsTests {

		[TestMethod]
		public void TestPatternMatching() {
			var matcher = new CSEBML.DataSource.BytePatterns(
				new byte[][] {
					new byte[]{ 0x00, 0x01, 0x02 },
					new byte[]{ 0x01, 0x02, 0x03 },
					new byte[]{ 0x02, 0x03, 0x04 },
					new byte[]{ 0x03, 0x04, 0x05 }
				}
			);



			var b = Enumerable.Range(0, 512).Select(i => (byte)i).ToArray();

			matcher.Match(b, 0, (p, i) => {
				Debug.WriteLine(i);
				return true;
			});

		}
	}
}
