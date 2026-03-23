using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.AI.Application.Rules;

internal static class BreedRiskData
{
    // Breeds at risk of cardiac conditions (DCM, MVD, etc.)
    public static readonly IReadOnlyDictionary<Species, IReadOnlyList<string>> CardiacRiskBreeds =
        new Dictionary<Species, IReadOnlyList<string>>
        {
            [Species.Dog] = new[]
            {
                "Cavalier King Charles Spaniel", "Doberman", "Boxer", "Great Dane",
                "Irish Wolfhound", "Newfoundland", "Golden Retriever"
            },
            [Species.Cat] = new[]
            {
                "Maine Coon", "Ragdoll", "British Shorthair", "Sphynx", "Persian"
            }
        };

    // Breeds at risk of hip dysplasia
    public static readonly IReadOnlyList<string> HipDysplasiaBreeds = new[]
    {
        "German Shepherd", "Labrador Retriever", "Golden Retriever", "Rottweiler",
        "Bernese Mountain Dog", "Saint Bernard", "Bulldog", "Great Dane",
        "Mastiff", "Newfoundland"
    };

    // Brachycephalic breeds (airway syndrome risk)
    public static readonly IReadOnlyDictionary<Species, IReadOnlyList<string>> BrachycephalicBreeds =
        new Dictionary<Species, IReadOnlyList<string>>
        {
            [Species.Dog] = new[]
            {
                "Bulldog", "French Bulldog", "Pug", "Boston Terrier",
                "Pekingese", "Shih Tzu", "Cavalier King Charles Spaniel",
                "Boxer", "English Bulldog"
            },
            [Species.Cat] = new[]
            {
                "Persian", "Himalayan", "Exotic Shorthair", "British Shorthair",
                "Scottish Fold"
            }
        };

    // Breeds at risk of diabetes
    public static readonly IReadOnlyDictionary<Species, IReadOnlyList<string>> DiabetesRiskBreeds =
        new Dictionary<Species, IReadOnlyList<string>>
        {
            [Species.Dog] = new[]
            {
                "Samoyed", "Australian Terrier", "Miniature Schnauzer",
                "Miniature Poodle", "Pug", "Bichon Frise"
            },
            [Species.Cat] = new[]
            {
                "Burmese", "Siamese", "Norwegian Forest Cat", "Russian Blue"
            }
        };

    // Senior age thresholds in years by species (AAHA guidelines)
    public static readonly IReadOnlyDictionary<Species, int> SeniorAgeThresholdYears =
        new Dictionary<Species, int>
        {
            [Species.Dog] = 7,
            [Species.Cat] = 10,
            [Species.Horse] = 20,
            [Species.Rabbit] = 5,
            [Species.Bird] = 8,
            [Species.Exotic] = 5,
            [Species.Camel] = 15
        };

    public static bool IsBreedInList(string breed, IReadOnlyList<string> breedList)
        => breedList.Any(b => breed.Contains(b, StringComparison.OrdinalIgnoreCase));

    public static bool IsBreedInSpeciesMap(Species species, string breed, IReadOnlyDictionary<Species, IReadOnlyList<string>> map)
        => map.TryGetValue(species, out var breeds) && IsBreedInList(breed, breeds);
}
