namespace Borealis.TestHelpers;

public static class XUnitCancellationTokenHelper {
    public static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;
}
