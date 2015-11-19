using System;
using System.Text;

namespace Metrics {

	public static class NexonStringUtils
    {
		// Bracket Pair Struct
		public struct BracketPair
        {
			public Char OpenerBracket;
			public Char CloserBracket;
		
            public BracketPair(Char OpenerBracket, Char CloserBracket)
            {
				this.OpenerBracket = OpenerBracket;
				this.CloserBracket = CloserBracket;
			}
		}

		public static BracketPair CurvedBrackets = new BracketPair('{', '}');
		public static BracketPair RoundBrackets = new BracketPair('(', ')');
		
		// Static methods
		public static Boolean CompareStringOffsetToExample(
			String ComparedString, Int32 Offset, String Example
		)
        {
			return (
				(ComparedString.Length >= Offset + Example.Length) &&
				(ComparedString.Substring(Offset, Example.Length).Equals(Example))
			);
		}
		
	}
}