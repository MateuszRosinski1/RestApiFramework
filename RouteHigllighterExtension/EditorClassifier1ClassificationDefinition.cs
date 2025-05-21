using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace RouteHigllighterExtension
{
    internal static class EditorClassifier1ClassificationDefinition
    {
        // This disables "The field is never used" compiler's warning. Justification: the field is used by MEF.
#pragma warning disable 169

        /// <summary>
        /// Defines the "EditorClassifier1" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("EditorClassifier1")]
        private static ClassificationTypeDefinition typeDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("BlueRouteText")]
        private static ClassificationTypeDefinition blueType;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("GrayRouteParam")]
        private static ClassificationTypeDefinition grayType;


#pragma warning restore 169
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "BlueRouteText")]
    [Name("BlueRouteText")]
    [UserVisible(true)]
    [Order(After = Priority.Default)]
    internal sealed class BlueRouteTextFormat : ClassificationFormatDefinition
    {
        public BlueRouteTextFormat()
        {
            this.DisplayName = "Blue Route Text";
            this.ForegroundColor = Color.FromRgb(30, 101, 255);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "GrayRouteParam")]
    [Name("GrayRouteParam")]
    [UserVisible(true)]
    [Order(After = Priority.Default)]
    internal sealed class GrayRouteParamFormat : ClassificationFormatDefinition
    {
        public GrayRouteParamFormat()
        {
            this.DisplayName = "Gray Route Param";
            this.ForegroundColor = Colors.LightGray;
        }
    }
}
