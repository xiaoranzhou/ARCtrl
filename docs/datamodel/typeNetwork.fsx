(***hide***)
#r "nuget: ISADotNet"
//#r "nuget: CyJS.NET"
//#r "nuget: FSharp.FGL"
#r "nuget: Deedle"

(*** do-not-eval ***)

open ISADotNet
open System.Text.RegularExpressions
open System.Reflection
open Deedle

// do fsi.AddPrinter(fun (printer:Deedle.Internal.IFsiFormattable) -> "\n" + (printer.Format()))

let assembly = Assembly.Load(AssemblyName("ISADotNet"))

let m = assembly.Modules |> Seq.head

let types = m.GetTypes() |> Seq.toArray

let factor = 
    types
    |> Array.find (fun t -> t.Name = "Factor")

let materialValue = 
    types
    |> Array.find (fun t -> t.Name = "MaterialType")


let getTypeOfPropery (p : PropertyInfo) = 
    let n = p.PropertyType.FullName
    let pattern = @"(?<=\[\[).*(?=\]\])"
    if n.Contains "FSharpList" then
        let intern = Regex.Match(n,pattern).Value
        p.Name,"Many",Regex.Match(intern,@"(?<=\[).*(?=\])").Value.Split(',').[0].Split('.') |> Array.last
    else 
        p.Name,"Single",Regex.Match(n,pattern).Value.Split(',').[0].Split('.') |> Array.last

let getTypeOfCase (p : Reflection.UnionCaseInfo) = 
    let fields = p.GetFields()
    p.Name,"Case",
    if fields.Length = 0 then "Unit"
    else fields.[0].PropertyType.Name


let hasIISAPrintable (t : System.Type) =
    t.GetInterfaces()
    |> Array.exists (fun x -> x.Name = "IISAPrintable")

let hasCreateMethod (t : System.Type) =
    t.GetMethods()
    |> Array.exists (fun met -> met.Name = "create")

let network = 
    types 
    |> Array.filter (fun t -> hasCreateMethod t || hasIISAPrintable t
    )
    |> Array.filter (fun t -> t.FullName.Contains "+" |> not
    )
    |> Array.map (fun t ->     
        if FSharp.Reflection.FSharpType.IsRecord t then
            t.Name,"Record",
            FSharp.Reflection.FSharpType.GetRecordFields t
            |> Array.map (getTypeOfPropery)
        else
            t.Name,"Union",
            FSharp.Reflection.FSharpType.GetUnionCases t
            |> Array.map (getTypeOfCase)
    )



let data = 
    types
    |> Array.find (fun t -> t.Name = "Data")

types
|> Array.filter (fun t -> t.Name = "Data")
|> Array.map (fun t -> t.FullName)

//let case =
//    value
//    |> FSharp.Reflection.FSharpType.GetUnionCases
//    |> Array.head

//case.GetFields().[0].PropertyType.Name

//|> Array.map (getTypeOfCase)

data
|> fun t -> 
    if FSharp.Reflection.FSharpType.IsRecord t then
        t.Name,"Record",
        FSharp.Reflection.FSharpType.GetRecordFields t
        |> Array.map (getTypeOfPropery)
    else
        t.Name,"Union",
        FSharp.Reflection.FSharpType.GetUnionCases t
        |> Array.map (getTypeOfCase)
|> fun (source,c,edges) ->
    edges
    |> Array.map (fun (name,t,target) ->
        [
            "Source",source
            "SourceType",c
            "Name",name
            "EdgeType",t
            "Target",target
        ]
        |> series
    )   



let f = 
    network
    |> Array.collect (fun (source,c,edges) ->
        edges
        |> Array.map (fun (name,t,target) ->
            [
                "Source",source
                "SourceType",c
                "Name",name
                "EdgeType",t
                "Target",target
            ]
            |> series
        )   
    )
    |> Array.indexed
    |> Frame.ofRows

f
|> Frame.filterRows (fun _ os ->
    os.GetAs<string> "Source" = "Data"
)

network


f.SaveCsv(__SOURCE_DIRECTORY__ + @"\typeDependancies.tab",separator = '\t')