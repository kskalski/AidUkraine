namespace AidUkraine {
    internal class Matcher {

        public static IEnumerable<T> FilterStatus<T>(IEnumerable<T> input) where T: Data.IHasStatus {
            return input.Where(x => {
                var status = x.Status;
                return status != Data.Status.Matched && status != Data.Status.PotentialMatch && status != Data.Status.BeingMatched && status != Data.Status.TravelSupport &&
                       status != Data.Status.Closed && status != Data.Status.AppliedForVisa;
            });
        }

        public static bool IsGoodMatch(Data.Case c, Data.Host h) {
            if (c.NumChildren > 0 && !h.WillHostChildren)
                return false;
            if (c.HasPets && !h.WillHostPets)
                return false;
            //if (h.LanguagesSpoken.Any(l => SLAVIC_LANGS.Contains(l)) && c.LanguagesSpoken.Contains(Data.Language.ENGLISH))
            //    return false;
            if (c.Smoker != null && h.SmokerInHouse != null)
                return c.Smoker == h.SmokerInHouse;
            if (h.MaxNumPeople != null && h.MaxNumPeople < c.NumPeopleTotal)
                return false;
            return true;
        }

        internal static int[][] BuildGraph(IReadOnlyList<Data.Case> cases, IReadOnlyList<Data.Host> hosts) {
            var graph = new int[cases.Count][];
            for (int ci = 0; ci < cases.Count; ci++) {
                var c = cases[ci];
                var matched_his = new List<int>();
                for (int hi = 0; hi < hosts.Count; hi++) {
                    if (IsGoodMatch(c, hosts[hi]))
                        matched_his.Add(hi);
                }
                graph[ci] = matched_his.ToArray();
            }
            return graph;
        }

        internal static List<int>[] GenerateMatches(int[][] graph, int num_hosts) {
            bool[] taken = new bool[num_hosts];
            var all_matches = new List<int>[graph.Length];

            do {
                var (new_matches, num_new_matches) = maxBPM(graph, taken);
                if (num_new_matches == 0)
                    break;

                for (int hi = 0; hi < new_matches.Length; ++hi) {
                    var ci = new_matches[hi];
                    if (ci < 0)
                        continue;
                    taken[hi] = true;
                    if (all_matches[ci] == null)
                        all_matches[ci] = new();
                    all_matches[ci].Add(hi);
                }
                Console.WriteLine("Graph {0}, matches {1}", graph.Length, num_new_matches);
            } while (true);

            return all_matches;
        }

        // A DFS based recursive function
        // that returns true if a matching
        // for vertex u is possible
        static bool bpm(int[][] bpGraph, int u, bool[] seen, int[] matchR) {
            // Try every job one by one
            var adjacent = bpGraph[u];
            for (int vi = 0; vi < adjacent.Length; vi++) {
                int v = adjacent[vi];
                // If applicant u is interested
                // in job v and v is not visited
                if (!seen[v]) {
                    // Mark v as visited
                    seen[v] = true;

                    // If job 'v' is not assigned to
                    // an applicant OR previously assigned
                    // applicant for job v (which is matchR[v])
                    // has an alternate job available.
                    // Since v is marked as visited in the above
                    // line, matchR[v] in the following recursive
                    // call will not get job 'v' again
                    if (matchR[v] < 0 || bpm(bpGraph, matchR[v],
                                             seen, matchR)) {
                        matchR[v] = u;
                        return true;
                    }
                }
            }
            return false;
        }

        // Returns maximum number of
        // matching from M to N
        static Tuple<int[], int> maxBPM(int[][] bpGraph, bool[] ignore_jobs) {
            // An array to keep track of the
            // applicants assigned to jobs.
            // The value of matchR[i] is the
            // applicant number assigned to job i,
            // the value -1 indicates nobody is assigned.
            int N = ignore_jobs.Length;
            int[] matchR = new int[N];

            // Initially all jobs are available
            for (int i = 0; i < N; ++i)
                matchR[i] = -1;

            // Count of jobs assigned to applicants
            int result = 0;
            for (int u = 0; u < bpGraph.Length; u++) {
                // Mark all jobs as not
                // seen for next applicant.
                bool[] seen = (bool[]) ignore_jobs.Clone();

                // Find if the applicant
                // 'u' can get a job
                if (bpm(bpGraph, u, seen, matchR))
                    result++;
            }
            return Tuple.Create(matchR, result);
        }

        static Data.Language[] SLAVIC_LANGS = { Data.Language.UKRAINIAN, Data.Language.POLISH, Data.Language.RUSSIAN };
    }
}
