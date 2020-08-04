using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Roslyn.Helpers
{
    public static class DiagnosticsHelper
    {
        public static void ShowDiagnostics(IEnumerable<Diagnostic> codeIssues)
        {
            foreach (Diagnostic codeIssue in codeIssues)
            {
                string issue = $"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()}, Location: { codeIssue.Location.GetLineSpan()}, Severity: { codeIssue.Severity}";
                Console.WriteLine(issue);
            }
        }
    }
}
