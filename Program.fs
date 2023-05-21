open System.Reflection.Metadata
open System.Reflection.PortableExecutable
open System.IO
[<assembly: System.Runtime.CompilerServices.InternalsVisibleTo("myFriend")>] do ()

let getFriends dll =
    let decoder = { // inspired by a similar thing in https://github.com/dafny-lang/dafny
        new ICustomAttributeTypeProvider<System.Type> with
            member this.GetPrimitiveType(typeCode: PrimitiveTypeCode) =
                if typeCode = PrimitiveTypeCode.String then typeof<string> else failwith "not impl"
            member this.GetTypeFromDefinition(_, _, _) = failwith "not impl"
            member this.GetTypeFromReference(_, _, _) = failwith "not impl"
            member this.GetSZArrayType _ = failwith "not impl"
            member this.GetSystemType() = failwith "not impl"
            member this.GetTypeFromSerializedName _ = failwith "not impl"
            member this.GetUnderlyingEnumType _ = failwith "not impl"
            member this.IsSystemType _ = failwith "not impl"
    }
    use fs = new FileStream(dll, FileMode.Open, FileAccess.Read)
    use pr = new PEReader(fs)
    let mr = pr.GetMetadataReader()
    let customAttributes = mr.GetAssemblyDefinition().GetCustomAttributes() |> Seq.map mr.GetCustomAttribute
    let tryGetFriend (ca: CustomAttribute) =
            let mref = MemberReferenceHandle.op_Explicit ca.Constructor |> mr.GetMemberReference
            let tref = TypeReferenceHandle.op_Explicit mref.Parent |> mr.GetTypeReference
            let name = tref.Name |> mr.GetString
            if name <> "InternalsVisibleToAttribute" then None
            else (ca.DecodeValue decoder).FixedArguments[0].Value :?> string |> Some
    customAttributes |> Seq.choose tryGetFriend |> Seq.iter (printfn "%s")

getFriends "./bin/Debug/net7.0/ReadMetadataAttributeArgs.dll"
