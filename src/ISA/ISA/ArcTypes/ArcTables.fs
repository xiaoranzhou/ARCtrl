﻿namespace ARCtrl.ISA

open System.Collections.Generic

module ArcTablesAux =

    ///// 👀 Please do not remove this here until i copied it to Swate ~Kevin F
    //let getNextAutoGeneratedTableName (existingNames: seq<string>) =
    //    let findNextNumber (numbers: int list) =
    //        let rec findNext current = function
    //            | [] -> current + 1
    //            | x::xs when x = current + 1 -> findNext x xs
    //            | _ -> current + 1

    //        match numbers with
    //        | [] -> 0
    //        | x::xs -> findNext x xs
    //    let existingNumbers = existingNames |> Seq.choose (fun x -> match x with | Regex.ActivePatterns.AutoGeneratedTableName n -> Some n| _ -> None) 
    //    let nextNumber =
    //        if Seq.isEmpty existingNumbers then
    //            1
    //        else
    //            existingNumbers
    //            |> Seq.sort
    //            |> List.ofSeq
    //            |> findNextNumber
    //    ArcTable.init($"New Table {nextNumber}")

    /// If a table with the given name exists in the TableList, returns it, else returns None.
    let tryFindIndexByTableName (name: string) (tables: ResizeArray<ArcTable>) =
        Seq.tryFindIndex (fun t -> t.Name = name) tables

    /// If a table with the given name exists in the TableList, returns it, else fails.
    let findIndexByTableName (name: string) (tables: ResizeArray<ArcTable>) =
        match Seq.tryFindIndex (fun t -> t.Name = name) tables with
        | Some index -> index
        | None -> failwith $"Unable to find table with name '{name}'!"

    module SanityChecks =
        
        /// Fails, if the index is out of range of the Tables collection. When allowAppend is set to true, it may be out of range by at most 1. 
        let validateSheetIndex (index: int) (allowAppend: bool) (sheets: ResizeArray<ArcTable>) =
            let eval x y = if allowAppend then x > y else x >= y
            if index < 0 then failwith "Cannot insert ArcTable at index < 0."
            if eval index sheets.Count then failwith $"Specified index is out of range! Assay contains only {sheets.Count} tables."

        /// Fails, if two tables have the same name.
        let validateNamesUnique (names:seq<string>) =
            let isDistinct = (Seq.length names) = (Seq.distinct names |> Seq.length)
            if not isDistinct then 
                failwith "Cannot add multiple tables with the same name! Table names inside one assay must be unqiue"

        /// Fails, if the name is already used by another table.
        let validateNewNameUnique (newName:string) (existingNames:seq<string>) =
            match Seq.tryFindIndex (fun x -> x = newName) existingNames with
            | Some i ->
                failwith $"Cannot create table with name {newName}, as table names must be unique and table at index {i} has the same name."
            | None ->
                ()

        /// Fails, if the name is already used by another table at a different position.
        ///
        /// Does not fail, if the newName is the same as the one in the given position.
        let validateNewNameAtUnique (index : int) (newName:string) (existingNames:seq<string>) =
            match Seq.tryFindIndex (fun x -> x = newName) existingNames with
            | Some i when index = i-> ()
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

