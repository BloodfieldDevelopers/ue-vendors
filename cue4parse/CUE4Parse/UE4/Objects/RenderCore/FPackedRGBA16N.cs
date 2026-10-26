using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Objects.RenderCore
{
    [JsonConverter(typeof(FPackedRGBA16NConverter))]
    public class FPackedRGBA16N
    {
        public readonly uint X;
        public readonly uint Y;
        public readonly uint Z;
        public readonly uint W;
        private readonly bool _signed;

        public FPackedRGBA16N(FArchive Ar)
        {
            X = Ar.Read<ushort>();
            Y = Ar.Read<ushort>();
            Z = Ar.Read<ushort>();
            W = Ar.Read<ushort>();

            _signed = Ar.Game >= GAME_UE4_20;
        }

        private float Decode(uint component) => _signed
            ? MathF.Max(unchecked((short) component) / 32767.0f, -1.0f)
            : component / 32767.5f - 1.0f;

        public static explicit operator FVector(FPackedRGBA16N packedRGBA16N)
        {
            var X = packedRGBA16N.Decode(packedRGBA16N.X);
            var Y = packedRGBA16N.Decode(packedRGBA16N.Y);
            var Z = packedRGBA16N.Decode(packedRGBA16N.Z);

            return new FVector(X, Y, Z);
        }

        public static explicit operator FVector4(FPackedRGBA16N packedRGBA16N)
        {
            var X = packedRGBA16N.Decode(packedRGBA16N.X);
            var Y = packedRGBA16N.Decode(packedRGBA16N.Y);
            var Z = packedRGBA16N.Decode(packedRGBA16N.Z);
            var W = packedRGBA16N.Decode(packedRGBA16N.W);

            return new FVector4(X, Y, Z, W);
        }

    }
}
