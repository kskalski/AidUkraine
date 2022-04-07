using System.Globalization;

namespace AidUkraine {
    internal class Writer {
        internal static List<string[]> TransformMatchesToTextRows(IReadOnlyList<int>[] matches, IReadOnlyList<Data.Case> cases, IReadOnlyList<Data.Host> hosts) {
            var matches_text_rows = new List<string[]>();
            for (int ci = 0; ci < matches.Length; ++ci)
                foreach (var hi in matches[ci]) {
                    var text_cells = new string[] { 
                        cases[ci].Name, cases[ci].OriginIndex.ToString(), hosts[hi].Name, hosts[hi].OriginIndex.ToString() 
                    };
                    matches_text_rows.Add(text_cells);
                }
            return matches_text_rows;
        }


        internal static void SaveOutput(IReadOnlyList<string[]> text_rows) {
            var date_str = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture.DateTimeFormat.SortableDateTimePattern).Substring(0, 13);
            var output_path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), $"Generated matches.{date_str}.xlsx");
            var text_header = new string[] { "Case", "Case row", "Host", "Host row" };
            Skalware.Utils.OfficeFormats.SaveExcelFile(output_path, text_header, text_rows);
        }
    }
}
