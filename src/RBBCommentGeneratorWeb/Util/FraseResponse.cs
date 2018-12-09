namespace RBBCommentGeneratorWeb.Util
{
    public class FraseResponse
    {
        public FraseResponse(string frase)
        {
            Frase = frase;
            var fraseZip = TextUtilities.Zip(frase);
            Abbr = System.Convert.ToBase64String(fraseZip);
        }

        public FraseResponse()
        {

        }

        public string Frase { get; set; }
        public string Abbr { get; set; }
    }
}
