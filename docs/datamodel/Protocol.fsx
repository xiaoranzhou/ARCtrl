(**
---
title: Protocol
category: Datamodel
categoryindex: 1
index: 6
---
*)

(*** hide ***)
#r "nuget: Deedle"

open Deedle


let f = Frame.ReadCsv(__SOURCE_DIRECTORY__ + @"\typeDependancies.tab",separators = "\t")

let name = "Procotol"

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
# Procotol

The ISA protocol is a generalized instruction. It describes in detail how to perform an experimental aim but leaves two freedoms:

- The input and output files are not stated
- It contains some open [parameters](ProtoclParameter.html), for which no specific values are given. 

Therefore an ISA protocol `does not` exactly match the usage of the word in the laboratory context. Whereas the protocol 

An applied protocol is a [process](Process.html).


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