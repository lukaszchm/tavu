using System.Text.RegularExpressions;
using Tavu.Exceptions;

namespace Tavu.Storage
{
    public interface IExcerciseStore 
    {
        Task<string> GetWords(string userId);

        Task SetWords(string userId, string rawText);

    }

    public class ExcerciseStore : IExcerciseStore
    {
        private const string WordsFileTemplate = "/{0}/words.txt";

        private readonly Regex ValidationRegex = new Regex(@"^\p{L}+$");

        private readonly IBlobStore blobStore;

        public ExcerciseStore(IBlobStore blobStore)
        {
            this.blobStore = blobStore ?? throw new TavuServiceConfigurationException();
        }

        public async Task<string> GetWords(string userId)
        {
            string blobContent = await blobStore.GetBlobContentAsync(
                string.Format(WordsFileTemplate, userId))
                ?? string.Empty;
            return blobContent;
        }

        public async Task SetWords(string userId, string rawText)
        {
            var normalizedText = string.Join(Environment.NewLine, SplitAndValidate(rawText));
            await blobStore.SetBlobAsync(string.Format(WordsFileTemplate, userId), normalizedText);
        }

        private IEnumerable<string> SplitAndValidate(string rawText)
        {
            var splitted = (rawText ?? string.Empty).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawWord in splitted)
            {
                var normalizedWord = rawWord.ToLower().Trim();
                if (ValidationRegex.IsMatch(normalizedWord))
                {
                    yield return normalizedWord;
                }
                else 
                {
                    throw new TavuValueValidationException($"Word {normalizedWord} does not match the validation expression.");
                }
            }
        }
    }
}