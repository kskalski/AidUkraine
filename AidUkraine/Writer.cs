using System.Globalization;

namespace AidUkraine {
    internal class Writer {
        public Writer(ModelParser cases_parser, ModelParser hosts_parser) {
            cases_parser_ = cases_parser;
            hosts_parser_ = hosts_parser;
        }

        internal List<string[]> TransformMatchesToTextRows(IReadOnlyList<int>[] matches, IReadOnlyList<Data.Case> cases, IReadOnlyList<Data.Host> hosts) {
            var matches_text_rows = new List<string[]>();
            for (int ci = 0; ci < matches.Length; ++ci) {
                var match = matches[ci];
                if (match == null)
                    continue;
                var case_val = cases[ci];
                var text_cells = new List<string>() {
                    case_val.Caseid,
                    cases_ref_for(nameof(case_val.Name), case_val.OriginIndex),
                    cases_ref_for(nameof(case_val.Status), case_val.OriginIndex),
                    ":"
                };
                foreach (var hi in match) {
                    var host_val = hosts[hi];
                    text_cells.Add(host_val.HostId);
                    text_cells.Add(hosts_ref_for(nameof(host_val.HostName), host_val.OriginIndex));
                    text_cells.Add(hosts_ref_for(nameof(host_val.Status), host_val.OriginIndex));
                }
                matches_text_rows.Add(text_cells.ToArray());
            }
            return matches_text_rows;
        }

        string cases_ref_for(string name, int row_index) {
            char letter = (char)('A' + cases_parser_.HeaderIndexOf(name));
            return $"'{ModelParser.CASES_SHEET_NAME}'!{letter}{row_index}";
        }
        string hosts_ref_for(string name, int row_index) {
            char letter = (char)('A' + hosts_parser_.HeaderIndexOf(name));
            return $"'{ModelParser.HOSTS_SHEET_NAME}'!{letter}{row_index}";
        }

        internal void SaveOutput(IReadOnlyList<string[]> text_rows) {
            var date_str = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture.DateTimeFormat.SortableDateTimePattern).Substring(0, 13);
            var output_path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), $"Generated matches.{date_str}.xlsx");
            var text_header = new string[] { "Case Id", "Case name", "Case status", "", "Host1 Id", "Host1 name", "Host1 status", "Host2 Id", "Host2 name", "Host2 status", "..." };
            Skalware.Utils.OfficeFormats.SaveExcelFile(output_path, text_header, text_rows);
        }

        ModelParser cases_parser_;
        ModelParser hosts_parser_;
    }
}
