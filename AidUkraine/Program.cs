var input_path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Help For Refugees Database.xlsx");

var data = Skalware.Utils.OfficeFormats.ReadExcelFile(input_path);
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

output_writer.SaveOutput(text_rows);
