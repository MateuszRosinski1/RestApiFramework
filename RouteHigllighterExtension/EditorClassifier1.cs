using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.Build.Evaluation;
using System.IO;

namespace RouteHigllighterExtension
{
    /// <summary>
    /// Classifier that classifies all text as an instance of the "EditorClassifier1" classification type.
    /// </summary>
    internal class EditorClassifier1 : IClassifier
    {
        private readonly IClassificationType _blueTextType;
        private readonly IClassificationType _grayParamType;

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        public EditorClassifier1(IClassificationTypeRegistryService registry)
        {
            _blueTextType = registry.GetClassificationType("BlueRouteText");
            _grayParamType = registry.GetClassificationType("GrayRouteParam");
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            if (!IsRestAppFlagSetForBuffer(span.Snapshot.TextBuffer)) return Array.Empty<ClassificationSpan>();
            var result = new List<ClassificationSpan>();

            string text = span.GetText();

            foreach (Match attrMatch in Regex.Matches(text, @"\[Route\s*\(\s*""([^""]+)""\s*\)\s*\]"))
            {
                string routeContent = attrMatch.Groups[1].Value;
                int routeStart = attrMatch.Groups[1].Index;

                foreach (Match segment in Regex.Matches(routeContent, @"\{([^{}]+)\}"))
                {
                    int braceOpenIndex = routeContent.IndexOf("{" + segment.Groups[1].Value + "}");
                    if (braceOpenIndex < 0)
                        continue;

                    int globalOpen = span.Start + routeStart + braceOpenIndex;
                    int paramLength = segment.Groups[1].Value.Length;

                    var snapshot = span.Snapshot;

                    // { – blue
                    result.Add(new ClassificationSpan(
                        new SnapshotSpan(snapshot, new Span(globalOpen, 1)),
                        _blueTextType));

                    // środek – lightDray
                    result.Add(new ClassificationSpan(
                        new SnapshotSpan(snapshot, new Span(globalOpen + 1, paramLength)),
                        _grayParamType));

                    // } – blue
                    result.Add(new ClassificationSpan(
                        new SnapshotSpan(snapshot, new Span(globalOpen + 1 + paramLength, 1)),
                        _blueTextType));
                }
            }

            return result;
        }

        bool IsRestAppFlagSetForBuffer(ITextBuffer buffer)
        {
            var filePath = buffer.GetFilePath();
            if (string.IsNullOrEmpty(filePath))
                return false;

            string directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory))
                return false;
            System.Diagnostics.Debug.WriteLine($"Searching .csproj dla file: {filePath}");
            string csprojPath = FindCsprojPath(directory, filePath);
            System.Diagnostics.Debug.WriteLine($"Found csproj file: {csprojPath}");

            if (csprojPath == null)
                return false;

            try
            {
                var csprojString = File.ReadAllText(csprojPath);
                var isRestAppValue = csprojString.Contains("<IsRestApp>true</IsRestApp>");

                return isRestAppValue;
            }
            catch
            {
                return false;
            }
        }

        string FindCsprojPath(string startDirectory, string filePath)
        {
            var dir = new DirectoryInfo(startDirectory);
            while (dir != null)
            {
                var csprojFiles = dir.GetFiles("*.csproj");
                foreach (var csprojFile in csprojFiles)
                {
                    var projectDir = csprojFile.DirectoryName;
                    if (filePath.StartsWith(projectDir, StringComparison.OrdinalIgnoreCase))
                    {
                        return csprojFile.FullName;
                    }
                }
                dir = dir.Parent;
            }
            return null;
        }
    }
    public static class TextBufferExtensions
    {
        public static string GetFilePath(this ITextBuffer buffer)
        {
            if (buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument doc))
            {
                return doc.FilePath;
            }

            return null;
        }
    }
}
