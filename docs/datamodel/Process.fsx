(**
---
title: Process
category: Datamodel
categoryindex: 1
index: 5
---
*)

(*** hide ***)
#r "nuget: Deedle"

open Deedle


let f = Frame.ReadCsv(__SOURCE_DIRECTORY__ + @"\typeDependancies.tab",separators = "\t")

let name = "Process"

let partOf = 
    f
    |> Frame.rows
    |> Series.values
    |> Seq.filter (fun row -> row.GetAs<string>("Target") = name)
    |> Seq.map (fun row -> row.GetAs<string>("Source"), row.GetAs<string> "Name",row.GetAs<string>("EdgeType"))
    |> Seq.toArray


let hasParts = 
    f
    |> Frame.rows
    |> Series.values
    |> Seq.filter (fun row -> row.GetAs<string>("Source") = name)
    |> Seq.map (fun row -> row.GetAs<string>("Target"), row.GetAs<string> "Name",row.GetAs<string>("EdgeType"))
    |> Seq.toArray


(**
# Process

The process is an applied [protocol](Protocol.html). 
For a given protocol with generic [parameters](ProtoclParameter.html), specific [values](ProcessParameterValue.html) are set and which [inputs](ProcessInput) were mapped to which [outputs](ProcessOutput).

Processes can be grouped to an [assay](Assay.html).


## Has parts:

*)
(*** hide ***)
hasParts
|> Array.map (fun (name,caseName,e) -> sprintf """%s %s:<a href="%s.html">%s</a>""" e caseName name name)
|> Array.reduce (fun a b -> a + "<br>" + b)

(*** include-it-raw ***)

(** 
## Is part of

*)

(*** hide ***)
partOf
|> Array.map (fun (name,caseName,e) -> sprintf """%s %s:<a href="%s.html">%s</a>""" e caseName name name)
|> Array.reduce (fun a b -> a + "<br>" + b)

(*** include-it-raw ***)


(**
## Appears in file

[Assay.xlsx](https://isa-specs.readthedocs.io/en/latest/isatab.html#study-and-assay-files)
[Investigation.xlsx](https://isa-specs.readthedocs.io/en/latest/isatab.html#investigation-file)
[process.json](https://isa-specs.readthedocs.io/en/latest/isajson.html#process-schema-json)


*)