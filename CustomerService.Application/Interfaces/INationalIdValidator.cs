namespace CustomerService.Application.Interfaces;

public interface INationalIdValidator
{
    bool IsValid(string nationalIdNumber);
}
