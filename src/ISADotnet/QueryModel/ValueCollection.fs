﻿namespace ISADotNet.QueryModel

open ISADotNet
open System.Text.Json.Serialization
open System.Text.Json
open System.IO

open System.Collections.Generic
open System.Collections

type IOValueCollection(values : KeyValuePair<string*string,ISAValue> list) =

    member this.First = values.Head

    member this.Last = values.[values.Length - 1]

    member this.Item(i : int)  = values.[i]

    member this.Item(ioKey : string*string) = values |> List.pick (fun kv -> if ioKey = kv.Key then Some kv.Value else None)

    member this.Item(category : OntologyAnnotation) = values |> List.pick (fun kv -> if kv.Value.Category = category then Some kv.Key else None)

    member this.Characteristics(?Name) = 
        values
        |> List.filter (fun kv -> 
            match Name with 
            | Some name -> 
                kv.Value.IsCharacteristicValue && kv.Value.NameText = name
            | None -> 
                kv.Value.IsCharacteristicValue
        )
        |> IOValueCollection

    member this.Parameters(?Name) = 
        values
        |> List.filter (fun kv -> 
            match Name with 
            | Some name -> 
                kv.Value.IsParameterValue && kv.Value.NameText = name
            | None -> 
                kv.Value.IsParameterValue
        )
        |> IOValueCollection

    member this.Factors(?Name) = 
        values
        |> List.filter (fun kv -> 
            match Name with 
            | Some name -> 
                kv.Value.IsFactorValue && kv.Value.NameText = name
            | None -> 
                kv.Value.IsFactorValue
        )
        |> IOValueCollection

    member this.WithCategory(category : OntologyAnnotation) = 
        values
        |> List.filter (fun kv -> kv.Value.Category = category)
        |> IOValueCollection

    member this.WithName(name : string) = 
        values
        |> List.filter (fun kv -> kv.Value.Category.NameText = name)
        |> IOValueCollection

    member this.GroupBySource =
        values
        |> List.groupBy (fun kv -> fst kv.Key)
        |> List.map (fun (source,vals) -> source, vals |> List.map (fun kv -> snd kv.Key,kv.Value))

    member this.GroupBySink =
        values
        |> List.groupBy (fun kv -> snd kv.Key)
               |> List.map (fun (sink,vals) -> sink, vals |> List.map (fun kv -> fst kv.Key,kv.Value))
   
    interface IEnumerable<KeyValuePair<string*string,ISAValue>> with
        member this.GetEnumerator() = (Seq.ofList values).GetEnumerator()

    interface IEnumerable with
        member this.GetEnumerator() = (this :> IEnumerable<KeyValuePair<string*string,ISAValue>>).GetEnumerator() :> IEnumerator