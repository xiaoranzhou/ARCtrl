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

let f = 
    Frame.ReadCsv(__SOURCE_DIRECTORY__ + @"\typeDependancies.tab",separators = "\t")
    |> Frame.filterRows (fun _ os -> 
        os.GetAs<string> "Source" <> "Remark" 
        && os.GetAs<string> "Name" <> "Remarks" 
        && os.GetAs<string> "Source" <> "Comment" 
        && os.GetAs<string> "Source" <> "OntologyAnnotation" 
    )

let createLink n = sprintf """<a href="%s.html">%s</a>""" n n

let createNode (name,label,c) = 
    let shape,color = if c = "Record" then "triangle","#6FB1FC" elif c = "Union" then "square","#6FB1FC" else "square","#999999"
    node name [ CyParam.label label; CyParam.shape shape; CyParam.color color; CyParam.weight 60]

let createEdge ((source,sourceType),(target,targetType),edgeType) =
    let color =
        if edgeType = "Single" then "#6FB1FC"
        elif edgeType = "Many" then "#86B342"
        else "#EDA1ED"
    edge (source + "_" + targetType) source target [CyParam.label targetType; CyParam.color color]

   
let edges = 
    f
    |> Frame.rows
    |> Series.values
    |> Seq.map (fun row -> 
        let source = row.GetAs<string>("Source")
        let sourceType = row.GetAs<string>("SourceType")
        let edgeType = row.GetAs<string>("EdgeType")
        let target = row.GetAs<string>("Target")
        let name =
            if 
                (target = "Int")
                || (target = "String")
                || (target = "Float")
                || (target = "OntologyAnnotation")
                || (target = "Remark")
                || (target = "Comment")
            then
                source + row.GetAs<string>("Name"),row.GetAs<string>("Name")
            else target,target

        (source,sourceType),
        name,
        edgeType
    )
    |> Seq.toArray

let nodes = 
    let mains = 
        edges
        |> Array.map (fun ((s,st),(t,tt),et) ->
            s,s,st
        )
        |> Array.distinct
    let secondarys = 
        edges
        |> Array.choose (fun ((s,st),(t,tt),et) ->
            if Array.exists (fun (a,b,c) -> a = t) mains then
                None
            else
                Some (t,tt,"Minor")
        )
        |> Array.distinct
    Array.append mains secondarys

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
            CyParam.Text.Outline.color "#000000"
            CyParam.Text.Outline.width 0.8
            CyParam.weight 100
            //CyParam.Text.Outline.color =. CyParam.color  //=. CyParam.color
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
            Layout.initBreadthfirst (Layout.LayoutOptions.Cose((*ComponentSpacing=40*))) 
            )  
    //|> CyGraph.withSize(800, 800) 

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
cyGraph
|> CyGraph.withSize(3000, 3000) 
|> CyGraph.show

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