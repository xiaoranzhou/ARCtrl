﻿namespace ISA

open Fable.Core

module ArcAssayAux =

    let getNextAutoGeneratedTableName (existingNames: seq<string>) =
        let findNextNumber (numbers: int list) =
            let rec findNext current = function
                | [] -> current + 1
                | x::xs when x = current + 1 -> findNext x xs
                | _ -> current + 1

            match numbers with
            | [] -> 0
            | x::xs -> findNext x xs
        let existingNumbers = existingNames |> Seq.choose (fun x -> match x with | Regex.ActivePatterns.AutoGeneratedTableName n -> Some n| _ -> None) 
        let nextNumber =
            if Seq.isEmpty existingNumbers then
                1
            else
                existingNumbers
                |> Seq.sort
                |> List.ofSeq
                |> findNextNumber
        ArcTable.init($"New Table {nextNumber}")

    let tryByTableName (name: string) (tables: ResizeArray<ArcTable>) =
        match Seq.tryFindIndex (fun t -> t.Name = name) tables with
        | Some index -> index
        | None -> failwith $"Unable to find table with name '{name}'!"

    module SanityChecks =
        

        let validateSheetIndex (index: int) (allowAppend: bool) (sheets: ResizeArray<ArcTable>) =
            let eval x y = if allowAppend then x > y else x >= y
            if index < 0 then failwith "Cannot insert ArcTable at index < 0."
            if eval index sheets.Count then failwith $"Specified index is out of range! Assay contains only {sheets.Count} tables."

        let validateNamesUnique (names:seq<string>) =
            let isDistinct = (Seq.length names) = (Seq.distinct names |> Seq.length)
            if not isDistinct then 
                failwith "Cannot add multiple tables with the same name! Table names inside one assay must be unqiue"

        let validateNewNameUnique (newName:string) (existingNames:seq<string>) =
            match Seq.tryFindIndex (fun x -> x = newName) existingNames with
            | Some i ->
                failwith $"Cannot create table with name {newName}, as table names must be unique and table at index {i} has the same name."
            | None ->
                ()

        let validateNewNamesUnique (newNames:seq<string>) (existingNames:seq<string>) =
            validateNamesUnique newNames
            let setNew = Set.ofSeq newNames
            let setOld = Set.ofSeq existingNames
            let same = Set.intersect setNew setOld
            if not same.IsEmpty then
                failwith $"Cannot create tables with the names {same}, as table names must be unique."

open ArcAssayAux

// "MyAssay"; "assays/MyAssay/isa.assay.xlsx"

