namespace MovieReviews.Data;

public enum PerformerAgeCategory
{
    YoungAdult,
    Adult,
    Milf,
    Mature,
    Senior
}

public static class PerformerAgeCategories
{
    public static string Label(PerformerAgeCategory category) => category switch
    {
        PerformerAgeCategory.YoungAdult => "Young adult (18+)",
        PerformerAgeCategory.Adult => "Adult",
        PerformerAgeCategory.Milf => "MILF",
        PerformerAgeCategory.Mature => "Mature",
        PerformerAgeCategory.Senior => "Senior",
        _ => category.ToString()
    };
}
