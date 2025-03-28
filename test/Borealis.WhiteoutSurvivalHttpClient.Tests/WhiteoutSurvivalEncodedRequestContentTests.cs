namespace Borealis.WhiteoutSurvivalHttpClient.Tests;

public class WhiteoutSurvivalSignedRequestContentTests {
    [Fact]
    public void GetEncodedValues_WhenCalled_ReturnsExpectedQueryValues() {
        var queryValues = new List<KeyValuePair<string, string>>() {
            new("giraffe", "banana"),
            new("zebra", "apple"),
            new("elephant", "orange"),
            new("monkey", "grape"),
            new("lion", "kiwi")
        };

        var signedValues = WhiteoutSurvivalSignedRequestContent.GetSignedValues("secret", queryValues);

        signedValues.ShouldNotBeNull();
        signedValues.ShouldNotBeEmpty();
        signedValues.Count().ShouldBe(6);

        // The first value should be the sign and the rest should be the original values but sorted by key
        signedValues.ShouldBe([
            new("sign", "3d51fe19af91835619897761cdceb483"),
            new("elephant", "orange"),
            new("giraffe", "banana"),
            new("lion", "kiwi"),
            new("monkey", "grape"),
            new("zebra", "apple")
        ]);
    }
}
