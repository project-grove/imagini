using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace Imagini.Fonts
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Provides printable character sets and methods for their creation.
    /// </summary>
    public static class Symbols
    {
        private static ISet<UnicodeCategory> s_printables =
            new HashSet<UnicodeCategory>(new[] {
            /* Letters */
            UnicodeCategory.LowercaseLetter,
            UnicodeCategory.ModifierLetter,
            UnicodeCategory.OtherLetter,
            UnicodeCategory.TitlecaseLetter,
            UnicodeCategory.UppercaseLetter,
            /* Mark */
            UnicodeCategory.EnclosingMark,
            UnicodeCategory.NonSpacingMark,
            UnicodeCategory.SpacingCombiningMark,
            /* Numbers */
            UnicodeCategory.LetterNumber,
            UnicodeCategory.DecimalDigitNumber,
            UnicodeCategory.OtherNumber,
            /* Punctuation */
            UnicodeCategory.ClosePunctuation,
            UnicodeCategory.ConnectorPunctuation,
            UnicodeCategory.DashPunctuation,
            UnicodeCategory.FinalQuotePunctuation,
            UnicodeCategory.InitialQuotePunctuation,
            UnicodeCategory.OpenPunctuation,
            UnicodeCategory.OtherPunctuation,
            /* Symbols */
            UnicodeCategory.CurrencySymbol,
            UnicodeCategory.MathSymbol,
            UnicodeCategory.ModifierSymbol,
            UnicodeCategory.OtherSymbol
        });

        // TODO: Add more character sets
        public static ISet<char> ASCII = PrintablesInRange(1, 127);
        public static ISet<char> Latin1Supplement = PrintablesInRange(0x00A0, 0x00FF);
        public static ISet<char> LatinExtendedA = PrintablesInRange(0x0100, 0x17F);
        public static ISet<char> LatinExtendedB = PrintablesInRange(0x0180, 0x024F);
        public static ISet<char> Cyrillic = PrintablesInRange(0x0400, 0x04FF);


        /// <summary>
        /// Gets the printable characters from the specified set.
        /// </summary>
        public static ISet<char> GetPrintable(IEnumerable<char> characters) =>
            new HashSet<char>(characters
                .Where(c =>
                    s_printables.Contains(char.GetUnicodeCategory(c)) ||
                    c == ' '));

        /// <summary>
        /// Gets the printable characters in the specified unicode character range.
        /// </summary>
        public static ISet<char> PrintablesInRange(int start, int end) =>
            GetPrintable(Enumerable.Range(start, end).Select(i => (char)i));
    }
}