open ArcTablesAux
open ArcTableAux
/// This type only includes mutable options and only static members, the MUST be referenced and used in all record types implementing `ResizeArray<ArcTable>`
type ArcTables(thisTables:ResizeArray<ArcTable>) = 

    member this.Count 
        with get() = thisTables.Count

    member this.TableNames 
        with get() = 
            [for s in thisTables do yield s.Name]

    member this.Tables = 
        thisTables

    member this.Item 
        with get(index) = 
            thisTables.[index] 

    // - Table API - //
    member this.AddTable(table:ArcTable, ?index: int) = 
        let index = defaultArg index this.Count
        SanityChecks.validateSheetIndex index true thisTables
        SanityChecks.validateNewNameUnique table.Name this.TableNames
        thisTables.Insert(index, table)

    // - Table API - //
    member this.AddTables(tables:seq<ArcTable>, ?index: int) = 
        let index = defaultArg index this.Count
        SanityChecks.validateSheetIndex index true thisTables
        SanityChecks.validateNewNamesUnique (tables |> Seq.map (fun x -> x.Name)) this.TableNames
        thisTables.InsertRange(index, tables)

    // - Table API - //
    member this.InitTable(tableName:string, ?index: int) = 
        let index = defaultArg index this.Count
        let table = ArcTable.init(tableName)
        SanityChecks.validateSheetIndex index true thisTables
        SanityChecks.validateNewNameUnique table.Name this.TableNames
        thisTables.Insert(index, table)
        table

    // - Table API - //
    member this.InitTables(tableNames:seq<string>, ?index: int) = 
        let index = defaultArg index this.Count
        let tables = tableNames |> Seq.map (fun x -> ArcTable.init(x))
        SanityChecks.validateSheetIndex index true thisTables
        SanityChecks.validateNewNamesUnique (tables |> Seq.map (fun x -> x.Name)) this.TableNames
        thisTables.InsertRange(index, tables)

    // - Table API - //
    member this.GetTableAt(index:int) : ArcTable =
        SanityChecks.validateSheetIndex index false thisTables
        thisTables.[index]

    // - Table API - //
    member this.GetTable(name: string) : ArcTable =
        findIndexByTableName name thisTables
        |> this.GetTableAt

    // - Table API - //
    member this.UpdateTableAt(index:int, table:ArcTable) =
        SanityChecks.validateSheetIndex index false thisTables
        SanityChecks.validateNewNameAtUnique index table.Name this.TableNames
        thisTables.[index] <- table

    // - Table API - //
    member this.UpdateTable(name: string, table:ArcTable) : unit =
        (findIndexByTableName name thisTables, table)
        |> this.UpdateTableAt

    // - Table API - //
    member this.SetTableAt(index:int, table:ArcTable) =
        SanityChecks.validateSheetIndex index true thisTables
        SanityChecks.validateNewNameAtUnique index table.Name this.TableNames
        thisTables.[index] <- table

    // - Table API - //
    member this.SetTable(name: string, table:ArcTable) : unit =
        match tryFindIndexByTableName name thisTables with
        | Some index -> this.SetTableAt(index, table)
        | None -> this.AddTable(table)

    // - Table API - //
    member this.RemoveTableAt(index:int) : unit =
        SanityChecks.validateSheetIndex index false thisTables
        thisTables.RemoveAt(index)

    // - Table API - //
    member this.RemoveTable(name: string) : unit =
        findIndexByTableName name thisTables
        |> this.RemoveTableAt


    // - Table API - //
    // Remark: This must stay `ArcTable -> unit` so name cannot be changed here.
    member this.MapTableAt(index: int, updateFun: ArcTable -> unit) =
        SanityChecks.validateSheetIndex index false thisTables
        let table = thisTables.[index]
        updateFun table

    // - Table API - //
    member this.MapTable(name: string, updateFun: ArcTable -> unit) : unit =
        (findIndexByTableName name thisTables, updateFun)
        |> this.MapTableAt

    // - Table API - //
    member this.RenameTableAt(index: int, newName: string) : unit =
        SanityChecks.validateSheetIndex index false thisTables
        SanityChecks.validateNewNameUnique newName this.TableNames
        let table = this.GetTableAt index
        let renamed = {table with Name = newName} 
        this.UpdateTableAt(index, renamed)

    // - Table API - //
    member this.RenameTable(name: string, newName: string) : unit =
        (findIndexByTableName name thisTables, newName)
        |> this.RenameTableAt

    // - Column CRUD API - //
    member this.AddColumnAt(tableIndex:int, header: CompositeHeader, ?cells: CompositeCell [], ?columnIndex: int, ?forceReplace: bool) = 
        this.MapTableAt(tableIndex, fun table ->
            table.AddColumn(header, ?cells=cells, ?index=columnIndex, ?forceReplace=forceReplace)
        )

    // - Column CRUD API - //
    member this.AddColumn(tableName: string, header: CompositeHeader, ?cells: CompositeCell [], ?columnIndex: int, ?forceReplace: bool) =
        findIndexByTableName tableName thisTables
        |> fun i -> this.AddColumnAt(i, header, ?cells=cells, ?columnIndex=columnIndex, ?forceReplace=forceReplace)

    // - Column CRUD API - //
    member this.RemoveColumnAt(tableIndex: int, columnIndex: int) =
        this.MapTableAt(tableIndex, fun table ->
            table.RemoveColumn(columnIndex)
        )

    // - Column CRUD API - //
    member this.RemoveColumn(tableName: string, columnIndex: int) : unit =
        (findIndexByTableName tableName thisTables, columnIndex)
        |> this.RemoveColumnAt

    // - Column CRUD API - //
    member this.UpdateColumnAt(tableIndex: int, columnIndex: int, header: CompositeHeader, ?cells: CompositeCell []) =
        this.MapTableAt(tableIndex, fun table ->
            table.UpdateColumn(columnIndex, header, ?cells=cells)
        )

    // - Column CRUD API - //
    member this.UpdateColumn(tableName: string, columnIndex: int, header: CompositeHeader, ?cells: CompositeCell []) =
        findIndexByTableName tableName thisTables
        |> fun tableIndex -> this.UpdateColumnAt(tableIndex, columnIndex, header, ?cells=cells)

    // - Column CRUD API - //
    member this.GetColumnAt(tableIndex: int, columnIndex: int) =
        let table = this.GetTableAt(tableIndex)
        table.GetColumn(columnIndex)

    // - Column CRUD API - //
    member this.GetColumn(tableName: string, columnIndex: int) =
        (findIndexByTableName tableName thisTables, columnIndex)
        |> this.GetColumnAt

    // - Row CRUD API - //
    member this.AddRowAt(tableIndex:int, ?cells: CompositeCell [], ?rowIndex: int) = 
        this.MapTableAt(tableIndex, fun table ->
            table.AddRow(?cells=cells, ?index=rowIndex)
        )

    // - Row CRUD API - //
    member this.AddRow(tableName: string, ?cells: CompositeCell [], ?rowIndex: int) =
        findIndexByTableName tableName thisTables
        |> fun i -> this.AddRowAt(i, ?cells=cells, ?rowIndex=rowIndex)

    // - Row CRUD API - //
    member this.RemoveRowAt(tableIndex: int, rowIndex: int) =
        this.MapTableAt(tableIndex, fun table ->
            table.RemoveRow(rowIndex)
        )

    // - Row CRUD API - //
    member this.RemoveRow(tableName: string, rowIndex: int) : unit =
        (findIndexByTableName tableName thisTables, rowIndex)
        |> this.RemoveRowAt

    // - Row CRUD API - //
    member this.UpdateRowAt(tableIndex: int, rowIndex: int, cells: CompositeCell []) =
        this.MapTableAt(tableIndex, fun table ->
            table.UpdateRow(rowIndex, cells)
        )

    // - Row CRUD API - //
    member this.UpdateRow(tableName: string, rowIndex: int, cells: CompositeCell []) =
        (findIndexByTableName tableName thisTables, rowIndex, cells)
        |> this.UpdateRowAt

    // - Row CRUD API - //
    member this.GetRowAt(tableIndex: int, rowIndex: int) =
        let table = this.GetTableAt(tableIndex)
        table.GetRow(rowIndex)

    // - Row CRUD API - //
    member this.GetRow(tableName: string, rowIndex: int) =
        (findIndexByTableName tableName thisTables, rowIndex)
        |> this.GetRowAt

    /// Return a list of all the processes in all the tables.
    member this.GetProcesses() : Process list = 
        this.Tables
        |> Seq.toList
        |> List.collect (fun t -> t.GetProcesses())

    static member ofSeq (tables : ArcTable seq) : ArcTables = 
        tables
        |> ResizeArray
        |> ArcTables

    /// Create a collection of tables from a list of processes.
    ///
    /// For this, the processes are grouped by nameroot ("nameroot_1", "nameroot_2" ...) or exectued protocol if no name exists
    ///
    /// Then each group is converted to a table with this nameroot as sheetname
    static member fromProcesses (ps : Process list) : ArcTables = 
        ps
        |> ProcessParsing.groupProcesses
        |> List.map (fun (name,ps) ->
            ps
            |> List.collect (fun p -> ProcessParsing.processToRows p)
            |> fun rows -> ProcessParsing.alignByHeaders true rows
            |> fun (headers, rows) -> ArcTable.create(name,headers,rows)
        )
        |> ResizeArray
        |> ArcTables

    static member updateReferenceTablesBySheets (referenceTables : ArcTables,sheetTables : ArcTables,?keepUnusedRefTables : bool) : ArcTables =
        let keepUnusedRefTables = Option.defaultValue false keepUnusedRefTables
        let usedTables = HashSet<string>()
        let referenceTableMap = 
            referenceTables.Tables |> Seq.map (fun t -> t.GetProtocolNameColumn().Cells.[0].AsFreeText, t) |> Map.ofSeq
        sheetTables.Tables
        |> Seq.toArray
        |> Array.collect ArcTable.SplitByProtocolREF
        |> Array.map (fun t ->
            let k = 
                t.Headers |> Seq.tryFindIndex (fun x -> x = CompositeHeader.ProtocolREF)
                |> Option.bind (fun i ->
                    t.TryGetCellAt(i,0)                        
                )
                |> Option.bind (fun c -> 
                    if c.AsFreeText = ""then None
                    else Some c.AsFreeText )
                |> Option.defaultValue t.Name
            match Map.tryFind k referenceTableMap with
            | Some rt -> 
                usedTables.Add(k) |> ignore
                let rt = rt.Copy()
                rt.UpdateReferenceByAnnotationTable t
                ArcTable.create(t.Name, rt.Headers, rt.Values)
            | None -> t
        )
        |> Array.groupBy (fun t -> t.Name)
        |> Array.map (fun (_,ts) -> 
            ts
            |> Seq.reduce ArcTable.append
        )
        |> Array.map (fun t -> 
            ArcTableAux.Unchecked.fillMissingCells t.Headers t.Values
            t)
        |> fun s -> 
            if keepUnusedRefTables then
                Seq.append 
                    (referenceTableMap |> Seq.choose (fun (kv) -> if usedTables.Contains kv.Key then None else Some kv.Value))
                    s
            else
                s
        |> ResizeArray        
        |> ArcTables