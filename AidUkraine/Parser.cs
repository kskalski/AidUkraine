using System.Text;

namespace AidUkraine {
    class ModelParser {
        internal const string CASES_SHEET_NAME = "Cases";
        internal const string HOSTS_SHEET_NAME = "Host Families";

        public List<Data.Case> ParseCases(List<Tuple<int, string[]>> rows) {
            parse_header(rows.Skip(2));
            var cases = rows.Skip(3).Select(num_row => {
                var (num, row) = num_row;
                var c = new Data.Case();
                c.OriginIndex = num;
                c.Caseid = column_for(row, nameof(c.Caseid));
                c.Name = column_for(row, nameof(c.Name));
                c.CurrentPriority = parse_priority(column_for(row, nameof(c.CurrentPriority)));
                c.Status = parse_status(column_for(row, nameof(c.Status)));
                c.SupportPerson = column_for(row, nameof(c.SupportPerson));
                c.OutstandingActions = column_for(row, nameof(c.OutstandingActions));
                c.Description = column_for(row, "description of the group needing help");
                c.LanguagesSpoken = parse_languages(column_for(row, nameof(c.LanguagesSpoken)));
                c.ContactNumber = column_for(row, nameof(c.ContactNumber));
                c.Email = column_for(row, nameof(c.Email));
                c.FacebookLink = column_for(row, "Facebook link");
                if (int.TryParse(column_for(row, "Adults"), out var num_adults))
                    c.NumAdults = num_adults;
                if (int.TryParse(column_for(row, "no of children"), out var num_children))
                    c.NumChildren = num_children;
                if (int.TryParse(column_for(row, nameof(c.TotalNumberOfPeople)), out var num_people))
                    c.TotalNumberOfPeople = num_people;
                c.ChildrenAges = parse_ints(split_list(column_for(row, "Ages")));
                c.HasPets = parse_yes_no(column_for(row, "Pets"));
                c.PetTypes = column_for(row, "Types");
                c.HasSpecialNeeds = parse_yes_no(column_for(row, "Special Needs"));
                c.Smoker = parse_yes_no_empty(column_for(row, nameof(c.Smoker)));
                c.CurrentLocation = column_for(row, nameof(c.CurrentLocation));
                c.WantedDestination = column_for(row, nameof(c.WantedDestination));
                c.HostFamily = column_for(row, nameof(c.HostFamily));
                return c;
            }).Where(c => c.NumPeopleTotal > 0).ToList();
            return cases;
        }

        public List<Data.Host> ParseHosts(List<Tuple<int, string[]>> rows) {
            parse_header(rows);
            var cases = rows.Skip(2).Select(num_row => {
                var (num, row) = num_row;
                var h = new Data.Host();
                h.OriginIndex = num;
                h.HostId = column_for(row, nameof(h.HostId));
                h.HostName = column_for(row, nameof(h.HostName));
                h.Status = parse_status(column_for(row, nameof(h.Status)));
                h.PrimaryContact = column_for(row, nameof(h.PrimaryContact));
                h.OutstandingActions = column_for(row, nameof(h.OutstandingActions));
                h.Notes = column_for(row, nameof(h.Notes));
                h.DescriptionOfOffer = column_for(row, nameof(h.DescriptionOfOffer));
                //h.LanguagesSpoken = parse_languages(column_for(row, nameof(h.LanguagesSpoken)));
                h.PhoneNumber = column_for(row, "Phone number");
                h.Email = column_for(row, nameof(h.Email));
                h.FacebookLink = column_for(row, "Facebook link");
                h.Location = column_for(row, nameof(h.Location));
                h.WillHostPets = parse_yes_no(column_for(row, "Will host pets"));
                h.WillHostPetsKinds = column_for(row, "What kind of pets");
                h.WillHostChildren = parse_yes_no(column_for(row, "Will host children"));
                h.WillHostChildrenKinds = column_for(row, "What kinds of children");
                h.WillHostSpecialNeeds = parse_yes_no(column_for(row, "Will host special needs"));
                h.RoomsAvailable = column_for(row, "Number of rooms available");
                h.RegisteredWithGov = parse_yes_no(column_for(row, "Registered on Gov website"));
                h.SmokerInHouse = parse_yes_no_empty(column_for(row, nameof(h.SmokerInHouse)));
                if (int.TryParse(column_for(row, "Max no of people they can take"), out var max_num_people))
                    h.MaxNumPeople = max_num_people;
                return h;
            }).ToList();
            return cases;
        }

