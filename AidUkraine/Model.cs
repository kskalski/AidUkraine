namespace AidUkraine.Data {
    public enum Priority {
        None = 0,
        Low = 1,
        Medium,
        High,
        Urgent
    }
    public enum Status {
        None = 0,
        ToBeContacted = 1,
        Texted,
        SpokenOnPhone,
        InProgress,
        BeingMatched,
        PotentialMatch,
        Matched,
        AppliedForVisa,
        TravelSupport,
        Closed
    }

    public enum Language {
        ENGLISH = 0,
        UKRAINIAN = 1,
        RUSSIAN,
        ARABIC,
        POLISH,
        SPANISH,
        FRENCH,
        GERMAN
    }

    public interface IHasStatus {
        public Status Status { get; set; }
    }

    public class Case: IHasStatus {
        public int OriginIndex { get; set; }
        public string Caseid { get; set; }
        public string Name { get; set; }
        public string SupportPerson { get; set; }
        public Priority CurrentPriority { get; set; }
        public Status Status { get; set; }
        public string OutstandingActions { get; set; }
        public string Description { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string FacebookLink { get; set; }
        public Language[] LanguagesSpoken { get; set; }
        public int NumAdults { get; set; }
        public int NumChildren { get; set; }
        public int TotalNumberOfPeople { get; set; }
        public int NumPeopleTotal => TotalNumberOfPeople > 0 ? TotalNumberOfPeople : (NumAdults + NumChildren);
        public int[] ChildrenAges { get; set; }
        public bool HasPets { get; set; }
        public string PetTypes { get; set; }
        public bool HasSpecialNeeds { get; set; }
        public string CurrentLocation { get; set; }

        public string WantedDestination { get; set; }

        public string HostFamily { get; set; }
    }

    public class Host: IHasStatus {
        public int OriginIndex { get; set; }
        public string HostId { get; set; }
        public string HostName { get; set; }
        public string PrimaryContact { get; set; }
        public Status Status { get; set; }
        public string OutstandingActions { get; set; }
        public string Notes { get; set; }
        public string DescriptionOfOffer { get; set; }
        public Language[] LanguagesSpoken { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string FacebookLink { get; set; }
        public string Location { get; set; }
        public bool WillHostChildren { get; set; }
        public string WillHostChildrenKinds { get; set; }
        public bool WillHostPets { get; set; }
        public string WillHostPetsKinds { get; set; }
        public bool WillHostSpecialNeeds { get; set; }
        public string RoomsAvailable { get; set; }
        public bool RegisteredWithGov { get; set; }
        public int? MaxNumPeople { get; set; }
    }
}
