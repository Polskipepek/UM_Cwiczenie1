using UM_Cwiczenie1.Entities;

namespace Cwiczenie1.Entities {
    public class MyAttribute {
        public string Name { get; set; }
        public object Value { get; set; }

        public AttributeType AttributeType { get; set; }
        public int ValuesNum { get; set; } = 0;
        public double Min { get; set; }
        public double Max { get; set; }

        public MyAttribute() { }
        public MyAttribute(string name, object value, AttributeType attributeType) {
            Name = name;
            Value = value;
            AttributeType = attributeType;
        }

        public MyAttribute(string name, object value, AttributeType attributeType, int valuesNum)
        {
            Name = name;
            Value = value;
            AttributeType = attributeType;
            ValuesNum = valuesNum;
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
