namespace Borealis.Core.Contracts;

public interface ICaptchaSolverFactory {
    ICaptchaSolver CreateCaptchaSolver();
}
