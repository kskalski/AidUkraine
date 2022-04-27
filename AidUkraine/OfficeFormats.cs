using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;

namespace Skalware.Utils {
    public class CSVExcelException : Exception {
        public CSVExcelException(string message) : base(message) { }
        public CSVExcelException(string message, Exception e) : base(message, e) { }
    }

    public class OfficeFormats {
        public static IDictionary<string, List<Tuple<int, string[]>>> ReadExcelFile(string file_path) {
            using var stream = new FileStream(file_path, FileMode.Open);
            return ReadExcelStream(stream);
        }

        public static IDictionary<string, List<Tuple<int, string[]>>> ReadExcelStream(Stream stream) {
            var dict = new Dictionary<string, List<Tuple<int, string[]>>>();
            var workbook = new XSSFWorkbook(stream);
            workbook.MissingCellPolicy = MissingCellPolicy.RETURN_NULL_AND_BLANK;

            for (int s = 0; s < workbook.NumberOfSheets; ++s) {
                var sheet = workbook.GetSheetAt(s);

                var rows = new List<Tuple<int, string[]>>();
                int num_consecutive_empty = 0;
                int? header_first_col = null;
                for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum && num_consecutive_empty <= 5; ++i) {
                    IRow row = sheet.GetRow(i);
                    if (row == null || row.Cells.All(d => d.CellType == CellType.Blank)) {
                        ++num_consecutive_empty;
                        continue;
                    }
                    header_first_col ??= row.FirstCellNum;
                    string[] cells = new string[Math.Max(0, row.LastCellNum - header_first_col.Value)];
                    bool has_nonempty = false;
                    for (int j = header_first_col.Value; j < row.LastCellNum; ++j) {
                        var val = cells[j - header_first_col.Value] = row.GetCell(j)?.ToString() ?? "";
                        if (!string.IsNullOrWhiteSpace(val))
                            has_nonempty = true;
                    }
                    if (has_nonempty) {
                        num_consecutive_empty = 0;
                        rows.Add(Tuple.Create(row.RowNum + 1, cells));
                    } else {
                        ++num_consecutive_empty;
                    }
                }
                dict[sheet.SheetName] = rows;
            }
            return dict;
        }

        public static List<string[]> ReadWordFile(string file_path) {
            using var input_stream = new FileStream(file_path, FileMode.Open);
            return ReadWordStream(input_stream);
        }
        public static List<string[]> ReadWordStream(Stream input_stream) {
            var result = new List<string[]>();
            try {
                var document = new XWPFDocument(input_stream);
                result.Capacity = document.Paragraphs.Count;
                foreach (var paragraph in document.Paragraphs) {
                    result.Add(new string[] { paragraph.ParagraphText });
                }
            } catch (Exception e) {
                throw new CSVExcelException(e.Message, e);
            }
            return result;
        }

        public static void SaveExcelFile(string file_path, string[] header, IReadOnlyList<string[]> data) {
            using var fs = new FileStream(file_path, FileMode.Create, FileAccess.Write);
            SaveExcelStream(fs, Path.GetFileNameWithoutExtension(file_path), header, data);
        }
        public static void SaveExcelStream(Stream output_stream, string name, string[] header, IReadOnlyList<string[]> data) {
            var workbook = new XSSFWorkbook();
            var excelSheet = workbook.CreateSheet(name);

            var header_font = workbook.CreateFont();
            header_font.IsBold = true;
            var header_style = workbook.CreateCellStyle();
            header_style.Alignment = HorizontalAlignment.Center;
            header_style.SetFont(header_font);
            IRow row = excelSheet.CreateRow(0);
            for (int c = 0; c < header.Length; ++c) {
                var cell = row.CreateCell(c);
                cell.CellStyle = header_style;
                cell.SetCellValue(header[c]);
            }

            for (int r = 1; r <= data.Count; ++r) {
                var dsrow = data[r - 1];
                row = excelSheet.CreateRow(r);
                for (int c = 0; c < dsrow.Length; ++c) {
                    var cell = row.CreateCell(c);
                    if (dsrow[c].Contains('!'))
                        cell.SetCellFormula(dsrow[c]);
                    else
                        cell.SetCellValue(dsrow[c]);
                }
            }
            workbook.Write(output_stream);
        }

        public static async Task<IDictionary<string, List<Tuple<int, string[]>>>> ReadGoogleSpreadsheet(string credentials_path, string spreadsheet_id) {
            var credential = await ReadGoogleCredentials(credentials_path);

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer() {
                HttpClientInitializer = credential,
                ApplicationName = "AidUkraine",
            });

            var request = service.Spreadsheets.Get(spreadsheet_id);
            var response = await request.ExecuteAsync();

            var dict = new Dictionary<string, List<Tuple<int, string[]>>>();
            var workbook = response.Sheets;

            for (int s = 0; s < workbook.Count; ++s) {
                var sheet = await service.Spreadsheets.Values.Get(spreadsheet_id, workbook[s].Properties.Title).ExecuteAsync();

                var rows = new List<Tuple<int, string[]>>();
                int num_consecutive_empty = 0;

                if (sheet.Values == null)
                    continue;

                for (int i = 0; i < sheet.Values.Count && num_consecutive_empty <= 5; ++i) {
                    var row = sheet.Values[i];
                    if (row == null || row.All(d => string.IsNullOrWhiteSpace(d as string))) {
                        ++num_consecutive_empty;
                        continue;
                    }
                    string[] cells = new string[row.Count];
                    bool has_nonempty = false;
                    for (int j = 0; j < row.Count; ++j) {
                        var val = cells[j] = (row[j] as string) ?? "";
                        if (!string.IsNullOrWhiteSpace(val))
                            has_nonempty = true;
                    }
                    if (has_nonempty) {
                        num_consecutive_empty = 0;
                        rows.Add(Tuple.Create(1 + i, cells));
                    } else {
                        ++num_consecutive_empty;
                    }
                }
                dict[workbook[s].Properties.Title] = rows;
            }
            return dict;
        }

        static async Task<GoogleCredential> ReadGoogleCredentials(string credentials_path) {
            using var stream = new FileStream(credentials_path, FileMode.Open, FileAccess.Read);
            var raw_credentials = await GoogleCredential.FromStreamAsync(stream, CancellationToken.None);
            string[] scopes = { SheetsService.Scope.SpreadsheetsReadonly };
            return raw_credentials.CreateScoped(scopes);
        }
    }
}