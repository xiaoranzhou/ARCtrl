(**
---
title: Investigation
category: Datamodel
categoryindex: 1
index: 2
---
*)

(*** hide ***)
#r "nuget: Deedle"

open Deedle


let f = Frame.ReadCsv(__SOURCE_DIRECTORY__ + @"\typeDependancies.tab",separators = "\t")

let name = "Investigation"

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
# Investigation

The investigation is the biggest entity in the ISA datamodel. It may contain many related experiments.

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

[Investigation.xlsx](https://isa-specs.readthedocs.io/en/latest/isatab.html#investigation-file)
[investigation.json]https://isa-specs.readthedocs.io/en/latest/isajson.html#investigation-schema-json


*)