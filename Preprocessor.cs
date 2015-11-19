using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Metrics
{

    public abstract class Preprocessor
    {

        public const String StringLiteralBorder = "\"";
        public const String CharLiteralBorder = "'";
        public const String SingleLineCommentOpener = "//";
        public const String SingleLineCommentCloser = "\n";
        public const String MultiLineCommentOpener = "/*";
        public const String MultiLineCommentCloser = "*/";

        public const Int32 IndexNotFound = -1;

        public enum CodePartSpecificator
        {
            Plain, SingleLineComment, MultilineComment, String, Character
        }

        public enum LiteralType { String, Character }
        public enum CommentType { SingleLine, MultiLine }

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
                        CloserIndex = GetCommentCloserPosition(Code, i, CommentType.SingleLine);
                        i = (CloserIndex != IndexNotFound) ? (CloserIndex + SingleLineCommentCloser.Length - 1) : Code.Length;
                        break;
                    case CodePartSpecificator.MultilineComment:
                        CloserIndex = GetCommentCloserPosition(Code, i, CommentType.MultiLine);
                        i = (CloserIndex != IndexNotFound) ? (CloserIndex + MultiLineCommentCloser.Length - 1) : Code.Length;
                        break;
                    case CodePartSpecificator.String:
                        CloserIndex = GetLiteralCloserPosition(Code, i + StringLiteralBorder.Length, LiteralType.String);
                        i = (CloserIndex != IndexNotFound) ? CloserIndex : Code.Length;
                        CodeBuilder.Append("STRING");
                        break;
                    case CodePartSpecificator.Character:
                        CloserIndex = GetLiteralCloserPosition(Code, i + CharLiteralBorder.Length, LiteralType.Character);
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

        public static String RemoveCompilerInstructions(String Code)
        {
            Regex CompilerInstructionRegex = new Regex(@"#.*");
            return CompilerInstructionRegex.Replace(Code, String.Empty);
        }

        public static String PreprocessCode(String Code)
        {
            return RemoveCompilerInstructions(RemoveCommentsAndStrings(Code)).Trim();
        }

        public static CodePartSpecificator CheckCodePartSpecificator(String Code, Int32 Index)
        {
            if (IsStringLiteralOpener(Code, Index))
                return CodePartSpecificator.String;
            else if (IsCharLiteralOpener(Code, Index))
                return CodePartSpecificator.Character;
            else if (IsSingleLineCommentOpener(Code, Index))
                return CodePartSpecificator.SingleLineComment;
            else if (IsMultiLineCommentOpener(Code, Index))
                return CodePartSpecificator.MultilineComment;

            return CodePartSpecificator.Plain;
        }

        public static Int32 GetLiteralCloserPosition(String Code, Int32 SearchOffset, LiteralType Type)
        {
            Regex Expression = (Type == LiteralType.String)
                ? (new Regex("[^\\\\]?\""))
                : (new Regex("[^\\\\]?'"));

            Match CloserMatch = Expression.Match(Code, SearchOffset);
            return (CloserMatch.Success) ? (CloserMatch.Index + CloserMatch.Length - 1) : IndexNotFound;
        }

        public static Int32 GetCommentCloserPosition(String Code, Int32 SearchOffset, CommentType Type)
        {
            switch (Type)
            {
                case CommentType.SingleLine:
                    return Code.IndexOf(SingleLineCommentCloser, SearchOffset);
                case CommentType.MultiLine:
                    return Code.IndexOf(MultiLineCommentCloser, SearchOffset);
                default:
                    return IndexNotFound;
            }
        }

        public static Boolean IsStringLiteralOpener(String Code, Int32 Offset)
        {
            return NexonStringUtils.CompareStringOffsetToExample(Code, Offset, StringLiteralBorder);
        }

        public static Boolean IsCharLiteralOpener(String Code, Int32 Offset)
        {
            return NexonStringUtils.CompareStringOffsetToExample(Code, Offset, CharLiteralBorder);
        }

        public static Boolean IsSingleLineCommentOpener(String Code, Int32 Offset)
        {
            return NexonStringUtils.CompareStringOffsetToExample(Code, Offset, SingleLineCommentOpener);
        }

        public static Boolean IsMultiLineCommentOpener(String Code, Int32 Offset)
        {
            return NexonStringUtils.CompareStringOffsetToExample(Code, Offset, MultiLineCommentOpener);
        }

    }

}