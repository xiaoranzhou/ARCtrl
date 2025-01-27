﻿namespace ARCtrl.Contract

open ARCtrl
open ARCtrl.Path
open ARCtrl.ISA.Spreadsheet
open ARCtrl.ISA



[<AutoOpen>]
module ArcTypeExtensions = 

    let (|StudyPath|_|) (input) =
        match input with
        | [|StudiesFolderName; anyStudyName; StudyFileName|] -> 
            let path = ARCtrl.Path.combineMany input
            Some path
        | _ -> None

    let (|AssayPath|_|) (input) =
        match input with
        | [|AssaysFolderName; anyAssayName; AssayFileName|] -> 
            let path = ARCtrl.Path.combineMany input
            Some path
        | _ -> None

    let (|InvestigationPath|_|) (input) =
        match input with
        | [|InvestigationFileName|] -> 
            let path = ARCtrl.Path.combineMany input
            Some path
        | _ -> None

    type ArcInvestigation with

        member this.ToCreateContract () =
            let path = InvestigationFileName
            let c = Contract.createCreate(path, DTOType.ISA_Investigation, DTO.Spreadsheet (this |> ArcInvestigation.toFsWorkbook))
            c

        member this.ToUpdateContract () =
            let path = InvestigationFileName
            let c = Contract.createUpdate(path, DTOType.ISA_Investigation, DTO.Spreadsheet (this |> ArcInvestigation.toFsWorkbook))
            c

        //member this.ToDeleteContract () =
        //    let path = InvestigationFileName
        //    let c = Contract.createDelete(path)
        //    c
        
        //static member toDeleteContract () : Contract =
        //    let path = InvestigationFileName
        //    let c = Contract.createDelete(path)
        //    c

        static member toCreateContract (inv: ArcInvestigation) : Contract =
            inv.ToCreateContract()

        static member toUpdateContract (inv: ArcInvestigation) : Contract =
            inv.ToUpdateContract()

        static member tryFromReadContract (c:Contract) =
            match c with
            | {Operation = READ; DTOType = Some DTOType.ISA_Investigation; DTO = Some (DTO.Spreadsheet fsworkbook)} ->
                fsworkbook
                |> ArcInvestigation.fromFsWorkbook
                |> Some 
            | _ -> None
      

    type ArcStudy with

        member this.ToCreateContract () =
            let path = Identifier.Study.fileNameFromIdentifier this.Identifier
            let c = Contract.createCreate(path, DTOType.ISA_Study, DTO.Spreadsheet (this |> ArcStudy.toFsWorkbook))
            c

        member this.ToUpdateContract () =
            let path = Identifier.Study.fileNameFromIdentifier this.Identifier
            let c = Contract.createUpdate(path, DTOType.ISA_Study, DTO.Spreadsheet (this |> ArcStudy.toFsWorkbook))
            c

        member this.ToDeleteContract () =
            let path = Path.getStudyFolderPath(this.Identifier)
            let c = Contract.createDelete(path)
            c

        static member toDeleteContract (study: ArcStudy) : Contract =
            study.ToDeleteContract()

        static member toCreateContract (study: ArcStudy) : Contract =
            study.ToCreateContract()

        static member toUpdateContract (study: ArcStudy) : Contract =
            study.ToUpdateContract()           

        static member tryFromReadContract (c:Contract) =
            match c with
            | {Operation = READ; DTOType = Some DTOType.ISA_Study; DTO = Some (DTO.Spreadsheet fsworkbook)} ->
                fsworkbook
                |> ArcStudy.fromFsWorkbook
                |> Some 
            | _ -> None

    type ArcAssay with

        member this.ToCreateContract () =
            let path = Identifier.Assay.fileNameFromIdentifier this.Identifier
            let c = Contract.createCreate(path, DTOType.ISA_Assay, DTO.Spreadsheet (this |> ArcAssay.toFsWorkbook))
            c

        member this.ToUpdateContract () =
            let path = Identifier.Assay.fileNameFromIdentifier this.Identifier
            let c = Contract.createUpdate(path, DTOType.ISA_Assay, DTO.Spreadsheet (this |> ArcAssay.toFsWorkbook))
            c

        member this.ToDeleteContract () =
            let path = Path.getAssayFolderPath(this.Identifier)
            let c = Contract.createDelete(path)
            c

        static member toDeleteContract (assay: ArcAssay) : Contract =
            assay.ToDeleteContract()

        static member toCreateContract (assay: ArcAssay) : Contract =
            assay.ToCreateContract()

        static member toUpdateContract (assay: ArcAssay) : Contract =
            assay.ToUpdateContract()

        static member tryFromReadContract (c:Contract) =
            match c with
            | {Operation = READ; DTOType = Some DTOType.ISA_Assay; DTO = Some (DTO.Spreadsheet fsworkbook)} ->
                fsworkbook
                |> ArcAssay.fromFsWorkbook
                |> Some 
            | _ -> None
