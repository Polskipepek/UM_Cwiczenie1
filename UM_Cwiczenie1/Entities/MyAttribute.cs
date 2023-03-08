namespace Cwiczenie1.Entities {
    public class MyAttribute {
        public string Name { get; set; }
        public object Value { get; set; }

        public MyAttribute() { }
        public MyAttribute(string name, object value) {
            Name = name;
            Value = value;
        }
        override public string ToString() {
            return $"Name: {Name} \tValue: {Value}";
        }

        public override bool Equals(object? obj) {
            if (obj == null) return Value == null;
            return Name.Equals((obj as MyAttribute).Name) && Value.Equals((obj as MyAttribute).Value);
        }

        public override int GetHashCode() {
            throw new NotImplementedException();
        }
    }
}
