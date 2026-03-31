namespace Vetolib.Auth.Contracts;

public record InviteVetRequest(
    string VetEmail,
    string OwnerName,
    string PetName,
    string? Message);
