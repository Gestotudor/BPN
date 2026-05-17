namespace TransferService.Application.Interfaces;

public interface ITransferFeeCalculator
{
    decimal Calculate(decimal tryAmount);
}
