using Microsoft.EntityFrameworkCore;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Infrastructure;

/// <summary>
/// Seeds the global drug catalog with common veterinary medications, vaccines, interactions,
/// contraindications, and dosage guidelines. All entries have ClinicId = null (global).
/// INN names are used (public domain). Descriptions are original.
///
/// Loading mechanism: runtime seed called once at startup via DbInitializer.SeedDrugCatalogAsync.
/// This is NOT an EF Core HasData seed — it uses a manual idempotency guard (AnyAsync on ClinicId == null)
/// so the data is inserted only once per database lifecycle.
///
/// IgnoreQueryFilters is intentional here: global catalog entries have ClinicId = null, which
/// falls outside the tenant filter (WHERE ClinicId = @current). Without IgnoreQueryFilters the
/// AnyAsync check would always return false and re-insert every restart.
///
/// Status (2026-03-10): SeedDrugCatalogAsync is defined in DbInitializer but NOT wired in Program.cs.
/// The drug catalog is therefore not seeded automatically on startup. Wire it if/when needed by
/// adding `await DbInitializer.SeedDrugCatalogAsync(app.Services);` after MigrateAllAsync in Program.cs.
/// </summary>
internal static class DrugCatalogSeedData
{
    public static async Task SeedAsync(MedicalRecordsDbContext context, CancellationToken cancellationToken = default)
    {
        // Check if already seeded — use IgnoreQueryFilters because ClinicId = null is not covered by the tenant filter
        var alreadySeeded = await context.DrugCatalogEntries
            .IgnoreQueryFilters()
            .AnyAsync(d => d.ClinicId == null, cancellationToken);

        if (alreadySeeded)
            return;

        var entries = BuildAllEntries();

        context.DrugCatalogEntries.AddRange(entries);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static List<DrugCatalogEntry> BuildAllEntries()
    {
        var medications = BuildMedications();
        var vaccines = BuildVaccines();
        var supplements = BuildSupplements();

        AddInteractions(medications);
        AddContraindications(medications);
        AddDosageGuidelines(medications);

        return [.. medications, .. vaccines, .. supplements];
    }

    // ─── ANTIBIOTICS ───────────────────────────────────────────────────────────

    private static List<DrugCatalogEntry> BuildMedications()
    {
        var list = new List<DrugCatalogEntry>();

        // Antibiotics
        list.AddRange([
            Make("amoxicillin", "Amoxicillin", DrugCategory.Medication),
            Make("amoxicillin-clavulanate", "Amoxicillin / Clavulanate", DrugCategory.Medication),
            Make("ampicillin", "Ampicillin", DrugCategory.Medication),
            Make("cephalexin", "Cephalexin", DrugCategory.Medication),
            Make("cefazolin", "Cefazolin", DrugCategory.Medication),
            Make("cefpodoxime-proxetil", "Cefpodoxime Proxetil", DrugCategory.Medication),
            Make("cefovecin", "Cefovecin", DrugCategory.Medication),
            Make("enrofloxacin", "Enrofloxacin", DrugCategory.Medication),
            Make("marbofloxacin", "Marbofloxacin", DrugCategory.Medication),
            Make("pradofloxacin", "Pradofloxacin", DrugCategory.Medication),
            Make("doxycycline", "Doxycycline", DrugCategory.Medication),
            Make("tetracycline", "Tetracycline", DrugCategory.Medication),
            Make("oxytetracycline", "Oxytetracycline", DrugCategory.Medication),
            Make("metronidazole", "Metronidazole", DrugCategory.Medication),
            Make("clindamycin", "Clindamycin", DrugCategory.Medication),
            Make("lincomycin", "Lincomycin", DrugCategory.Medication),
            Make("erythromycin", "Erythromycin", DrugCategory.Medication),
            Make("azithromycin", "Azithromycin", DrugCategory.Medication),
            Make("chloramphenicol", "Chloramphenicol", DrugCategory.Medication),
            Make("trimethoprim-sulfamethoxazole", "Trimethoprim / Sulfamethoxazole", DrugCategory.Medication),
            Make("gentamicin", "Gentamicin", DrugCategory.Medication),
            Make("tobramycin", "Tobramycin", DrugCategory.Medication),
            Make("amikacin", "Amikacin", DrugCategory.Medication),
            Make("neomycin", "Neomycin", DrugCategory.Medication),
            Make("rifampin", "Rifampin", DrugCategory.Medication),
            Make("tylosin", "Tylosin", DrugCategory.Medication),
            Make("tilmicosin", "Tilmicosin", DrugCategory.Medication),
        ]);

        // NSAIDs & Analgesics
        list.AddRange([
            Make("meloxicam", "Meloxicam", DrugCategory.Medication),
            Make("carprofen", "Carprofen", DrugCategory.Medication),
            Make("deracoxib", "Deracoxib", DrugCategory.Medication),
            Make("grapiprant", "Grapiprant", DrugCategory.Medication),
            Make("robenacoxib", "Robenacoxib", DrugCategory.Medication),
            Make("ketoprofen", "Ketoprofen", DrugCategory.Medication),
            Make("ibuprofen", "Ibuprofen", DrugCategory.Medication),
            Make("naproxen", "Naproxen", DrugCategory.Medication),
            Make("acetylsalicylic-acid", "Acetylsalicylic Acid (Aspirin)", DrugCategory.Medication),
            Make("acetaminophen", "Acetaminophen (Paracetamol)", DrugCategory.Medication),
            Make("tramadol", "Tramadol", DrugCategory.Medication),
            Make("buprenorphine", "Buprenorphine", DrugCategory.Medication),
            Make("butorphanol", "Butorphanol", DrugCategory.Medication),
            Make("morphine", "Morphine", DrugCategory.Medication),
            Make("fentanyl", "Fentanyl", DrugCategory.Medication),
            Make("gabapentin", "Gabapentin", DrugCategory.Medication),
            Make("pregabalin", "Pregabalin", DrugCategory.Medication),
        ]);

        // Antiparasitics
        list.AddRange([
            Make("ivermectin", "Ivermectin", DrugCategory.Medication),
            Make("selamectin", "Selamectin", DrugCategory.Medication),
            Make("milbemycin-oxime", "Milbemycin Oxime", DrugCategory.Medication),
            Make("moxidectin", "Moxidectin", DrugCategory.Medication),
            Make("doramectin", "Doramectin", DrugCategory.Medication),
            Make("pyrantel", "Pyrantel", DrugCategory.Medication),
            Make("fenbendazole", "Fenbendazole", DrugCategory.Medication),
            Make("mebendazole", "Mebendazole", DrugCategory.Medication),
            Make("albendazole", "Albendazole", DrugCategory.Medication),
            Make("praziquantel", "Praziquantel", DrugCategory.Medication),
            Make("epsiprantel", "Epsiprantel", DrugCategory.Medication),
            Make("permethrin", "Permethrin", DrugCategory.Medication),
            Make("fipronil", "Fipronil", DrugCategory.Medication),
            Make("imidacloprid", "Imidacloprid", DrugCategory.Medication),
            Make("afoxolaner", "Afoxolaner", DrugCategory.Medication),
            Make("fluralaner", "Fluralaner", DrugCategory.Medication),
            Make("sarolaner", "Sarolaner", DrugCategory.Medication),
            Make("lotilaner", "Lotilaner", DrugCategory.Medication),
            Make("spinosad", "Spinosad", DrugCategory.Medication),
            Make("nitenpyram", "Nitenpyram", DrugCategory.Medication),
            Make("metaflumizone", "Metaflumizone", DrugCategory.Medication),
        ]);

        // Cardiac & Respiratory
        list.AddRange([
            Make("atenolol", "Atenolol", DrugCategory.Medication),
            Make("sotalol", "Sotalol", DrugCategory.Medication),
            Make("diltiazem", "Diltiazem", DrugCategory.Medication),
            Make("amlodipine", "Amlodipine", DrugCategory.Medication),
            Make("digoxin", "Digoxin", DrugCategory.Medication),
            Make("furosemide", "Furosemide", DrugCategory.Medication),
            Make("spironolactone", "Spironolactone", DrugCategory.Medication),
            Make("benazepril", "Benazepril", DrugCategory.Medication),
            Make("enalapril", "Enalapril", DrugCategory.Medication),
            Make("pimobendan", "Pimobendan", DrugCategory.Medication),
            Make("sildenafil", "Sildenafil", DrugCategory.Medication),
            Make("torsemide", "Torsemide", DrugCategory.Medication),
            Make("hydrochlorothiazide", "Hydrochlorothiazide", DrugCategory.Medication),
            Make("theophylline", "Theophylline", DrugCategory.Medication),
            Make("terbutaline", "Terbutaline", DrugCategory.Medication),
            Make("aminophylline", "Aminophylline", DrugCategory.Medication),
        ]);

        // Anesthetics & Sedatives
        list.AddRange([
            Make("propofol", "Propofol", DrugCategory.Medication),
            Make("ketamine", "Ketamine", DrugCategory.Medication),
            Make("thiopental", "Thiopental", DrugCategory.Medication),
            Make("alfaxalone", "Alfaxalone", DrugCategory.Medication),
            Make("medetomidine", "Medetomidine", DrugCategory.Medication),
            Make("dexmedetomidine", "Dexmedetomidine", DrugCategory.Medication),
            Make("xylazine", "Xylazine", DrugCategory.Medication),
            Make("acepromazine", "Acepromazine", DrugCategory.Medication),
            Make("diazepam", "Diazepam", DrugCategory.Medication),
            Make("midazolam", "Midazolam", DrugCategory.Medication),
            Make("tiletamine-zolazepam", "Tiletamine / Zolazepam", DrugCategory.Medication),
            Make("isoflurane", "Isoflurane", DrugCategory.Medication),
            Make("sevoflurane", "Sevoflurane", DrugCategory.Medication),
            Make("lidocaine", "Lidocaine", DrugCategory.Medication),
            Make("bupivacaine", "Bupivacaine", DrugCategory.Medication),
            Make("atropine", "Atropine", DrugCategory.Medication),
            Make("glycopyrrolate", "Glycopyrrolate", DrugCategory.Medication),
        ]);

        // Steroids & Immunosuppressants
        list.AddRange([
            Make("prednisolone", "Prednisolone", DrugCategory.Medication),
            Make("prednisone", "Prednisone", DrugCategory.Medication),
            Make("dexamethasone", "Dexamethasone", DrugCategory.Medication),
            Make("methylprednisolone", "Methylprednisolone", DrugCategory.Medication),
            Make("triamcinolone", "Triamcinolone", DrugCategory.Medication),
            Make("cyclosporine", "Cyclosporine", DrugCategory.Medication),
            Make("azathioprine", "Azathioprine", DrugCategory.Medication),
            Make("mycophenolate-mofetil", "Mycophenolate Mofetil", DrugCategory.Medication),
            Make("chlorambucil", "Chlorambucil", DrugCategory.Medication),
            Make("vincristine", "Vincristine", DrugCategory.Medication),
            Make("doxorubicin", "Doxorubicin", DrugCategory.Medication),
            Make("cyclophosphamide", "Cyclophosphamide", DrugCategory.Medication),
        ]);

        // GI & Antiemetics
        list.AddRange([
            Make("metoclopramide", "Metoclopramide", DrugCategory.Medication),
            Make("maropitant", "Maropitant", DrugCategory.Medication),
            Make("ondansetron", "Ondansetron", DrugCategory.Medication),
            Make("famotidine", "Famotidine", DrugCategory.Medication),
            Make("ranitidine", "Ranitidine", DrugCategory.Medication),
            Make("omeprazole", "Omeprazole", DrugCategory.Medication),
            Make("pantoprazole", "Pantoprazole", DrugCategory.Medication),
            Make("sucralfate", "Sucralfate", DrugCategory.Medication),
            Make("misoprostol", "Misoprostol", DrugCategory.Medication),
            Make("lactulose", "Lactulose", DrugCategory.Medication),
            Make("bisacodyl", "Bisacodyl", DrugCategory.Medication),
        ]);

        // Endocrine
        list.AddRange([
            Make("insulin-glargine", "Insulin Glargine", DrugCategory.Medication),
            Make("insulin-lente", "Insulin Lente", DrugCategory.Medication),
            Make("methimazole", "Methimazole", DrugCategory.Medication),
            Make("carbimazole", "Carbimazole", DrugCategory.Medication),
            Make("levothyroxine", "Levothyroxine", DrugCategory.Medication),
            Make("trilostane", "Trilostane", DrugCategory.Medication),
            Make("mitotane", "Mitotane", DrugCategory.Medication),
            Make("megestrol-acetate", "Megestrol Acetate", DrugCategory.Medication),
            Make("deslorelin", "Deslorelin", DrugCategory.Medication),
        ]);

        // Neurological & Behavioral
        list.AddRange([
            Make("phenobarbital", "Phenobarbital", DrugCategory.Medication),
            Make("potassium-bromide", "Potassium Bromide", DrugCategory.Medication),
            Make("levetiracetam", "Levetiracetam", DrugCategory.Medication),
            Make("zonisamide", "Zonisamide", DrugCategory.Medication),
            Make("imepitoin", "Imepitoin", DrugCategory.Medication),
            Make("fluoxetine", "Fluoxetine", DrugCategory.Medication),
            Make("clomipramine", "Clomipramine", DrugCategory.Medication),
            Make("sertraline", "Sertraline", DrugCategory.Medication),
            Make("buspirone", "Buspirone", DrugCategory.Medication),
            Make("trazodone", "Trazodone", DrugCategory.Medication),
            Make("alprazolam", "Alprazolam", DrugCategory.Medication),
        ]);

        // Ophthalmic & Dermatology
        list.AddRange([
            Make("atropine-ophthalmic", "Atropine Ophthalmic", DrugCategory.Medication),
            Make("dorzolamide", "Dorzolamide", DrugCategory.Medication),
            Make("latanoprost", "Latanoprost", DrugCategory.Medication),
            Make("ciprofloxacin-ophthalmic", "Ciprofloxacin Ophthalmic", DrugCategory.Medication),
            Make("oxytetracycline-ophthalmic", "Oxytetracycline Ophthalmic", DrugCategory.Medication),
            Make("oclacitinib", "Oclacitinib", DrugCategory.Medication),
            Make("lokivetmab", "Lokivetmab", DrugCategory.Medication),
        ]);

        // Miscellaneous
        list.AddRange([
            Make("vitamin-k1", "Vitamin K1 (Phytomenadione)", DrugCategory.Medication),
            Make("n-acetylcysteine", "N-Acetylcysteine", DrugCategory.Medication),
            Make("activated-charcoal", "Activated Charcoal", DrugCategory.Medication),
            Make("atipamezole", "Atipamezole", DrugCategory.Medication),
            Make("naloxone", "Naloxone", DrugCategory.Medication),
            Make("neostigmine", "Neostigmine", DrugCategory.Medication),
            Make("calcium-gluconate", "Calcium Gluconate", DrugCategory.Medication),
            Make("dextrose", "Dextrose", DrugCategory.Medication),
            Make("sodium-bicarbonate", "Sodium Bicarbonate", DrugCategory.Medication),
            Make("potassium-chloride", "Potassium Chloride", DrugCategory.Medication),
            Make("lactated-ringers", "Lactated Ringer's Solution", DrugCategory.Medication),
            Make("normal-saline", "Normal Saline (0.9% NaCl)", DrugCategory.Medication),
            Make("hetastarch", "Hetastarch", DrugCategory.Medication),
            Make("mannitol", "Mannitol", DrugCategory.Medication),
            Make("diphenhydramine", "Diphenhydramine", DrugCategory.Medication),
            Make("hydroxyzine", "Hydroxyzine", DrugCategory.Medication),
            Make("cetirizine", "Cetirizine", DrugCategory.Medication),
            Make("loratadine", "Loratadine", DrugCategory.Medication),
            Make("epinephrine", "Epinephrine (Adrenaline)", DrugCategory.Medication),
            Make("desmopressin", "Desmopressin", DrugCategory.Medication),
            Make("misoprostol-2", "Misoprostol (Reproductive)", DrugCategory.Medication),
            Make("oxytocin", "Oxytocin", DrugCategory.Medication),
            Make("prostaglandin-f2alpha", "Prostaglandin F2-alpha (Dinoprost)", DrugCategory.Medication),
            Make("testosterone", "Testosterone", DrugCategory.Medication),
            Make("progesterone", "Progesterone", DrugCategory.Medication),
            Make("stanozolol", "Stanozolol", DrugCategory.Medication),
            Make("yohimbine", "Yohimbine", DrugCategory.Medication),
            Make("flumazenil", "Flumazenil", DrugCategory.Medication),
            Make("terbinafine", "Terbinafine", DrugCategory.Medication),
            Make("itraconazole", "Itraconazole", DrugCategory.Medication),
            Make("fluconazole", "Fluconazole", DrugCategory.Medication),
            Make("ketoconazole", "Ketoconazole", DrugCategory.Medication),
            Make("nystatin", "Nystatin", DrugCategory.Medication),
            Make("griseofulvin", "Griseofulvin", DrugCategory.Medication),
            Make("acyclovir", "Acyclovir", DrugCategory.Medication),
            Make("famciclovir", "Famciclovir", DrugCategory.Medication),
            Make("interferon-omega", "Feline Interferon Omega", DrugCategory.Medication),
        ]);

        return list;
    }

    private static List<DrugCatalogEntry> BuildVaccines()
    {
        var list = new List<DrugCatalogEntry>();

        // Dog vaccines (core)
        list.AddRange([
            Make("vaccine-dog-distemper", "Canine Distemper Vaccine (CDV)", DrugCategory.Vaccine),
            Make("vaccine-dog-parvovirus", "Canine Parvovirus Vaccine (CPV)", DrugCategory.Vaccine),
            Make("vaccine-dog-adenovirus", "Canine Adenovirus Vaccine (CAV-2)", DrugCategory.Vaccine),
            Make("vaccine-dog-dap", "Canine DAP (Distemper-Adenovirus-Parvovirus) Vaccine", DrugCategory.Vaccine),
            Make("vaccine-dog-rabies", "Canine Rabies Vaccine", DrugCategory.Vaccine),
        ]);

        // Dog vaccines (non-core)
        list.AddRange([
            Make("vaccine-dog-bordetella", "Canine Bordetella bronchiseptica Vaccine", DrugCategory.Vaccine),
            Make("vaccine-dog-leptospira", "Canine Leptospira Vaccine (L4)", DrugCategory.Vaccine),
            Make("vaccine-dog-borrelia", "Canine Borrelia burgdorferi (Lyme) Vaccine", DrugCategory.Vaccine),
            Make("vaccine-dog-coronavirus", "Canine Coronavirus Vaccine (CCoV)", DrugCategory.Vaccine),
            Make("vaccine-dog-influenza-h3n8", "Canine Influenza Vaccine H3N8", DrugCategory.Vaccine),
            Make("vaccine-dog-influenza-h3n2", "Canine Influenza Vaccine H3N2", DrugCategory.Vaccine),
            Make("vaccine-dog-leishmania", "Canine Leishmania Vaccine", DrugCategory.Vaccine),
            Make("vaccine-dog-rattlesnake", "Canine Rattlesnake Vaccine", DrugCategory.Vaccine),
        ]);

        // Cat vaccines (core)
        list.AddRange([
            Make("vaccine-cat-panleukopenia", "Feline Panleukopenia Vaccine (FPV)", DrugCategory.Vaccine),
            Make("vaccine-cat-herpesvirus", "Feline Herpesvirus-1 Vaccine (FHV-1)", DrugCategory.Vaccine),
            Make("vaccine-cat-calicivirus", "Feline Calicivirus Vaccine (FCV)", DrugCategory.Vaccine),
            Make("vaccine-cat-fvrcp", "Feline FVRCP Combo Vaccine", DrugCategory.Vaccine),
            Make("vaccine-cat-rabies", "Feline Rabies Vaccine", DrugCategory.Vaccine),
        ]);

        // Cat vaccines (non-core)
        list.AddRange([
            Make("vaccine-cat-felv", "Feline Leukemia Virus Vaccine (FeLV)", DrugCategory.Vaccine),
            Make("vaccine-cat-fiv", "Feline Immunodeficiency Virus Vaccine (FIV)", DrugCategory.Vaccine),
            Make("vaccine-cat-chlamydia", "Feline Chlamydophila felis Vaccine", DrugCategory.Vaccine),
            Make("vaccine-cat-bordetella", "Feline Bordetella Vaccine", DrugCategory.Vaccine),
            Make("vaccine-cat-peritonitis", "Feline Infectious Peritonitis Vaccine (FIP)", DrugCategory.Vaccine),
        ]);

        // Horse vaccines
        list.AddRange([
            Make("vaccine-horse-tetanus", "Equine Tetanus Toxoid", DrugCategory.Vaccine),
            Make("vaccine-horse-influenza", "Equine Influenza Vaccine", DrugCategory.Vaccine),
            Make("vaccine-horse-herpesvirus", "Equine Herpesvirus (EHV-1/4) Vaccine", DrugCategory.Vaccine),
            Make("vaccine-horse-encephalitis", "Equine Encephalitis (EEE/WEE/VEE) Vaccine", DrugCategory.Vaccine),
            Make("vaccine-horse-rabies", "Equine Rabies Vaccine", DrugCategory.Vaccine),
            Make("vaccine-horse-strangles", "Equine Strangles (Streptococcus equi) Vaccine", DrugCategory.Vaccine),
            Make("vaccine-horse-wnv", "Equine West Nile Virus Vaccine", DrugCategory.Vaccine),
            Make("vaccine-horse-rotavirus", "Equine Rotavirus Vaccine", DrugCategory.Vaccine),
            Make("vaccine-horse-botulism", "Equine Botulism Vaccine", DrugCategory.Vaccine),
            Make("vaccine-horse-rhinopneumonitis", "Equine Rhinopneumonitis Vaccine", DrugCategory.Vaccine),
        ]);

        // Camel vaccines
        list.AddRange([
            Make("vaccine-camel-mers", "Camel MERS-CoV Experimental Vaccine", DrugCategory.Vaccine),
            Make("vaccine-camel-camelpox", "Camelpox Vaccine", DrugCategory.Vaccine),
            Make("vaccine-camel-rift-valley", "Rift Valley Fever Vaccine", DrugCategory.Vaccine),
            Make("vaccine-camel-foot-mouth", "Foot-and-Mouth Disease Vaccine", DrugCategory.Vaccine),
            Make("vaccine-camel-rabies", "Camel Rabies Vaccine", DrugCategory.Vaccine),
            Make("vaccine-camel-clostridial", "Camel Clostridial Diseases Vaccine", DrugCategory.Vaccine),
            Make("vaccine-camel-hemorrhagic-fever", "Camel Hemorrhagic Fever Vaccine", DrugCategory.Vaccine),
        ]);

        return list;
    }

    private static List<DrugCatalogEntry> BuildSupplements()
    {
        return [
            Make("omega-3-fatty-acids", "Omega-3 Fatty Acids (Fish Oil)", DrugCategory.Supplement),
            Make("glucosamine", "Glucosamine", DrugCategory.Supplement),
            Make("chondroitin-sulfate", "Chondroitin Sulfate", DrugCategory.Supplement),
            Make("vitamin-e", "Vitamin E (Alpha-Tocopherol)", DrugCategory.Supplement),
            Make("vitamin-c", "Vitamin C (Ascorbic Acid)", DrugCategory.Supplement),
            Make("coenzyme-q10", "Coenzyme Q10", DrugCategory.Supplement),
            Make("probiotics-canine", "Probiotics (Canine Formula)", DrugCategory.Supplement),
            Make("probiotics-feline", "Probiotics (Feline Formula)", DrugCategory.Supplement),
            Make("milk-thistle", "Milk Thistle (Silymarin)", DrugCategory.Supplement),
            Make("s-adenosylmethionine", "S-Adenosylmethionine (SAMe)", DrugCategory.Supplement),
            Make("taurine", "Taurine", DrugCategory.Supplement),
            Make("l-carnitine", "L-Carnitine", DrugCategory.Supplement),
            Make("zinc-supplement", "Zinc Supplement", DrugCategory.Supplement),
            Make("iron-supplement", "Iron Supplement", DrugCategory.Supplement),
            Make("b-complex", "B-Complex Vitamins", DrugCategory.Supplement),
        ];
    }

    // ─── CONTRAINDICATIONS ──────────────────────────────────────────────────────

    private static void AddContraindications(List<DrugCatalogEntry> list)
    {
        var lookup = list.ToDictionary(d => d.InnName);

        // Critical: NSAID contraindications
        Get(lookup, "ibuprofen")?.AddContraindication(Species.Cat, InteractionSeverity.Critical,
            "Ibuprofen is highly toxic to cats: causes acute renal failure and GI ulceration. Fatal doses can occur with single tablet.");
        Get(lookup, "ibuprofen")?.AddContraindication(Species.Dog, InteractionSeverity.Moderate,
            "Ibuprofen is not approved for veterinary use in dogs; causes GI ulceration and nephrotoxicity at therapeutic human doses.");

        Get(lookup, "naproxen")?.AddContraindication(Species.Cat, InteractionSeverity.Critical,
            "Naproxen causes severe GI ulceration and renal failure in cats. Even a single human dose can be lethal.");
        Get(lookup, "naproxen")?.AddContraindication(Species.Dog, InteractionSeverity.Critical,
            "Naproxen is highly toxic to dogs; narrow margin of safety with severe GI and renal toxicity.");

        Get(lookup, "acetylsalicylic-acid")?.AddContraindication(Species.Cat, InteractionSeverity.Critical,
            "Cats lack the enzyme to metabolise salicylates. Aspirin has a very long half-life in cats (>37 h), causing salicylate toxicity.");

        // Critical: Paracetamol
        Get(lookup, "acetaminophen")?.AddContraindication(Species.Cat, InteractionSeverity.Critical,
            "Cats lack hepatic glucuronyl transferase. Acetaminophen causes methemoglobinemia and hepatic necrosis. Single human tablet can be fatal.");
        Get(lookup, "acetaminophen")?.AddContraindication(Species.Dog, InteractionSeverity.Moderate,
            "Acetaminophen can cause hepatotoxicity and methemoglobinemia in dogs, especially at high doses. Not recommended.");

        // Critical: Permethrin
        Get(lookup, "permethrin")?.AddContraindication(Species.Cat, InteractionSeverity.Critical,
            "Permethrin causes severe acute neurotoxicity in cats (tremors, seizures, hyperthermia). Many spot-on flea products for dogs contain permethrin. Never use on cats.");

        // Xylazine
        Get(lookup, "xylazine")?.AddContraindication(Species.Cat, InteractionSeverity.Moderate,
            "Cats are extremely sensitive to xylazine; use at 1/10 canine dose. Monitor for bradycardia, respiratory depression, and prolonged sedation.");

        // Metronidazole / cats at high dose
        Get(lookup, "metronidazole")?.AddContraindication(Species.Cat, InteractionSeverity.Moderate,
            "High doses of metronidazole (>62.5 mg/cat/day) cause acute neurotoxicity in cats: ataxia, nystagmus, head tilt, seizures.");

        // Aminoglycosides — renal
        Get(lookup, "gentamicin")?.AddContraindication(Species.Cat, InteractionSeverity.Moderate,
            "Aminoglycosides are nephrotoxic; cats are particularly sensitive. Use only with adequate hydration and renal function monitoring.");
        Get(lookup, "gentamicin")?.AddContraindication(Species.Dog, InteractionSeverity.Moderate,
            "Aminoglycosides are nephrotoxic. Ensure adequate hydration. Monitor BUN/creatinine. Avoid in patients with pre-existing renal disease.");

        // Enrofloxacin / cats — retinal toxicity
        Get(lookup, "enrofloxacin")?.AddContraindication(Species.Cat, InteractionSeverity.Critical,
            "Enrofloxacin at doses >5 mg/kg/day causes irreversible retinal degeneration and blindness in cats. Reduce dose or use alternative fluoroquinolone.");

        // Chloramphenicol / cats — bone marrow
        Get(lookup, "chloramphenicol")?.AddContraindication(Species.Cat, InteractionSeverity.Moderate,
            "Cats metabolise chloramphenicol slowly; accumulation causes bone marrow suppression. Use low doses with monitoring.");

        // Doxycycline / cats — oesophageal stricture
        Get(lookup, "doxycycline")?.AddContraindication(Species.Cat, InteractionSeverity.Moderate,
            "Doxycycline tablets/capsules given without water can lodge and dissolve in the feline oesophagus, causing stricture. Always follow with a water bolus.");

        // Ivermectin / sensitive breeds
        Get(lookup, "ivermectin")?.AddContraindication(Species.Dog, InteractionSeverity.Moderate,
            "Collies and related breeds (MDR1/ABCB1 mutation) are hypersensitive to ivermectin. Neurotoxicity at doses safe for other dogs. Screen for MDR1 before use.");

        // Ketoconazole / cat — hepatotoxicity
        Get(lookup, "ketoconazole")?.AddContraindication(Species.Cat, InteractionSeverity.Moderate,
            "Ketoconazole is hepatotoxic in cats. Prefer itraconazole or fluconazole for systemic fungal infections in cats.");

        // Tilmicosin / horses, goats
        Get(lookup, "tilmicosin")?.AddContraindication(Species.Horse, InteractionSeverity.Critical,
            "Tilmicosin is cardiotoxic in horses and other non-ruminant species. Do not use in horses. Fatal cardiac arrest has been reported.");

        // Griseofulvin / pregnant
        Get(lookup, "griseofulvin")?.AddContraindication(Species.Cat, InteractionSeverity.Critical,
            "Griseofulvin is teratogenic; do not use in pregnant queens. FIV-positive cats have severe idiosyncratic bone marrow suppression.");

        // Albendazole / cats and dogs (bone marrow toxicity)
        Get(lookup, "albendazole")?.AddContraindication(Species.Cat, InteractionSeverity.Moderate,
            "Albendazole causes bone marrow suppression and teratogenicity in cats. Use fenbendazole instead.");
        Get(lookup, "albendazole")?.AddContraindication(Species.Dog, InteractionSeverity.Moderate,
            "Albendazole may cause bone marrow suppression at high doses in dogs. Monitor CBC during treatment.");

        // Fluoroquinolones / young animals — cartilage
        Get(lookup, "enrofloxacin")?.AddContraindication(Species.Dog, InteractionSeverity.Moderate,
            "Fluoroquinolones cause articular cartilage erosion in growing dogs. Avoid in dogs <12 months (large breeds) or <8 months (small breeds).");
    }

    // ─── DRUG-DRUG INTERACTIONS ──────────────────────────────────────────────────

    private static void AddInteractions(List<DrugCatalogEntry> list)
    {
        var lookup = list.ToDictionary(d => d.InnName);

        // NSAID + NSAID
        AddBidirectional(lookup, "meloxicam", "carprofen", InteractionSeverity.Critical,
            "Concurrent use of two NSAIDs markedly increases risk of GI ulceration and renal toxicity. Avoid combination.");
        AddBidirectional(lookup, "meloxicam", "ibuprofen", InteractionSeverity.Critical,
            "Combining NSAIDs increases GI and renal toxicity risk significantly. Avoid combination.");
        AddBidirectional(lookup, "carprofen", "acetylsalicylic-acid", InteractionSeverity.Critical,
            "Aspirin displaces NSAIDs from plasma proteins and increases GI bleeding risk. Avoid combination.");
        AddBidirectional(lookup, "deracoxib", "meloxicam", InteractionSeverity.Critical,
            "Concurrent NSAID use leads to additive toxicity. Contraindicated.");

        // Corticosteroids + NSAIDs
        AddBidirectional(lookup, "prednisolone", "meloxicam", InteractionSeverity.Critical,
            "NSAIDs + corticosteroids dramatically increase risk of GI ulceration and perforation. Contraindicated combination.");
        AddBidirectional(lookup, "prednisolone", "carprofen", InteractionSeverity.Critical,
            "NSAIDs + corticosteroids dramatically increase risk of GI ulceration. Avoid combination.");
        AddBidirectional(lookup, "dexamethasone", "meloxicam", InteractionSeverity.Critical,
            "Concurrent use of NSAID and corticosteroid is contraindicated; risk of life-threatening GI complications.");

        // Aminoglycosides + furosemide
        AddBidirectional(lookup, "gentamicin", "furosemide", InteractionSeverity.Critical,
            "Furosemide potentiates aminoglycoside nephrotoxicity and ototoxicity. Use with caution; increase monitoring.");
        AddBidirectional(lookup, "amikacin", "furosemide", InteractionSeverity.Critical,
            "Furosemide potentiates aminoglycoside nephrotoxicity and ototoxicity.");

        // Aminoglycosides + NSAIDs
        AddBidirectional(lookup, "gentamicin", "meloxicam", InteractionSeverity.Moderate,
            "NSAIDs may reduce renal blood flow and impair gentamicin excretion, increasing nephrotoxicity risk. Monitor renal function.");

        // Metronidazole + phenobarbital
        AddBidirectional(lookup, "metronidazole", "phenobarbital", InteractionSeverity.Moderate,
            "Phenobarbital induces hepatic enzymes and may decrease metronidazole efficacy. Monitor for reduced metronidazole effect.");

        // Metronidazole + amoxicillin
        AddBidirectional(lookup, "metronidazole", "amoxicillin", InteractionSeverity.Moderate,
            "Combination increases risk of GI side effects (nausea, vomiting, anorexia). Commonly used together but monitor GI tolerance.");

        // Digoxin + furosemide
        AddBidirectional(lookup, "digoxin", "furosemide", InteractionSeverity.Moderate,
            "Furosemide-induced hypokalemia increases risk of digoxin toxicity. Monitor electrolytes and digoxin levels closely.");

        // Digoxin + diltiazem
        AddBidirectional(lookup, "digoxin", "diltiazem", InteractionSeverity.Moderate,
            "Diltiazem increases serum digoxin levels. Monitor for signs of digoxin toxicity (anorexia, vomiting, arrhythmias).");

        // Cyclosporine + ketoconazole
        AddBidirectional(lookup, "cyclosporine", "ketoconazole", InteractionSeverity.Moderate,
            "Ketoconazole inhibits CYP3A4 and increases cyclosporine blood levels. If used together, reduce cyclosporine dose significantly and monitor levels.");

        // Cyclosporine + itraconazole
        AddBidirectional(lookup, "cyclosporine", "itraconazole", InteractionSeverity.Moderate,
            "Itraconazole inhibits CYP3A4 and increases cyclosporine levels. Reduce cyclosporine dose and monitor blood levels.");

        // Phenobarbital + cyclosporine
        AddBidirectional(lookup, "phenobarbital", "cyclosporine", InteractionSeverity.Moderate,
            "Phenobarbital induces CYP enzymes and decreases cyclosporine bioavailability. Higher cyclosporine doses may be needed.");

        // Tramadol + SSRI
        AddBidirectional(lookup, "tramadol", "fluoxetine", InteractionSeverity.Critical,
            "Tramadol + SSRIs risk serotonin syndrome: hyperthermia, tremors, agitation, seizures. Avoid combination.");
        AddBidirectional(lookup, "tramadol", "clomipramine", InteractionSeverity.Critical,
            "Tramadol + TCAs (tricyclic antidepressants) risk serotonin syndrome. Avoid combination.");

        // Fluoxetine + MAO inhibitors / selegiline
        AddBidirectional(lookup, "fluoxetine", "serotonin-baseline", InteractionSeverity.Moderate,
            "SSRIs combined with other serotonergic agents risk serotonin syndrome.");

        // Ketoconazole + midazolam
        AddBidirectional(lookup, "ketoconazole", "midazolam", InteractionSeverity.Moderate,
            "Ketoconazole inhibits CYP3A4 metabolism of midazolam, prolonging and intensifying sedation.");

        // Ketamine + acepromazine
        AddBidirectional(lookup, "ketamine", "acepromazine", InteractionSeverity.Info,
            "Acepromazine reduces seizure threshold when combined with ketamine. Use with caution; benzodiazepine co-induction preferred.");

        // Warfarin / anticoagulant + NSAIDs
        AddBidirectional(lookup, "acetylsalicylic-acid", "diphenhydramine", InteractionSeverity.Info,
            "No major interaction, but both have sedative/anticholinergic properties that may be additive.");

        // Theophylline + enrofloxacin
        AddBidirectional(lookup, "theophylline", "enrofloxacin", InteractionSeverity.Moderate,
            "Fluoroquinolones inhibit theophylline metabolism, increasing theophylline levels and risk of toxicity (tremors, seizures). Reduce theophylline dose and monitor.");

        // Theophylline + erythromycin
        AddBidirectional(lookup, "theophylline", "erythromycin", InteractionSeverity.Moderate,
            "Erythromycin inhibits theophylline metabolism; theophylline levels may rise significantly. Monitor for toxicity.");

        // Dexamethasone + phenobarbital
        AddBidirectional(lookup, "dexamethasone", "phenobarbital", InteractionSeverity.Moderate,
            "Phenobarbital accelerates dexamethasone metabolism, potentially reducing steroid efficacy.");

        // Potassium bromide + chloride loading
        AddBidirectional(lookup, "potassium-bromide", "normal-saline", InteractionSeverity.Moderate,
            "High chloride intake (IV saline, high-salt diets) reduces serum bromide levels by increasing renal bromide excretion. Monitor bromide levels with IV fluid therapy.");

        // Omeprazole + metronidazole (H.pylori triple therapy — not approved in vet but noted)
        AddBidirectional(lookup, "omeprazole", "metronidazole", InteractionSeverity.Info,
            "Omeprazole may modestly increase metronidazole plasma levels by inhibiting CYP2C19. Generally well tolerated; no dose adjustment needed.");

        // Spironolactone + ACE inhibitors — hyperkalemia
        AddBidirectional(lookup, "spironolactone", "benazepril", InteractionSeverity.Moderate,
            "Combination increases risk of hyperkalemia. Monitor serum potassium closely, particularly in cats with cardiac disease.");
        AddBidirectional(lookup, "spironolactone", "enalapril", InteractionSeverity.Moderate,
            "Combination may cause hyperkalemia. Monitor serum potassium.");

        // Furosemide + spironolactone (commonly used together; interaction is mild)
        AddBidirectional(lookup, "furosemide", "spironolactone", InteractionSeverity.Info,
            "Commonly used together in cardiac patients. Spironolactone counters furosemide-induced potassium loss. Still monitor electrolytes.");

        // Doxycycline + antacids / sucralfate
        AddBidirectional(lookup, "doxycycline", "sucralfate", InteractionSeverity.Moderate,
            "Sucralfate significantly reduces doxycycline absorption by chelation. Administer at least 2 hours apart.");

        // Clindamycin + erythromycin
        AddBidirectional(lookup, "clindamycin", "erythromycin", InteractionSeverity.Moderate,
            "Antagonistic antibacterial effect when combined (competition for same ribosomal binding site). Avoid combination.");

        // Amphotericin B not in the list but imidazoles + each other
        AddBidirectional(lookup, "itraconazole", "fluconazole", InteractionSeverity.Moderate,
            "Combining azole antifungals offers no added benefit and increases risk of hepatotoxicity. Use one agent at a time.");

        // Acepromazine + opioids
        AddBidirectional(lookup, "acepromazine", "morphine", InteractionSeverity.Moderate,
            "Neuroleptanalgesia combination; synergistic sedation and cardiovascular depression. Reduce doses of both agents.");
        AddBidirectional(lookup, "acepromazine", "butorphanol", InteractionSeverity.Info,
            "Standard neuroleptanalgesia combination. Monitor for cardiovascular depression, especially in compromised patients.");

        // Levetiracetam + phenobarbital
        AddBidirectional(lookup, "levetiracetam", "phenobarbital", InteractionSeverity.Moderate,
            "Phenobarbital may increase levetiracetam clearance due to enzyme induction. Higher levetiracetam doses may be required. Monitor seizure control.");

        // Calcium gluconate + digoxin
        AddBidirectional(lookup, "calcium-gluconate", "digoxin", InteractionSeverity.Critical,
            "IV calcium administration in digitalized patients can precipitate ventricular arrhythmias. Avoid IV calcium gluconate in patients receiving digoxin unless treating life-threatening hypocalcemia.");

        // Azathioprine + allopurinol (not in list, but note if needed)
        // Cyclosporine + azathioprine
        AddBidirectional(lookup, "cyclosporine", "azathioprine", InteractionSeverity.Moderate,
            "Combining immunosuppressants increases risk of severe immunosuppression and opportunistic infections. Use only with close monitoring.");

        // Insulin + corticosteroids
        AddBidirectional(lookup, "insulin-glargine", "prednisolone", InteractionSeverity.Moderate,
            "Corticosteroids cause insulin resistance and hyperglycemia, requiring insulin dose increases in diabetic patients.");
        AddBidirectional(lookup, "insulin-lente", "prednisolone", InteractionSeverity.Moderate,
            "Corticosteroids cause insulin resistance and increased insulin requirements in diabetic patients.");

        // Sildenafil + amlodipine
        AddBidirectional(lookup, "sildenafil", "amlodipine", InteractionSeverity.Moderate,
            "Additive hypotensive effect when sildenafil is combined with calcium channel blockers. Monitor blood pressure.");

        // Metoclopramide + opioids
        AddBidirectional(lookup, "metoclopramide", "morphine", InteractionSeverity.Moderate,
            "Opioids reduce GI motility, antagonising the prokinetic effect of metoclopramide. Co-administration may be ineffective for GI stasis.");

        // Tramadol + buprenorphine
        AddBidirectional(lookup, "tramadol", "buprenorphine", InteractionSeverity.Moderate,
            "Buprenorphine (partial agonist) may compete with tramadol for mu-opioid receptors, reducing analgesic efficacy of tramadol.");

        // Vitamin K1 + warfarin (clinical management)
        AddBidirectional(lookup, "vitamin-k1", "acetylsalicylic-acid", InteractionSeverity.Info,
            "Aspirin may slightly reduce vitamin K-dependent clotting factor activity. Generally not clinically significant at low aspirin doses.");
    }

    // ─── DOSAGE GUIDELINES ──────────────────────────────────────────────────────

    private static void AddDosageGuidelines(List<DrugCatalogEntry> list)
    {
        var lookup = list.ToDictionary(d => d.InnName);

        // Amoxicillin
        Get(lookup, "amoxicillin")?.AddDosageGuideline(Species.Dog, 10, 20, "mg/kg", "oral, q8-12h");
        Get(lookup, "amoxicillin")?.AddDosageGuideline(Species.Cat, 10, 20, "mg/kg", "oral, q12h");

        // Amoxicillin-Clavulanate
        Get(lookup, "amoxicillin-clavulanate")?.AddDosageGuideline(Species.Dog, 12.5m, 25, "mg/kg", "oral, q12h");
        Get(lookup, "amoxicillin-clavulanate")?.AddDosageGuideline(Species.Cat, 12.5m, 25, "mg/kg", "oral, q12h");

        // Cephalexin
        Get(lookup, "cephalexin")?.AddDosageGuideline(Species.Dog, 22, 30, "mg/kg", "oral, q8h");
        Get(lookup, "cephalexin")?.AddDosageGuideline(Species.Cat, 15, 30, "mg/kg", "oral, q12h");

        // Enrofloxacin
        Get(lookup, "enrofloxacin")?.AddDosageGuideline(Species.Dog, 5, 20, "mg/kg", "oral or SC, q24h");
        Get(lookup, "enrofloxacin")?.AddDosageGuideline(Species.Cat, 2.5m, 5, "mg/kg", "oral or SC, q24h (max 5 mg/kg)");

        // Doxycycline
        Get(lookup, "doxycycline")?.AddDosageGuideline(Species.Dog, 5, 10, "mg/kg", "oral, q12-24h");
        Get(lookup, "doxycycline")?.AddDosageGuideline(Species.Cat, 5, 10, "mg/kg", "oral, q12h; follow with 6 mL water");

        // Metronidazole
        Get(lookup, "metronidazole")?.AddDosageGuideline(Species.Dog, 10, 15, "mg/kg", "oral, q8-12h");
        Get(lookup, "metronidazole")?.AddDosageGuideline(Species.Cat, 7.5m, 10, "mg/kg", "oral, q12h (max 62.5 mg total)");

        // Meloxicam
        Get(lookup, "meloxicam")?.AddDosageGuideline(Species.Dog, 0.1m, 0.2m, "mg/kg", "oral, q24h (loading dose 0.2 mg/kg day 1)");
        Get(lookup, "meloxicam")?.AddDosageGuideline(Species.Cat, 0.05m, 0.1m, "mg/kg", "oral, q24-48h; lowest effective dose");
        Get(lookup, "meloxicam")?.AddDosageGuideline(Species.Horse, 0.6m, 0.6m, "mg/kg", "oral or IV, q24h");

        // Carprofen
        Get(lookup, "carprofen")?.AddDosageGuideline(Species.Dog, 2.2m, 4.4m, "mg/kg", "oral, q12h or 4.4 mg/kg q24h");

        // Tramadol
        Get(lookup, "tramadol")?.AddDosageGuideline(Species.Dog, 2, 10, "mg/kg", "oral, q8-12h");
        Get(lookup, "tramadol")?.AddDosageGuideline(Species.Cat, 2, 4, "mg/kg", "oral, q12h");

        // Gabapentin
        Get(lookup, "gabapentin")?.AddDosageGuideline(Species.Dog, 5, 10, "mg/kg", "oral, q8-12h");
        Get(lookup, "gabapentin")?.AddDosageGuideline(Species.Cat, 5, 10, "mg/kg", "oral, q8-12h");

        // Buprenorphine
        Get(lookup, "buprenorphine")?.AddDosageGuideline(Species.Dog, 0.01m, 0.02m, "mg/kg", "IV, IM, or SC, q6-8h");
        Get(lookup, "buprenorphine")?.AddDosageGuideline(Species.Cat, 0.01m, 0.03m, "mg/kg", "IV, IM, or transmucosally, q6-8h");

        // Furosemide
        Get(lookup, "furosemide")?.AddDosageGuideline(Species.Dog, 1, 4, "mg/kg", "oral, IV, or IM, q8-24h");
        Get(lookup, "furosemide")?.AddDosageGuideline(Species.Cat, 1, 2, "mg/kg", "oral, IV, or IM, q8-24h");

        // Enalapril
        Get(lookup, "enalapril")?.AddDosageGuideline(Species.Dog, 0.25m, 0.5m, "mg/kg", "oral, q12-24h");
        Get(lookup, "enalapril")?.AddDosageGuideline(Species.Cat, 0.25m, 0.5m, "mg/kg", "oral, q24h");

        // Benazepril
        Get(lookup, "benazepril")?.AddDosageGuideline(Species.Dog, 0.25m, 0.5m, "mg/kg", "oral, q24h");
        Get(lookup, "benazepril")?.AddDosageGuideline(Species.Cat, 0.25m, 0.5m, "mg/kg", "oral, q24h");

        // Prednisolone
        Get(lookup, "prednisolone")?.AddDosageGuideline(Species.Dog, 1, 2, "mg/kg", "oral, q24h (anti-inflammatory); 2-4 mg/kg (immunosuppressive)");
        Get(lookup, "prednisolone")?.AddDosageGuideline(Species.Cat, 1, 2, "mg/kg", "oral, q24h; cats may need higher doses");

        // Dexamethasone
        Get(lookup, "dexamethasone")?.AddDosageGuideline(Species.Dog, 0.1m, 0.2m, "mg/kg", "IV or IM, q12-24h (anti-inflammatory)");
        Get(lookup, "dexamethasone")?.AddDosageGuideline(Species.Cat, 0.1m, 0.2m, "mg/kg", "IV or IM, q12-24h");

        // Maropitant
        Get(lookup, "maropitant")?.AddDosageGuideline(Species.Dog, 1, 1, "mg/kg", "SC or oral, q24h");
        Get(lookup, "maropitant")?.AddDosageGuideline(Species.Cat, 1, 1, "mg/kg", "SC, q24h");

        // Omeprazole
        Get(lookup, "omeprazole")?.AddDosageGuideline(Species.Dog, 0.5m, 1, "mg/kg", "oral, q24h");
        Get(lookup, "omeprazole")?.AddDosageGuideline(Species.Cat, 0.5m, 1, "mg/kg", "oral, q24h");

        // Famotidine
        Get(lookup, "famotidine")?.AddDosageGuideline(Species.Dog, 0.5m, 1, "mg/kg", "oral or IV, q12-24h");
        Get(lookup, "famotidine")?.AddDosageGuideline(Species.Cat, 0.5m, 1, "mg/kg", "oral, q12-24h");

        // Phenobarbital
        Get(lookup, "phenobarbital")?.AddDosageGuideline(Species.Dog, 2, 5, "mg/kg", "oral, q12h; titrate to seizure control");
        Get(lookup, "phenobarbital")?.AddDosageGuideline(Species.Cat, 2, 3, "mg/kg", "oral, q12h");

        // Levetiracetam
        Get(lookup, "levetiracetam")?.AddDosageGuideline(Species.Dog, 20, 30, "mg/kg", "oral, q8h");
        Get(lookup, "levetiracetam")?.AddDosageGuideline(Species.Cat, 20, 30, "mg/kg", "oral, q8h");

        // Cyclosporine
        Get(lookup, "cyclosporine")?.AddDosageGuideline(Species.Dog, 5, 10, "mg/kg", "oral, q24h");
        Get(lookup, "cyclosporine")?.AddDosageGuideline(Species.Cat, 5, 7, "mg/kg", "oral, q24h");

        // Methimazole
        Get(lookup, "methimazole")?.AddDosageGuideline(Species.Cat, 2.5m, 5, "mg/cat", "oral, q12h (not per kg — fixed dose)");

        // Pimobendan
        Get(lookup, "pimobendan")?.AddDosageGuideline(Species.Dog, 0.2m, 0.3m, "mg/kg", "oral, q12h");

        // Diltiazem
        Get(lookup, "diltiazem")?.AddDosageGuideline(Species.Cat, 1.75m, 2.5m, "mg/kg", "oral (regular release), q8h");
        Get(lookup, "diltiazem")?.AddDosageGuideline(Species.Dog, 0.5m, 1.5m, "mg/kg", "oral, q8h");

        // Atenolol
        Get(lookup, "atenolol")?.AddDosageGuideline(Species.Cat, 6.25m, 12.5m, "mg/cat", "oral, q24h (fixed dose)");
        Get(lookup, "atenolol")?.AddDosageGuideline(Species.Dog, 0.25m, 1, "mg/kg", "oral, q12-24h");

        // Ivermectin
        Get(lookup, "ivermectin")?.AddDosageGuideline(Species.Dog, 0.006m, 0.012m, "mg/kg", "oral, monthly (heartworm prevention)");
        Get(lookup, "ivermectin")?.AddDosageGuideline(Species.Horse, 0.2m, 0.2m, "mg/kg", "oral, q8 weeks (endoparasites)");

        // Fenbendazole
        Get(lookup, "fenbendazole")?.AddDosageGuideline(Species.Dog, 50, 50, "mg/kg", "oral, q24h for 3 days");
        Get(lookup, "fenbendazole")?.AddDosageGuideline(Species.Cat, 50, 50, "mg/kg", "oral, q24h for 3 days");
        Get(lookup, "fenbendazole")?.AddDosageGuideline(Species.Horse, 5, 10, "mg/kg", "oral, single dose or 5-day course");

        // Praziquantel
        Get(lookup, "praziquantel")?.AddDosageGuideline(Species.Dog, 5, 10, "mg/kg", "oral, single dose");
        Get(lookup, "praziquantel")?.AddDosageGuideline(Species.Cat, 5, 10, "mg/kg", "oral, single dose");

        // Ketamine
        Get(lookup, "ketamine")?.AddDosageGuideline(Species.Dog, 5, 10, "mg/kg", "IV (induction) or 10-20 mg/kg IM");
        Get(lookup, "ketamine")?.AddDosageGuideline(Species.Cat, 5, 15, "mg/kg", "IM for sedation; 1-2 mg/kg IV for induction");

        // Propofol
        Get(lookup, "propofol")?.AddDosageGuideline(Species.Dog, 4, 6, "mg/kg", "IV (slow bolus to effect)");
        Get(lookup, "propofol")?.AddDosageGuideline(Species.Cat, 4, 6, "mg/kg", "IV (slow bolus to effect)");

        // Medetomidine
        Get(lookup, "medetomidine")?.AddDosageGuideline(Species.Dog, 0.01m, 0.04m, "mg/kg", "IM or IV for sedation");
        Get(lookup, "medetomidine")?.AddDosageGuideline(Species.Cat, 0.05m, 0.08m, "mg/kg", "IM for sedation");

        // Acepromazine
        Get(lookup, "acepromazine")?.AddDosageGuideline(Species.Dog, 0.025m, 0.05m, "mg/kg", "IM or SC (max 3 mg total)");
        Get(lookup, "acepromazine")?.AddDosageGuideline(Species.Cat, 0.05m, 0.1m, "mg/kg", "IM or SC");

        // Trimethoprim-Sulfamethoxazole
        Get(lookup, "trimethoprim-sulfamethoxazole")?.AddDosageGuideline(Species.Dog, 15, 30, "mg/kg", "oral, q12h");
        Get(lookup, "trimethoprim-sulfamethoxazole")?.AddDosageGuideline(Species.Cat, 15, 30, "mg/kg", "oral, q12h");

        // Clindamycin
        Get(lookup, "clindamycin")?.AddDosageGuideline(Species.Dog, 5.5m, 11, "mg/kg", "oral, q12h");
        Get(lookup, "clindamycin")?.AddDosageGuideline(Species.Cat, 5.5m, 11, "mg/kg", "oral, q12h");

        // Fluoxetine
        Get(lookup, "fluoxetine")?.AddDosageGuideline(Species.Dog, 1, 2, "mg/kg", "oral, q24h");
        Get(lookup, "fluoxetine")?.AddDosageGuideline(Species.Cat, 0.5m, 1, "mg/kg", "oral, q24h");

        // Trazodone
        Get(lookup, "trazodone")?.AddDosageGuideline(Species.Dog, 2, 5, "mg/kg", "oral, q8-24h (situational anxiety)");

        // Diphenhydramine
        Get(lookup, "diphenhydramine")?.AddDosageGuideline(Species.Dog, 1, 2, "mg/kg", "oral, IM, or SC, q8-12h");
        Get(lookup, "diphenhydramine")?.AddDosageGuideline(Species.Cat, 1, 2, "mg/kg", "oral or IM, q8h");

        // Vitamin K1
        Get(lookup, "vitamin-k1")?.AddDosageGuideline(Species.Dog, 2.5m, 5, "mg/kg", "SC divided q12h (first 2 days), then oral q12-24h");
        Get(lookup, "vitamin-k1")?.AddDosageGuideline(Species.Cat, 2.5m, 5, "mg/kg", "SC divided q12h (first 2 days), then oral q12-24h");

        // Atropine
        Get(lookup, "atropine")?.AddDosageGuideline(Species.Dog, 0.02m, 0.04m, "mg/kg", "IV, IM, or SC (preanesthetic or bradycardia)");
        Get(lookup, "atropine")?.AddDosageGuideline(Species.Cat, 0.02m, 0.04m, "mg/kg", "IV, IM, or SC");

        // Mannitol
        Get(lookup, "mannitol")?.AddDosageGuideline(Species.Dog, 500, 1000, "mg/kg", "IV slow infusion over 20 min (cerebral oedema)");
        Get(lookup, "mannitol")?.AddDosageGuideline(Species.Cat, 250, 500, "mg/kg", "IV slow infusion over 20 min");

        // Itraconazole
        Get(lookup, "itraconazole")?.AddDosageGuideline(Species.Dog, 5, 10, "mg/kg", "oral, q24h with food");
        Get(lookup, "itraconazole")?.AddDosageGuideline(Species.Cat, 5, 10, "mg/kg", "oral, q24h with food");

        // Fluconazole
        Get(lookup, "fluconazole")?.AddDosageGuideline(Species.Dog, 5, 10, "mg/kg", "oral, q24h");
        Get(lookup, "fluconazole")?.AddDosageGuideline(Species.Cat, 25, 50, "mg/cat", "oral, q12-24h (fixed dose)");

        // Terbinafine
        Get(lookup, "terbinafine")?.AddDosageGuideline(Species.Dog, 30, 35, "mg/kg", "oral, q24h with food");
        Get(lookup, "terbinafine")?.AddDosageGuideline(Species.Cat, 30, 35, "mg/kg", "oral, q24h with food");

        // Midazolam
        Get(lookup, "midazolam")?.AddDosageGuideline(Species.Dog, 0.1m, 0.3m, "mg/kg", "IV, IM, or intranasal");
        Get(lookup, "midazolam")?.AddDosageGuideline(Species.Cat, 0.1m, 0.3m, "mg/kg", "IV, IM, or intranasal");

        // Morphine
        Get(lookup, "morphine")?.AddDosageGuideline(Species.Dog, 0.1m, 0.5m, "mg/kg", "IM or SC, q4-6h");
        Get(lookup, "morphine")?.AddDosageGuideline(Species.Cat, 0.05m, 0.1m, "mg/kg", "IM or SC, q4-6h (use with caution)");
    }

    // ─── HELPERS ─────────────────────────────────────────────────────────────────

    private static DrugCatalogEntry Make(string innName, string displayName, DrugCategory category)
    {
        var result = DrugCatalogEntry.Create(innName, displayName, category, clinicId: null);
        // Create always succeeds with valid inputs
        return result.Value;
    }

    private static DrugCatalogEntry? Get(Dictionary<string, DrugCatalogEntry> lookup, string innName)
    {
        return lookup.TryGetValue(innName, out var entry) ? entry : null;
    }

    private static void AddBidirectional(
        Dictionary<string, DrugCatalogEntry> lookup,
        string drug1,
        string drug2,
        InteractionSeverity severity,
        string description)
    {
        var entry1 = Get(lookup, drug1);
        var entry2 = Get(lookup, drug2);

        if (entry1 is not null && entry2 is not null)
        {
            entry1.AddInteraction(entry2.Id, entry2.DisplayName, severity, description);
            entry2.AddInteraction(entry1.Id, entry1.DisplayName, severity, description);
        }
        else if (entry1 is not null)
        {
            // drug2 might be a placeholder name — only add one direction
            entry1.AddInteraction(Guid.Empty, drug2, severity, description);
        }
    }
}
