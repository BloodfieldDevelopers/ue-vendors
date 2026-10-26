using System.Numerics;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Objects.RenderCore
{
    [JsonConverter(typeof(FPackedNormalConverter))]
    public struct FPackedNormal : IUStruct
    {
        public uint Data;
        private readonly bool _signed;
        public float X => Decode(Data);
        public float Y => Decode(Data >> 8);
        public float Z => Decode(Data >> 16);
        public float W => Decode(Data >> 24);

        private float Decode(uint component) => _signed
            ? MathF.Max(unchecked((sbyte) component) / 127.0f, -1.0f)
            : (component & 0xFF) / 127.5f - 1.0f;

        public FPackedNormal(FArchive Ar)
        {
            Data = Ar.Read<uint>();
            // Modern vertex declarations use R8G8B8A8_SNORM. XOR plus UNORM
            // decoding is not equivalent: it biases every normal component.
            _signed = FRenderingObjectVersion.Get(Ar) >= FRenderingObjectVersion.Type.IncreaseNormalPrecision;
        }

        public FPackedNormal(uint data, bool signed = false)
        {
            Data = data;
            _signed = signed;
        }


        public FPackedNormal(FVector vector) : this(vector.X, vector.Y, vector.Z, 0f) { }

        public FPackedNormal(FVector4 vector) : this(vector.X, vector.Y, vector.Z, vector.W) { }

        private FPackedNormal(float x, float y, float z, float w)
        {
            Data = Pack(x) | Pack(y) << 8 | Pack(z) << 16 | Pack(w) << 24;
            _signed = false;
        }

        public void SetW(float value)
        {
            Data = (Data & 0xFFFFFF) | (uint) ((int) Math.Round(value * 127.0f) << 24);
        }

        public float GetW()
        {
            return (byte) (Data >> 24) / 127.0f;
        }

        private uint Pack(float value) => (uint) Math.Clamp((int) MathF.Round((value + 1f) * 127.5f), 0, 255);

        public static explicit operator FVector(FPackedNormal packedNormal) => new(packedNormal.X, packedNormal.Y, packedNormal.Z);
        public static implicit operator FVector4(FPackedNormal packedNormal) => new(packedNormal.X, packedNormal.Y, packedNormal.Z, packedNormal.W);
        public static explicit operator Vector3(FPackedNormal packedNormal) => new(packedNormal.X, packedNormal.Y, packedNormal.Z);
        public static implicit operator Vector4(FPackedNormal packedNormal) => new(packedNormal.X, packedNormal.Y, packedNormal.Z, packedNormal.W);

        public static bool operator ==(FPackedNormal a, FPackedNormal b) => a.Data == b.Data && a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
        public static bool operator !=(FPackedNormal a, FPackedNormal b) => a.Data != b.Data || a.X != b.X || a.Y != b.Y || a.Z != b.Z || a.W != b.W;
    }

    public struct FDeprecatedSerializedPackedNormal
    {
        public uint Data;

        public static FVector4 VectorMultiplyAdd(FVector4 vec1, FVector4 vec2, FVector4 vec3) =>
            new(vec1.X * vec2.X + vec3.X, vec1.Y * vec2.Y + vec3.Y, vec1.Z * vec2.Z + vec3.Z, vec1.W * vec2.W + vec3.W);

        public static explicit operator FVector4(FDeprecatedSerializedPackedNormal packed)
        {
            var vectorToUnpack = new FVector4(packed.Data & 0xFF, (packed.Data >> 8) & 0xFF, (packed.Data >> 16) & 0xFF, (packed.Data >> 24) & 0xFF);
            return VectorMultiplyAdd(vectorToUnpack, new FVector4(1.0f / 127.5f), new FVector4(-1.0f));
        }

        public static explicit operator FVector(FDeprecatedSerializedPackedNormal packed) => (FVector) (FVector4) packed;
    }
}
