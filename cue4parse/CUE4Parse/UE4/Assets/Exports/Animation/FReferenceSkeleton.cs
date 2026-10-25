using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Objects.Core.Math;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;

namespace CUE4Parse.UE4.Assets.Exports.Animation;

[JsonConverter(typeof(FReferenceSkeletonConverter))]
public class FReferenceSkeleton
{
    public readonly FMeshBoneInfo[] FinalRefBoneInfo;
    public readonly FTransform[] FinalRefBonePose;
    public readonly Dictionary<string, int> FinalNameToIndexMap;

    public FReferenceSkeleton(FAssetArchive Ar, FVirtualBone[]? virtualBones = null)
    {
        FinalRefBoneInfo = Ar.ReadArray(() => new FMeshBoneInfo(Ar));
        if (Ar.Game < GAME_UE4_0)
        {
            FinalRefBonePose = new FTransform[FinalRefBoneInfo.Length];
            for (int i = 0; i < FinalRefBoneInfo.Length; i++)
            {
                FinalRefBonePose[i] = new FTransform(FinalRefBoneInfo[i].BonePos.Orientation, FinalRefBoneInfo[i].BonePos.Position, FVector.OneVector);
            }
        }
        else
        {
            FinalRefBonePose = Ar.ReadArray(() => new FTransform(Ar));
        }

        FinalNameToIndexMap = Ar.Ver >= EUnrealEngineObjectUE4Version.REFERENCE_SKELETON_REFACTOR ? Ar.ReadMap(() => Ar.ReadFName().Text, Ar.Read<int>) : [];

        if (Ar.Game == GAME_DaysGone) Ar.SkipFixedArray(12);

        if (Ar.Ver < EUnrealEngineObjectUE4Version.FIXUP_ROOTBONE_PARENT)
        {
            if (FinalRefBoneInfo.Length > 0 && FinalRefBoneInfo[0].ParentIndex != -1)
            {
                FinalRefBoneInfo[0] = new FMeshBoneInfo(FinalRefBoneInfo[0].Name, -1);
            }
        }

        if (Ar.Game == GAME_WutheringWaves)
        {
            Ar.SkipFixedArray(12);
            Ar.Position += 4;
        }

        // The serialized arrays are UE's raw reference skeleton. Cooked
        // animation track indices address the rebuilt skeleton, including
        // virtual bones (FReferenceSkeleton::RebuildRefSkeleton).
        if (virtualBones is { Length: > 0 })
        {
            var info = FinalRefBoneInfo.ToList();
            var poses = FinalRefBonePose.ToList();
            var indices = info.Select((bone, index) => (bone.Name.Text, index))
                .ToDictionary(item => item.Text, item => item.index, StringComparer.OrdinalIgnoreCase);
            var componentPoses = new List<FTransform>(info.Count + virtualBones.Length);
            for (var index = 0; index < info.Count; index++)
            {
                var parent = info[index].ParentIndex;
                componentPoses.Add(parent < 0 ? poses[index] : poses[index] * componentPoses[parent]);
            }
            foreach (var bone in virtualBones)
            {
                if (!indices.TryGetValue(bone.SourceBoneName.Text, out var parent)
                    || !indices.TryGetValue(bone.TargetBoneName.Text, out var target))
                {
                    // UE omits virtual bones whose endpoints are absent.
                    continue;
                }
                var source = parent;
                foreach (var other in virtualBones)
                {
                    if (other.VirtualBoneName == bone.SourceBoneName)
                    {
                        source = indices[other.TargetBoneName.Text];
                        break;
                    }
                }
                var pose = componentPoses[target].GetRelativeTransform(componentPoses[source]);
                indices.Add(bone.VirtualBoneName.Text, info.Count);
                info.Add(new FMeshBoneInfo(bone.VirtualBoneName, parent));
                poses.Add(pose);
                componentPoses.Add(pose * componentPoses[parent]);
            }
            FinalRefBoneInfo = info.ToArray();
            FinalRefBonePose = poses.ToArray();
            FinalNameToIndexMap = indices;
        }
    }
}
