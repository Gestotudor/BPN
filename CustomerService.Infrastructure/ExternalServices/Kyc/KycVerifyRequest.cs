namespace CustomerService.Infrastructure.ExternalServices.Kyc;

public sealed record KycVerifyRequest(
    string UserId,
    string Tcno,
    int BirthYear,
    string Name,
    string Surname);
