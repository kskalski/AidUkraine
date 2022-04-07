var input_path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Help For Refugees Database.xlsx");

var data = Skalware.Utils.OfficeFormats.ReadExcelFile(input_path);
var cases = AidUkraine.Matcher.FilterStatus(
    new AidUkraine.ModelParser().ParseCases(data["Cases"]))
    .OrderByDescending(x => x.CurrentPriority)
    .ToList();
var hosts = AidUkraine.Matcher.FilterStatus(
    new AidUkraine.ModelParser().ParseHosts(data["Host Families"]))
    .ToList();

Console.WriteLine("Parsed and filtered {0} cases, {1} hosts", cases.Count(), hosts.Count());

var graph = AidUkraine.Matcher.BuildGraph(cases, hosts);

var matches = AidUkraine.Matcher.GenerateMatches(graph, hosts.Count);

var text_rows = AidUkraine.Writer.TransformMatchesToTextRows(matches, cases, hosts);
Console.WriteLine($"Generated {text_rows.Count} text rows");
AidUkraine.Writer.SaveOutput(text_rows);
