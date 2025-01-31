using Godot;

public class SaveField {
	public enum FieldType : uint {
		Int,
		Vec2,
		Vec3,
		String,
		Float,

		Count
	};

	public SaveField( string name, Variant value, FileAccess file ) {
		file.StorePascalString( name );
		file.StoreVar( value );
	}
	public SaveField( FileAccess file ) {
		Name = file.GetPascalString();
		Type = (FieldType)file.Get32();

		switch ( Type ) {
		case FieldType.Int:
			Value = (int)file.Get32();
			break;
		case FieldType.Vec2: {
			Vector2 value;
			value.X = file.GetFloat();
			value.Y = file.GetFloat();
			Value = value;
			break; }
		case FieldType.Vec3: {
			Vector3 value;
			value.X = file.GetFloat();
			value.Y = file.GetFloat();
			value.Z = file.GetFloat();
			Value = value;
			break; }
		case FieldType.String:
			Value = file.GetPascalString();
			break;
		case FieldType.Float:
			Value = file.GetFloat();
			break;
		default:
			GD.PushError( "Invalid save field type " + Type + ", corruption or incompatible version?" );
			break;
		};
	}

	public Variant GetValue() {
		return Value;
	}
	public string GetName() {
		return Name;
	}
	public FieldType GetType() {
		return Type;
	}

	private string Name;
	private FieldType Type;
	private Variant Value;
};