(**
---
title: Overview
category: Datamodel
categoryindex: 1
index: 1
---
*)

(***hide***)
#r "nuget: CyJS.NET"
#r "nuget: FSharp.FGL"
#r "nuget: Deedle"

open Deedle
open Cyjs.NET
open Elements

// do fsi.AddPrinter(fun (printer:Deedle.Internal.IFsiFormattable) -> "\n" + (printer.Format()))

let f = Frame.ReadCsv(__SOURCE_DIRECTORY__ + @"\typeDependancies.tab",separators = "\t")

let createLink n = sprintf """<a href="%s.html">%s</a>""" n n

let createNode (name,c) = 
    node name [ CyParam.label name; if c = "Record" then CyParam.shape "triangle" else CyParam.shape "square"]

let createEdge (source,target,name,edgeType) =
    let color =
        if edgeType = "Single" then "#6FB1FC"
        elif edgeType = "Many" then "#86B342"
        else "#EDA1ED"
    edge (source + "_" + name) source target [CyParam.label "name"; CyParam.color color]

   
let nodes = 
    f
    |> Frame.rows
    |> Series.values
    |> Seq.map (fun row -> row.GetAs<string>("Source"),row.GetAs<string>("SourceType"))
    |> Seq.toArray
    |> Array.distinct
    |> Array.filter (fun (name,label) ->
        (name <> "Int")
        && (name <> "String")
        && (name <> "Float")
        && (name <> "OntologyAnnotation")
        && (name <> "Remark")
        && (name <> "Comment")
    )

let edges = 
    f
    |> Frame.rows
    |> Series.values
    |> Seq.map (fun row -> row.GetAs<string>("Source"),row.GetAs<string>("Target"),row.GetAs<string>("Name"),row.GetAs<string>("EdgeType"))
    |> Seq.toArray
    |> Array.filter (fun (source,target,_,_) ->
        (Array.exists (fst >> (=) source) nodes)
        &&
        (Array.exists (fst >> (=) target) nodes)
    )

let createGraph nodes edges = 
    CyGraph.initEmpty ()
    |> CyGraph.withElements nodes
    |> CyGraph.withElements edges
    |> CyGraph.withStyle "node"     
        [
            CyParam.shape =. CyParam.shape
            // Style mapper can also be used untyped in string form: CyParam.shape "data(shape)"
            // CyParam.width <=. (CyParam.weight, 40, 80, 20, 60)
            // A linear style mapper like this: CyParam.width "mapData(weight, 40, 80, 20, 60)"
            CyParam.content =. CyParam.label
            CyParam.Text.Align.center
            CyParam.Text.Outline.width 2
            CyParam.Text.Outline.color =. CyParam.color  //=. CyParam.color
            CyParam.Background.color   =. CyParam.color  //=. CyParam.color
            CyParam.color =. CyParam.color
            
        ]  
    |> CyGraph.withStyle "edge"     
                [
                    CyParam.Curve.style "bezier"
                    CyParam.opacity 0.666
                    // CyParam.width <=. (CyParam.weight, 70, 100, 2, 6)
                    CyParam.Target.Arrow.shape "triangle"
                    CyParam.Source.Arrow.shape "circle"
                    CyParam.Line.color =. CyParam.color
                    CyParam.Target.Arrow.color =. CyParam.color
                    CyParam.Source.Arrow.color =. CyParam.color
                ]
    |> CyGraph.withLayout (
            Layout.initBreadthfirst (Layout.LayoutOptions.Cose(ComponentSpacing=40)) 
            )  
    |> CyGraph.withSize(800, 800) 

(**
# Datamodel overview

*)

(*** hide ***)
let cyNodes = 
    nodes
    |> Array.map createNode

let cyEdges = 
    edges
    |> Array.map createEdge

let cyGraph = createGraph cyNodes cyEdges


// (***do-not-eval***)
// cyGraph
// |> CyGraph.show

(*** hide ***)
cyGraph
|> CyGraph.withSize(600, 400) 
|> Cyjs.NET.HTML.toEmbeddedHTML
(*** include-it-raw ***)  

(**
# Types covered in Investigation.xlsx file

*)

(*** hide ***)
let includedInInvestigation = 
    [
        "Investigation"
        "Study"
        "Publication"
        "Assay"
        "Person"
        "Protocol"
        "Factor"
        "OntologySourceReference"
    ]
    |> set

let createInvNode (name,c) = 
    let color = if includedInInvestigation.Contains name then "#6FB1FC" else "#999999"
    node name [ CyParam.label name; if c = "Record" then CyParam.shape "triangle" else CyParam.shape "square"; CyParam.color color; CyParam.weight 60]

let invNodes = 
    nodes
    |> Array.map createInvNode

let invEdges = 
    edges
    |> Array.map createEdge

let invGraph = createGraph invNodes invEdges


// (***do-not-eval***)
// invGraph
// |> CyGraph.show
invGraph
|> CyGraph.show

invGraph
|> CyGraph.withSize(600, 400) 
|> Cyjs.NET.HTML.toEmbeddedHTML
(*** include-it-raw ***)  


(**
# Types covered in Assay.xlsx file

*)

(*** hide ***)
let includedInAssay = 
    [
        "Assay"
        "Process"
        "ProcessParameterValue"
        "ProtocolParameter"
        "Factor"
        "FactorValue"
        "MaterialAttribute"
        "MaterialAttributeValue"
        "Value"
        "Sample"
        "Source"
        "ProcessInput"
        "ProcessOutput"
    ]
    |> set

let createAssNode (name,c) = 
    let color = if includedInAssay.Contains name then "#6FB1FC" else "#999999"
    node name [ CyParam.label name; if c = "Record" then CyParam.shape "triangle" else CyParam.shape "square"; CyParam.color color]

let assNodes = 
    nodes
    |> Array.map createInvNode

let assEdges = 
    edges
    |> Array.map createEdge

let assGraph = createGraph assNodes assEdges


// (***do-not-eval***)
// cyGraph
// |> CyGraph.show


assGraph
|> CyGraph.withSize(600, 400) 
|> Cyjs.NET.HTML.toEmbeddedHTML
(*** include-it-raw ***)  