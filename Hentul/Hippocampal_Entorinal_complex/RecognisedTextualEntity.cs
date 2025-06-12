namespace Hentul.Hippocampal_Entorinal_complex
{
    public class RecognisedTextualEntity : Entity
    {
        public List<Sensation> Sensations { get; private set; }
        public string Name { get; private set; }



        public RecognisedTextualEntity() 
        {
            Sensations = new();
            Name = string.Empty;
        }

        public RecognisedTextualEntity(List<Sensation> sensations, string name)
        {
            this.Sensations = sensations;
            this.Name = name;
        }



    }
}
