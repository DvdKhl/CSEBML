using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSEBML.DataSource {
	public class BytePatterns {
		private class Node {
			public byte Value;

			public bool InDictionary;
			public Node Parent;
			public Node Suffix;
			public Node DictSuffix;

			public Node[] Children = new Node[256];
		}

		private Node root, current;
		private Dictionary<Node, byte[]> patternLookup;

		public BytePatterns(byte[][] patterns) {

			//Build Trie
			var root = new Node();
			var nodes = new List<Node>();
			patternLookup = new Dictionary<Node, byte[]>();
			foreach(var pattern in patterns) {
				var curNode = root;
				foreach(var value in pattern) {
					if(curNode.Children[value] == null) {
						curNode.Children[value] = new Node {
							Value = value,
							Parent = curNode
						};
						nodes.Add(curNode.Children[value]);
					}

	
					curNode = curNode.Children[value];
				}
				patternLookup.Add(curNode, pattern);
				curNode.InDictionary = true;
			}

			//Determine Suffix and DictSuffix
			foreach(var nodeA in nodes) {
				//Get longest Suffix
				int longestSuffixLength = 0;
				Node longestSuffixNode = null;
				foreach(var nodeB in nodes) {
					if(nodeB == nodeA) continue;

					var curNodeA = nodeA;
					var curNodeB = nodeB;
					var curLongestSuffixLength = 0;

					while(curNodeB != root && curNodeA != root) {
						if(curNodeB.Value != curNodeA.Value) break;
						curNodeA = curNodeA.Parent;
						curNodeB = curNodeB.Parent;
						curLongestSuffixLength++;
					}

					if(curLongestSuffixLength > 0 && curNodeB == root && curLongestSuffixLength > longestSuffixLength) {
						longestSuffixLength = curLongestSuffixLength;
						longestSuffixNode = nodeB;
					}
				}
				nodeA.Suffix = longestSuffixNode ?? root;

				//Get longest suffix in patterns
				Node curSuffixNode = nodeA.Suffix;
				while(curSuffixNode != null && curSuffixNode != root) {
					if(curSuffixNode.InDictionary) {
						nodeA.DictSuffix = curSuffixNode;
						break;
					}

					curSuffixNode = curSuffixNode.Suffix;
				}
			}

			this.current = this.root = root;
		}

		public void Match(byte[] data, int offset, Func<byte[], int, bool> match) {
			for(int i = offset;i < data.Length;i++) {
				if(current.Children[data[i]] != null) {
					current = current.Children[data[i]];
				} else {

					while(current != root && current.Children[data[i]] == null) current = current.Suffix;
					current = current.Children[data[i]] ?? root;

				}

				Node matchNode = current.InDictionary ? current : current.DictSuffix;
				while(matchNode != null) {
					var pattern = patternLookup[matchNode];
					if(!match(pattern, i - pattern.Length + 1)) return;
					matchNode = matchNode.DictSuffix;
				}
			}
		}
	}
}
