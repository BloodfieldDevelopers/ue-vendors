using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.UnrealTypes;
using UAssetAPI.UnrealTypes.EngineEnums;

namespace UAssetAPI.PropertyTypes.Structs
{
    /*
        The code within this file is modified from LongerWarrior's UEAssetToolkitGenerator project, which is licensed under the Apache License 2.0.
        Please see the NOTICE.md file distributed with UAssetAPI and UAssetGUI for more information.
    */

    public class MovieSceneFloatValuePropertyData : PropertyData<FMovieSceneFloatValue>
    {
        public MovieSceneFloatValuePropertyData(FName name) : base(name)
        {

        }

        public MovieSceneFloatValuePropertyData()
        {
            
        }

        private static readonly FString CurrentPropertyType = new FString("MovieSceneFloatValue");
        public override bool HasCustomStructSerialization { get { return true; } }
        public override FString PropertyType { get { return CurrentPropertyType; } }

        public override void Read(AssetBinaryReader reader, bool includeHeader, long leng1, long leng2 = 0)
        {
            if (includeHeader)
            {
                PropertyGuid = reader.ReadPropertyGuid();
            }

            // Value is a struct returned by a property; reading into that copy
            // discards every key's value and tangents.
            var value = new FMovieSceneFloatValue();
            // Tagged keys use Serialize's field order, not a channel's bulk layout.
            value.Value = reader.ReadSingle();
            value.InterpMode = (ERichCurveInterpMode)reader.ReadByte();
            value.TangentMode = (ERichCurveTangentMode)reader.ReadByte();
            value.Tangent.ArriveTangent = reader.ReadSingle();
            value.Tangent.LeaveTangent = reader.ReadSingle();
            value.Tangent.TangentWeightMode = (ERichCurveTangentWeightMode)reader.ReadByte();
            value.Tangent.ArriveTangentWeight = reader.ReadSingle();
            value.Tangent.LeaveTangentWeight = reader.ReadSingle();
            Value = value;
        }

        public override int Write(AssetBinaryWriter writer, bool includeHeader)
        {
            if (includeHeader)
            {
                writer.WritePropertyGuid(PropertyGuid);
            }

            int here = (int)writer.BaseStream.Position;

            writer.Write(Value.Value);
            writer.Write((byte)Value.InterpMode);
            writer.Write((byte)Value.TangentMode);
            writer.Write(Value.Tangent.ArriveTangent);
            writer.Write(Value.Tangent.LeaveTangent);
            writer.Write((byte)Value.Tangent.TangentWeightMode);
            writer.Write(Value.Tangent.ArriveTangentWeight);
            writer.Write(Value.Tangent.LeaveTangentWeight);

            return (int)writer.BaseStream.Position - here;
        }

        public override void FromString(string[] d, UAsset asset)
        {

        }

        public override string ToString()
        {
            return "";
        }
    }
}
