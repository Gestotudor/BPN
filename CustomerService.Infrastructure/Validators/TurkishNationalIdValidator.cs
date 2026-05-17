using CustomerService.Application.Interfaces;

namespace CustomerService.Infrastructure.Validators;

public sealed class TurkishNationalIdValidator : INationalIdValidator
{
    public bool IsValid(string nationalIdNumber)
    {
        if (string.IsNullOrWhiteSpace(nationalIdNumber) ||
            nationalIdNumber.Length != 11 ||
            !nationalIdNumber.All(char.IsDigit) ||
            nationalIdNumber[0] == '0')
        {
            return false;
        }

        var digits = nationalIdNumber.Select(c => c - '0').ToArray();
        var oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        var evenSum = digits[1] + digits[3] + digits[5] + digits[7];
        var tenthDigit = ((oddSum * 7) - evenSum) % 10;

        if (tenthDigit != digits[9])
        {
            return false;
        }

        var firstTenSum = digits.Take(10).Sum();
        var eleventhDigit = firstTenSum % 10;

        return eleventhDigit == digits[10];
    }
}
