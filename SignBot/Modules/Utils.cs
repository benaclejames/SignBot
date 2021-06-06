namespace SignBot.Modules
{
    public static class Utils
    {
        public static string Beautify(this string inputString) => char.ToUpper(inputString[0]) + inputString.ToLower()[1..];
    }
}