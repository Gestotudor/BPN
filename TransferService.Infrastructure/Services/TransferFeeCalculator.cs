using TransferService.Application.Interfaces;

namespace TransferService.Infrastructure.Services;

public sealed class TransferFeeCalculator : ITransferFeeCalculator
{
    private const decimal FeeRate = 0.01m;

    public decimal Calculate(decimal tryAmount)
    {
        return Math.Round(tryAmount * FeeRate, 2, MidpointRounding.AwayFromZero);
    }
}
