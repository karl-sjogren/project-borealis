using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace Borealis.Core.Lucene;

public class BorealisAnalyzer : Analyzer {
    private readonly LuceneVersion _version = LuceneVersion.LUCENE_48;

    protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader) {
        var tokenizer = new StandardTokenizer(_version, reader);
        TokenStream filter = new StandardFilter(_version, tokenizer);
        filter = new LowerCaseFilter(_version, filter);
        filter = new StopFilter(_version, filter, StopAnalyzer.ENGLISH_STOP_WORDS_SET);
        filter = new ASCIIFoldingFilter(filter);
        return new TokenStreamComponents(tokenizer, filter);
    }
}
