var root_dir = Environment.GetCommandLineArgs().Skip(1).FirstOrDefault();
if (string.IsNullOrWhiteSpace(root_dir))
    root_dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
Console.WriteLine("Will use config files from {0}", root_dir);

var google_creds_path = Path.Join(root_dir, "aid-ukraine-credentials.json");
var google_spreadsheet_id = File.ReadAllText(Path.Join(root_dir, "aid-ukraine-spreadsheet-id.txt")).Trim();
var input_path = Path.Join(root_dir, "Help For Refugees Database.xlsx");

var data = string.IsNullOrEmpty(google_spreadsheet_id) ?
    Skalware.Utils.OfficeFormats.ReadExcelFile(input_path)
    : await Skalware.Utils.OfficeFormats.ReadGoogleSpreadsheet(google_creds_path, google_spreadsheet_id);

var cases_parser = new AidUkraine.ModelParser(); 
var cases = AidUkraine.Matcher.FilterStatus(
    cases_parser.ParseCases(data[AidUkraine.ModelParser.CASES_SHEET_NAME]))
    .OrderByDescending(x => x.CurrentPriority)
    .ToList();
var hosts_parser = new AidUkraine.ModelParser();
var hosts = AidUkraine.Matcher.FilterStatus(
    hosts_parser.ParseHosts(data[AidUkraine.ModelParser.HOSTS_SHEET_NAME]))
    .ToList();

Console.WriteLine("Parsed and filtered {0} cases, {1} hosts", cases.Count(), hosts.Count());

var graph = AidUkraine.Matcher.BuildGraph(cases, hosts);

var matches = AidUkraine.Matcher.GenerateMatches(graph, hosts.Count);

var output_writer = new AidUkraine.Writer(cases_parser, hosts_parser);

var text_rows = output_writer.TransformMatchesToTextRows(matches, cases, hosts);
Console.WriteLine($"Generated {text_rows.Count} text rows");

if (string.IsNullOrEmpty(google_spreadsheet_id))
    output_writer.SaveOutput(text_rows);
else
    await Skalware.Utils.OfficeFormats.UpdateGoogleSpreadsheet(google_creds_path, google_spreadsheet_id, AidUkraine.Writer.OUTPUT_SHEET_NAME, AidUkraine.Writer.OUTPUT_HEADERS, text_rows);