        void parse_header(IEnumerable<Tuple<int, string[]>> rows) {
            var first_row = rows.First().Item2;
            for (int i = 0; i < first_row.Length; ++i) {
                var name = first_row[i];
                if (string.IsNullOrWhiteSpace(name))
                    continue;
                name = name.Split(new char[] { '-', '/', '(', ')' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).First();
                name = name.ToLower();
                headers_[name] = i;
            }
        }

        internal int HeaderIndexOf(string name) {
            if (name.IndexOf(' ') < 0) {
                var name_builder = new StringBuilder();
                for (int i = 1; i < name.Length; ++i) {
                    name_builder.Append(name[i - 1]);
                    if (char.IsLower(name[i - 1]) && char.IsUpper(name[i]))
                        name_builder.Append(' ');
                }
                name_builder.Append(name[name.Length - 1]);
                name = name_builder.ToString();
            }
            name = name.ToLower();
            return headers_[name];
        }

        string column_for(string[] row, string name) {
            var idx = HeaderIndexOf(name);
            if (row.Length <= idx)
                return "";
            return row[idx].Trim();
        }

        static Data.Priority parse_priority(string text) {
            if (string.IsNullOrWhiteSpace(text))
                return Data.Priority.None;
            text = text.Split(new char[] { '/', '-', '(', ')' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).First();
            switch (text) {
                case nameof(Data.Priority.Low): return Data.Priority.Low;
                case nameof(Data.Priority.Medium): return Data.Priority.Medium;
                case nameof(Data.Priority.High): return Data.Priority.High;
                case nameof(Data.Priority.Urgent): return Data.Priority.Urgent;
                default: return Data.Priority.None;
            }
        }

        static Data.Language[] parse_languages(string text) {
            var result = new List<Data.Language>();
            foreach (var word in split_list(text)) {
                if (word.StartsWith("Ru", StringComparison.InvariantCultureIgnoreCase) || word.StartsWith("Русс", StringComparison.InvariantCultureIgnoreCase))
                    result.Add(Data.Language.RUSSIAN);
                else if (word.StartsWith("Eng", StringComparison.InvariantCultureIgnoreCase) || word.StartsWith("англ", StringComparison.InvariantCultureIgnoreCase))
                    result.Add(Data.Language.ENGLISH);
                else if (word.StartsWith("Ukr", StringComparison.InvariantCultureIgnoreCase) || word.StartsWith("UA", StringComparison.InvariantCultureIgnoreCase) || word.StartsWith("Укра", StringComparison.InvariantCultureIgnoreCase))
                    result.Add(Data.Language.UKRAINIAN);
                else if (word.StartsWith("Pol", StringComparison.InvariantCultureIgnoreCase) || word.StartsWith("PL", StringComparison.InvariantCultureIgnoreCase))
                    result.Add(Data.Language.POLISH);
                else if (word.StartsWith("Fren", StringComparison.InvariantCultureIgnoreCase) || word.StartsWith("FR", StringComparison.InvariantCultureIgnoreCase))
                    result.Add(Data.Language.FRENCH);
                else if (word.StartsWith("German", StringComparison.InvariantCultureIgnoreCase) || word.StartsWith("DE", StringComparison.InvariantCultureIgnoreCase))
                    result.Add(Data.Language.GERMAN);
                else if (word.StartsWith("Spanish", StringComparison.InvariantCultureIgnoreCase) || word.StartsWith("ES", StringComparison.InvariantCultureIgnoreCase))
                    result.Add(Data.Language.SPANISH);
                else if (word.Equals("Arabic", StringComparison.InvariantCultureIgnoreCase))
                    result.Add(Data.Language.ARABIC);
            }
            return result.ToArray();
        }

        static Data.Status parse_status(string text) {
            text = text.Replace(" ", "");
            if (string.IsNullOrWhiteSpace(text))
                return Data.Status.None;
            text = text.Split(new char[] { '/', '-' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).First();
            if (text.Equals(nameof(Data.Status.ToBeContacted), StringComparison.InvariantCultureIgnoreCase))
                return Data.Status.ToBeContacted;
            if (text.Equals(nameof(Data.Status.Texted), StringComparison.InvariantCultureIgnoreCase))
                return Data.Status.Texted;
            if (text.Equals(nameof(Data.Status.SpokenOnPhone), StringComparison.InvariantCultureIgnoreCase))
                return Data.Status.SpokenOnPhone;
            if (text.Equals(nameof(Data.Status.InProgress), StringComparison.InvariantCultureIgnoreCase))
                return Data.Status.InProgress;
            if (text.Equals(nameof(Data.Status.Matched), StringComparison.InvariantCultureIgnoreCase))
                return Data.Status.Matched;
            if (text.Equals(nameof(Data.Status.PotentialMatch), StringComparison.InvariantCultureIgnoreCase))
                return Data.Status.PotentialMatch;
            if (text.Equals(nameof(Data.Status.BeingMatched), StringComparison.InvariantCultureIgnoreCase))
                return Data.Status.BeingMatched;
            if (text.Equals(nameof(Data.Status.AppliedForVisa), StringComparison.InvariantCultureIgnoreCase))
                return Data.Status.AppliedForVisa;
            if (text.Equals(nameof(Data.Status.TravelSupport), StringComparison.InvariantCultureIgnoreCase))
                return Data.Status.TravelSupport;
            if (text.Equals(nameof(Data.Status.Closed), StringComparison.InvariantCultureIgnoreCase))
                return Data.Status.Closed;
            return Data.Status.None;
        }

        static string[] split_list(string txt) {
            if (string.IsNullOrWhiteSpace(txt))
                return Array.Empty<string>();
            return txt.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        static int[] parse_ints(string[] vals) {
            var result = new List<int>();
            foreach (var val in vals)
                if (int.TryParse(val, out var n))
                    result.Add(n);
            return result.ToArray();
        }
        static bool parse_yes_no(string txt) {
            return txt != null && txt.StartsWith("y", StringComparison.InvariantCultureIgnoreCase);
        }
        static bool? parse_yes_no_empty(string txt) {
            if (string.IsNullOrWhiteSpace(txt))
                return null;
            return txt.StartsWith("y", StringComparison.InvariantCultureIgnoreCase);
        }

        Dictionary<string, int> headers_ = new();
    }
}
