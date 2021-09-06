(**
---
title: ProcessParameterValue
category: datamodel
categoryindex: 1
index: 7
---
*)

(*** hide ***)
#r "nuget: Deedle"

open Deedle


let f = Frame.ReadCsv(__SOURCE_DIRECTORY__ + @"\typeDependancies.tab",separators = "\t")

let name = "ProcessParameterValue"

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
# ProcessParameterValue

The process parameter value is an applied [protocol parameter](ProtoclParameter.html). 

It describes what specific [value](Value.html) was set for a parameter in a given [Process](Process.html). 

Process parameter values describe the execution of a process, just like [factor values](FactorValue.html). But unlike factor values, parameter values are not the main variable of interest in the experiment.

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
[process_parameter_value.json](https://isa-specs.readthedocs.io/en/latest/isajson.html#process-parameter-value-schema-json)


*)