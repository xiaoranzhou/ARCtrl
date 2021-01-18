namespace ISADotNet.XSLX

open DocumentFormat.OpenXml.Spreadsheet
open FSharpSpreadsheetML
open ISADotNet
open Comment
open Remark
open System.Collections.Generic

module Assays = 

    let measurementTypeLabel =                    "Measurement Type"
    let measurementTypeTermAccessionNumberLabel = "Measurement Type Term Accession Number"
    let measurementTypeTermSourceREFLabel =       "Measurement Type Term Source REF"
    let technologyTypeLabel =                     "Technology Type"
    let technologyTypeTermAccessionNumberLabel =  "Technology Type Term Accession Number"
    let technologyTypeTermSourceREFLabel =        "Technology Type Term Source REF"
    let technologyPlatformLabel =                 "Technology Platform"
    let fileNameLabel =                           "File Name"

    let labels = 
        [
        measurementTypeLabel;measurementTypeTermAccessionNumberLabel;measurementTypeTermSourceREFLabel;
        technologyTypeLabel;technologyTypeTermAccessionNumberLabel;technologyTypeTermSourceREFLabel;technologyPlatformLabel;fileNameLabel
        ]

    
    let fromString measurementType measurementTypeTermAccessionNumber measurementTypeTermSourceREF technologyType technologyTypeTermAccessionNumber technologyTypeTermSourceREF technologyPlatform fileName comments =
        let measurementType = OntologyAnnotation.fromString measurementType measurementTypeTermAccessionNumber measurementTypeTermSourceREF
        let technologyType = OntologyAnnotation.fromString technologyType technologyTypeTermAccessionNumber technologyTypeTermSourceREF
        Assay.create null fileName measurementType technologyType technologyPlatform [] {Samples=[];OtherMaterials=[]} [] [] [] comments
        
    let fromSparseMatrix (matrix : SparseMatrix) =
        
        List.init matrix.Length (fun i -> 

            let comments = 
                matrix.CommentKeys 
                |> List.map (fun k -> 
                    Comment.fromString k (matrix.TryGetValueDefault("",(k,i))))

            fromString
                (matrix.TryGetValueDefault("",(measurementTypeLabel,i)))             
                (matrix.TryGetValueDefault("",(measurementTypeTermAccessionNumberLabel,i)))
                (matrix.TryGetValueDefault("",(measurementTypeTermSourceREFLabel,i)))
                (matrix.TryGetValueDefault("",(technologyTypeLabel,i)))               
                (matrix.TryGetValueDefault("",(technologyTypeTermAccessionNumberLabel,i))) 
                (matrix.TryGetValueDefault("",(technologyTypeTermSourceREFLabel,i)))   
                (matrix.TryGetValueDefault("",(technologyPlatformLabel,i)))     
                (matrix.TryGetValueDefault("",(fileNameLabel,i)))                    
                comments
        )

    let toSparseMatrix (assays: Assay list) =
        let matrix = SparseMatrix.Create (keys = labels,length=assays.Length)
        let mutable commentKeys = []
        assays
        |> List.iteri (fun i a ->
            let measurementType,measurementAccession,measurementSource = OntologyAnnotation.toString a.MeasurementType
            let technologyType,technologyAccession,technologySource = OntologyAnnotation.toString a.TechnologyType
            do matrix.Matrix.Add ((measurementTypeLabel,i),                       measurementType)
            do matrix.Matrix.Add ((measurementTypeTermAccessionNumberLabel,i),    measurementAccession)
            do matrix.Matrix.Add ((measurementTypeTermSourceREFLabel,i),          measurementSource)
            do matrix.Matrix.Add ((technologyTypeLabel,i),                        technologyType)
            do matrix.Matrix.Add ((technologyTypeTermAccessionNumberLabel,i),     technologyAccession)
            do matrix.Matrix.Add ((technologyTypeTermSourceREFLabel,i),           technologySource)
            do matrix.Matrix.Add ((technologyPlatformLabel,i),                    a.TechnologyPlatform)
            do matrix.Matrix.Add ((fileNameLabel,i),                              a.FileName)

            a.Comments
            |> List.iter (fun comment -> 
                commentKeys <- comment.Name :: commentKeys
                matrix.Matrix.Add((comment.Name,i),comment.Value)
            )      
        )
        {matrix with CommentKeys = commentKeys |> List.distinct}

    let readAssays (prefix : string) lineNumber (en:IEnumerator<Row>) =
        let rec loop (matrix : SparseMatrix) remarks lineNumber = 

            if en.MoveNext() then  
                let row = en.Current |> Row.getIndexedValues None |> Seq.map (fun (i,v) -> int i - 1,v)
                match Seq.tryItem 0 row |> Option.map snd, Seq.trySkip 1 row with

                | Comment k, Some v -> 
                    loop (SparseMatrix.AddComment k v matrix) remarks (lineNumber + 1)

                | Remark k, _  -> 
                    loop matrix (Remark.create lineNumber k :: remarks) (lineNumber + 1)

                | Some k, Some v when List.exists (fun label -> k = prefix + " " + label) labels -> 
                    let label = List.find (fun label -> k = prefix + " " + label) labels
                    loop (SparseMatrix.AddRow label v matrix) remarks (lineNumber + 1)

                | Some k, _ -> Some k,lineNumber,remarks,fromSparseMatrix matrix
                | _ -> None, lineNumber,remarks,fromSparseMatrix matrix
            else
                None,lineNumber,remarks,fromSparseMatrix matrix
        loop (SparseMatrix.Create()) [] lineNumber

    
    let writeAssays prefix (assays : Assay list) =
        assays
        |> toSparseMatrix
        |> SparseMatrix.ToRows prefix