using System;
using System.Text.RegularExpressions;

namespace Konfus.Utility.Custom_Types
{
    /// <summary>
    /// A struct that contains stack trace info such a line number, filename, and a message with a hyperlink for Unity logs.
    /// </summary>
    public struct UnityStackTraceInfo
    {
        private const string Pattern = @"in (.*):line (\d+)";
        
        public string Message { get; }
        public string FileName { get; }
        public int LineNumber { get; }
        
        public UnityStackTraceInfo(string stackTrace)
        {
            var match = Regex.Match(stackTrace, Pattern);
            if (!match.Success)
            {
                throw new ArgumentException("Invalid stack trace");
            }
            FileName = match.Groups[1].Value;
            LineNumber = int.Parse(match.Groups[2].Value);
            Message = Regex.Replace(input: stackTrace, pattern: Pattern, replacement: $"<a href=\"{FileName}\" line=\"{LineNumber}\">{FileName}</a>");
        }

        public override string ToString()
        {
            return Message;
        }
        
        public static implicit operator string(UnityStackTraceInfo info)
        {
            return info.ToString();
        }
    }
}