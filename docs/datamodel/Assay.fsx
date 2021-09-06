(**
---
title: Assay
category: Datamodel
categoryindex: 1
index: 4
---
*)

(*** hide ***)
#r "nuget: Deedle"

open Deedle


let f = Frame.ReadCsv(__SOURCE_DIRECTORY__ + @"\typeDependancies.tab",separators = "\t")

let name = "Assay"

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
# Assay

The assay is at the core of both the ISA datamodel and the ARC. It describes in detail what is necessary and the steps made to get from raw material to a final sample or data files.

These steps are grouped into [processes](Process.html).


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
[assay.json](https://isa-specs.readthedocs.io/en/latest/isajson.html#assay-schema-json)


*)