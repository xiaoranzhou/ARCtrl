namespace ISADotNet.Json


#if FABLE_COMPILER
open Thoth.Json
#else
open Thoth.Json.Net
#endif
open ISADotNet
open System.IO
open GEncode

module Comment = 
    
    let genID (c:Comment) = 
        match c.ID with
        | Some id -> URI.toString id
        | None -> match c.Name with
                  | Some n -> "#Comment_" + n.Replace(" ","_") + if c.Value.IsSome then "_" + c.Value.Value.Replace(" ","_") else ""
                  | None -> "#EmptyComment"

    let encoder (options : ConverterOptions) (comment : obj) = 
        if options.IsRoCrate then
            let c = comment :?> Comment
            match c.Name,c.Value with
            | Some n, Some v -> GEncode.string (n+":"+v)
            | Some n, None -> GEncode.string n
            | None, Some v -> GEncode.string v
            | _ -> GEncode.string ""
        else
            [
                if options.SetID then "@id", GEncode.string (comment :?> Comment |> genID)
                    else tryInclude "@id" GEncode.string (comment |> tryGetPropertyValue "ID")
                if options.IncludeType then "@type", GEncode.string "Comment"
                tryInclude "name" GEncode.string (comment |> tryGetPropertyValue "Name")
                tryInclude "value" GEncode.string (comment |> tryGetPropertyValue "Value")
            ]
            |> GEncode.choose
            |> List.append (if options.IncludeContext then [("@context",Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText("/home/wetzels/arc/ISADotNet_public/src/ISADotNet.Json/context/sdo/isa_comment_sdo_context.jsonld")).GetValue("@context"))] else [])
            |> Encode.object

    let decoder (options : ConverterOptions) : Decoder<Comment> =
        Decode.object (fun get ->
            {
                ID = get.Optional.Field "@id" GDecode.uri
                Name = get.Optional.Field "name" Decode.string
                Value = get.Optional.Field "value" Decode.string
            }
        )

    let fromString (s:string)  = 
        GDecode.fromString (decoder (ConverterOptions())) s

    let toString (c:Comment) = 
        encoder (ConverterOptions()) c
        |> Encode.toString 2

    /// exports in json-ld format
    let toStringLD (c:Comment) = 
        encoder (ConverterOptions(SetID=true,IncludeType=true)) c
        |> Encode.toString 2
    let toStringLDWithContext (a:Comment) = 
        encoder (ConverterOptions(SetID=true,IncludeType=true,IncludeContext=true)) a
        |> Encode.toString 2

    //let fromFile (path : string) = 
    //    File.ReadAllText path 
    //    |> fromString

    //let toFile (path : string) (c:Comment) = 
    //    File.WriteAllText(path,toString c)
