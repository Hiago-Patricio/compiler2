namespace Compiler
{
    public class Symbol
    {
        public Symbol(EnumToken type, string value, int endRel)
        {
            this.type = type;
            this.value = value;
            this.endRel = endRel;
        }

        public string value {get; set; }
        public EnumToken type {get; set; }
        public int endRel { get; set; }

        public override string ToString()
        {
            return $"Token[{type}, {value}]";
        }
    }
}