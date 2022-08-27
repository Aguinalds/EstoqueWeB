
namespace EstoqueWeb.Extensions
{
    public static class StringExtensions
    {
        public static string PrimeiraPalavra(this string texto)
        {
            return texto.Trim().Substring(2, texto.IndexOf(" "));
        }
    }
}