[<AttachMembers>]
type ArcAssay = 

    {
        ID : URI option
        FileName : string option
        MeasurementType : OntologyAnnotation option
        TechnologyType : OntologyAnnotation option
        TechnologyPlatform : string option
        Tables : ResizeArray<ArcTable>
        Performers : Person list option
        Comments : Comment list option
    }
   
    static member make 
        (id : URI option)
        (fleName : string option)
        (measurementType : OntologyAnnotation option)
        (technologyType : OntologyAnnotation option)
        (technologyPlatform : string option)
        (tables : ResizeArray<ArcTable>)
        (performers : Person list option)
        (comments : Comment list option) = 
        {
            ID = id
            FileName = fleName
            MeasurementType = measurementType
            TechnologyType = technologyType
            TechnologyPlatform = technologyPlatform
            Tables = tables
            Performers = performers
            Comments = comments
        }

    member this.TableCount 
        with get() = this.Tables.Count

    member this.TableNames 
        with get() = 
            [for s in this.Tables do yield s.Name]

    [<NamedParams>]
    static member create (?ID : URI, ?FileName : string, ?MeasurementType : OntologyAnnotation, ?TechnologyType : OntologyAnnotation, ?TechnologyPlatform : string, ?Sheets : ResizeArray<ArcTable>, ?Performers : Person list, ?Comments : Comment list) = 
        let Sheets = ResizeArray()
        ArcAssay.make ID FileName MeasurementType TechnologyType TechnologyPlatform Sheets Performers Comments


    // - Table API - //
    // remark should this return ArcTable?
    member this.AddTable(?table:ArcTable, ?index: int) = 
        let index = defaultArg index this.TableCount
        let table =
            let createName() = getNextAutoGeneratedTableName this.TableNames
            defaultArg table (createName())
        SanityChecks.validateSheetIndex index true this.Tables
        SanityChecks.validateNewNameUnique table.Name this.TableNames
        this.Tables.Insert(index, table)

    member this.AddTables(tables:seq<ArcTable>, ?index: int) = 
        let index = defaultArg index this.TableCount
        SanityChecks.validateSheetIndex index true this.Tables
        SanityChecks.validateNewNamesUnique (tables |> Seq.map (fun x -> x.Name)) this.TableNames
        this.Tables.InsertRange(index, tables)


    // - Table API - //
    member this.GetTableAt(index:int) : ArcTable =
        SanityChecks.validateSheetIndex index false this.Tables
        this.Tables.[index]

    static member getTableAt(index:int) : ArcAssay -> ArcTable =
        fun (assay:ArcAssay) ->
            let newAssay = assay.Copy()
            newAssay.GetTableAt(index)

    // - Table API - //
    member this.GetTable(name: string) : ArcTable =
        tryByTableName name this.Tables
        |> this.GetTableAt

    static member getTable(name: string) : ArcAssay -> ArcTable =
        fun (assay:ArcAssay) ->
            // copy is done in subfunction
            tryByTableName name assay.Tables
            |> ArcAssay.getTableAt <| assay

    // - Table API - //
    member this.SetTableAt(index:int, table:ArcTable) =
        SanityChecks.validateSheetIndex index false this.Tables
        SanityChecks.validateNewNameUnique table.Name this.TableNames
        this.Tables.[index] <- table

    static member setTableAt(index:int, table:ArcTable) : ArcAssay -> ArcAssay =
        fun (assay:ArcAssay) ->
            let newAssay = assay.Copy()
            newAssay.SetTableAt(index, table)
            newAssay

    // - Table API - //
    member this.SetTable(name: string, table:ArcTable) : unit =
        (tryByTableName name this.Tables, table)
        |> this.SetTableAt

    static member setTable(name: string, table:ArcTable) : ArcAssay -> ArcAssay =
        fun (assay:ArcAssay) ->
            // copy is done in subfunction
            (tryByTableName name assay.Tables, table)
            |> ArcAssay.setTableAt <| assay

    // - Table API - //
    member this.RemoveTableAt(index:int) : unit =
        SanityChecks.validateSheetIndex index false this.Tables
        this.Tables.RemoveAt(index)

    static member removeTableAt(index:int) : ArcAssay -> ArcAssay =
        fun (assay:ArcAssay) ->
            let newAssay = assay.Copy()
            newAssay.RemoveTableAt(index)
            newAssay

    // - Table API - //
    member this.RemoveTable(name: string) : unit =
        tryByTableName name this.Tables
        |> this.RemoveTableAt

    static member removeTable(name: string) : ArcAssay -> ArcAssay =
        fun (assay:ArcAssay) ->
            // copy is done in subfunction
            tryByTableName name assay.Tables
            |> ArcAssay.removeTableAt <| assay

    // - Table API - //
    // Remark: This must stay `ArcTable -> unit` so name cannot be changed here.
    member this.UpdateTableAt(index: int, updateFun: ArcTable -> unit) =
        SanityChecks.validateSheetIndex index false this.Tables
        let table = this.Tables.[index]
        updateFun table

    static member updateTableAt(index:int, updateFun: ArcTable -> unit) =
        fun (assay:ArcAssay) ->
            let newAssay = assay.Copy()    
            newAssay.UpdateTableAt(index, updateFun)
            newAssay

    // - Table API - //
    member this.UpdateTable(name: string, updateFun: ArcTable -> unit) : unit =
        (tryByTableName name this.Tables, updateFun)
        |> this.UpdateTableAt

    static member updateTable(name: string, updateFun: ArcTable -> unit) : ArcAssay -> ArcAssay =
        fun (assay:ArcAssay) ->
            // copy is done in subfunction
            (tryByTableName name assay.Tables, updateFun)
            |> ArcAssay.updateTableAt <| assay

    // - Table API - //
    member this.RenameTableAt(index: int, newName: string) : unit =
        SanityChecks.validateSheetIndex index false this.Tables
        SanityChecks.validateNewNameUnique newName this.TableNames
        let table = this.GetTableAt index
        let renamed = {table with Name = newName} 
        this.SetTableAt(index, renamed)

    static member renameTableAt(index: int, newName: string) : ArcAssay -> ArcAssay =
        fun (assay:ArcAssay) ->
            let newAssay = assay.Copy()    
            newAssay.RenameTableAt(index, newName)
            newAssay

    // - Table API - //
    member this.RenameTable(name: string, newName: string) : unit =
        (tryByTableName name this.Tables, newName)
        |> this.RenameTableAt

    static member renameTableAt(name: string, newName: string) : ArcAssay -> ArcAssay =
        fun (assay:ArcAssay) ->
            // copy is done in subfunction
            (tryByTableName name assay.Tables, newName)
            |> ArcAssay.renameTableAt <| assay

    // - Column CRUD API - //
    member this.AddColumn(tableIndex:int, header: CompositeHeader, ?cells: CompositeCell [], ?columnIndex: int, ?forceReplace: bool) = 
        this.UpdateTableAt(tableIndex, fun table ->
            table.AddColumn(header, ?cells=cells, ?index=columnIndex, ?forceReplace=forceReplace)
        )

    member this.Copy() : ArcAssay =
        let newSheets = ResizeArray()
        for table in this.Tables do
            let copy = table.Copy()
            newSheets.Add(copy)
        { this with Tables = newSheets }
        
    static member getIdentifier (assay : Assay) = 
        raise (System.NotImplementedException())

    static member setPerformers performers assay =
        {assay with Performers = performers}

    static member fromAssay (assay : Assay) : ArcAssay =
        raise (System.NotImplementedException())

    static member toAssay (assay : ArcAssay) : Assay =
        raise (System.NotImplementedException())