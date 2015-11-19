using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Metrics
{
    public abstract class Preprocessor
    {
        public const String BorderOfStringLiteral = "\"";
        public const String BorderOfCharLiteral = "'";
        public const String OpenerOfSingleLineComment = "//";
        public const String CloserOfSingleLineComment = "\n";
        public const String OpenerOfMultiLineComment = "/*";
        public const String CloserOfMultiLineComment = "*/";

        public const Int32 IndexNotFound = -1;

        public enum CodePartSpecificator
        {
            Plain, SingleLineComment, MultilineComment, String, Character
        }

        public enum LiteralType { String, Character }
        public enum CommentType { SingleLine, MultiLine }

        // Bracket Pair Struct
        public struct PairOfBracket
        {
            public Char OpenerBracket;
            public Char CloserBracket;

            public PairOfBracket(Char OpenerBracket, Char CloserBracket)
            {
                this.OpenerBracket = OpenerBracket;
                this.CloserBracket = CloserBracket;
            }
        }

        public static PairOfBracket CurvedBrackets = new PairOfBracket('{', '}');
        public static PairOfBracket RoundBrackets = new PairOfBracket('(', ')');

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
	
        public static String RemoveCommentsAndStrings(String Code)
        {
            StringBuilder CodeBuilder = new StringBuilder();

            CodePartSpecificator CodeSpecificator = CodePartSpecificator.Plain;

            for (Int32 i = 0; i < Code.Length; i++)
            {
                CodeSpecificator = CheckCodePartSpecificator(Code, i);

                Int32 CloserIndex;
                switch (CodeSpecificator)
                {
                    case CodePartSpecificator.SingleLineComment:
                        CloserIndex = GetCloserOfPositionOfComment(Code, i, CommentType.SingleLine);
                        i = (CloserIndex != IndexNotFound) ? (CloserIndex + CloserOfSingleLineComment.Length - 1) : Code.Length;
                        break;
                    case CodePartSpecificator.MultilineComment:
                        CloserIndex = GetCloserOfPositionOfComment(Code, i, CommentType.MultiLine);
                        i = (CloserIndex != IndexNotFound) ? (CloserIndex + CloserOfMultiLineComment.Length - 1) : Code.Length;
                        break;
                    case CodePartSpecificator.String:
                        CloserIndex = GetCloserOfPositionOfLiteral(Code, i + BorderOfStringLiteral.Length, LiteralType.String);
                        i = (CloserIndex != IndexNotFound) ? CloserIndex : Code.Length;
                        CodeBuilder.Append("STRING");
                        break;
                    case CodePartSpecificator.Character:
                        CloserIndex = GetCloserOfPositionOfLiteral(Code, i + BorderOfCharLiteral.Length, LiteralType.Character);
                        i = (CloserIndex != IndexNotFound) ? CloserIndex : Code.Length;
                        CodeBuilder.Append("CHAR");
                        break;
                    default:
                        CodeBuilder.Append(Code[i]);
                        break;
                }
            }
            return CodeBuilder.ToString();
        }

        public static String RemoveInstructions(String Code)
        {
            Regex CompilerInstructionRegex = new Regex(@"#.*");
            return CompilerInstructionRegex.Replace(Code, String.Empty);
        }

        public static String PreprocessCode(String Code)
        {
            return RemoveInstructions(RemoveCommentsAndStrings(Code)).Trim();
        }

        public static CodePartSpecificator CheckCodePartSpecificator(String Code, Int32 Index)
        {
            if (IsOpenerOfStringLiteral(Code, Index))
                return CodePartSpecificator.String;
            else if (IsOpenerOfCharLiteral(Code, Index))
                return CodePartSpecificator.Character;
            else if (IsOpenerOfSingleLineComment(Code, Index))
                return CodePartSpecificator.SingleLineComment;
            else if (IsOpenerOfMultiLineComment(Code, Index))
                return CodePartSpecificator.MultilineComment;

            return CodePartSpecificator.Plain;
        }

        public static Int32 GetCloserOfPositionOfLiteral(String Code, Int32 SearchOffset, LiteralType Type)
        {
            Regex Expression = (Type == LiteralType.String)
                ? (new Regex("[^\\\\]?\""))
                : (new Regex("[^\\\\]?'"));

            Match CloserMatch = Expression.Match(Code, SearchOffset);
            return (CloserMatch.Success) ? (CloserMatch.Index + CloserMatch.Length - 1) : IndexNotFound;
        }

        public static Int32 GetCloserOfPositionOfComment(String Code, Int32 SearchOffset, CommentType Type)
        {
            switch (Type)
            {
                case CommentType.SingleLine:
                    return Code.IndexOf(CloserOfSingleLineComment, SearchOffset);
                case CommentType.MultiLine:
                    return Code.IndexOf(CloserOfMultiLineComment, SearchOffset);
                default:
                    return IndexNotFound;
            }
        }

        public static Boolean IsOpenerOfStringLiteral(String Code, Int32 Offset)
        {
            return CompareStringOffsetToExample(Code, Offset, BorderOfStringLiteral);
        }

        public static Boolean IsOpenerOfCharLiteral(String Code, Int32 Offset)
        {
            return CompareStringOffsetToExample(Code, Offset, BorderOfCharLiteral);
        }

        public static Boolean IsOpenerOfSingleLineComment(String Code, Int32 Offset)
        {
            return CompareStringOffsetToExample(Code, Offset, OpenerOfSingleLineComment);
        }

        public static Boolean IsOpenerOfMultiLineComment(String Code, Int32 Offset)
        {
            return CompareStringOffsetToExample(Code, Offset, OpenerOfMultiLineComment);
        }
    }
}
