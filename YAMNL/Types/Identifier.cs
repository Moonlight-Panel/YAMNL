namespace YAMNL.Types
{
    public class Identifier
    {

        public const string DefaultNamespace = "minecraft";

        public Identifier(string identifier)
        {
            var namespaceSplit = identifier.IndexOf(":");
            if (namespaceSplit == -1)
            {
                Namespace = DefaultNamespace;
                Value = identifier;
            }
            else
            {
                Namespace = identifier.Substring(0, namespaceSplit);
                Value = identifier.Substring(namespaceSplit + 1);
            }
        }

        public Identifier(string Namespace, string value)
        {
            this.Namespace = Namespace;
            Value = value;
        }

        public string Namespace
        {
            get;
        }
        public string Value
        {
            get;
        }

        public override string ToString() => Namespace + ":" + Value;

        public static implicit operator string(Identifier identifier) => identifier.ToString();
        public static explicit operator Identifier(string identifier) => new Identifier(identifier);
    }
}
