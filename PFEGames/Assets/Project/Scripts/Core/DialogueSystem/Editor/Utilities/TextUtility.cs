namespace Goblfin.DialogueView.Utilities
{
    public static class TextUtility
    {
        // Benchmarks: https://stackoverflow.com/a/37347881
        public static string RemoveWhitespaces(this string text)
        {
            int textLength = text.Length;

            char[] textCharacters = text.ToCharArray();

            int currentWhitespacelessTextLength = 0;

            for (int currentCharacterIndex = 0; currentCharacterIndex < textLength; ++currentCharacterIndex)
            {
                char currentTextCharacter = textCharacters[currentCharacterIndex];

                if (char.IsWhiteSpace(currentTextCharacter))
                {
                    continue;
                }

                textCharacters[currentWhitespacelessTextLength++] = currentTextCharacter;
            }

            return new string(textCharacters, 0, currentWhitespacelessTextLength);
        }

        // See here for alternatives: https://stackoverflow.com/questions/3210393/how-do-i-remove-all-non-alphanumeric-characters-from-a-string-except-dash
        public static string RemoveSpecialCharacters(this string text)
        {
            int textLength = text.Length;

            char[] textCharacters = text.ToCharArray();

            int currentWhitespacelessTextLength = 0;

            for (int currentCharacterIndex = 0; currentCharacterIndex < textLength; ++currentCharacterIndex)
            {
                char currentTextCharacter = textCharacters[currentCharacterIndex];

                if (!char.IsLetterOrDigit(currentTextCharacter) && !char.IsWhiteSpace(currentTextCharacter))
                {
                    continue;
                }

                textCharacters[currentWhitespacelessTextLength++] = currentTextCharacter;
            }

            return new string(textCharacters, 0, currentWhitespacelessTextLength);
        }
    }
}
