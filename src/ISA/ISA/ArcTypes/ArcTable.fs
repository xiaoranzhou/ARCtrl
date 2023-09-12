﻿namespace ARCtrl.ISA

open Fable.Core
open System.Collections.Generic
open ArcTableAux
open ColumnIndex

open Fable.Core.JsInterop

[<CustomEquality; NoComparison>]
[<AttachMembers>]
type ArcTable = 
    {
        Name : string
        Headers : ResizeArray<CompositeHeader>
        /// Key: Column * Row
        Values : System.Collections.Generic.Dictionary<int*int,CompositeCell>  
    }

    static member create(name, headers, values) =
        {
            Name = name
            Headers = headers
            Values = values

        }

    /// Create ArcTable with empty 'ValueHeader' and 'Values' 
    static member init(name: string) = {
        Name = name
        Headers = ResizeArray<CompositeHeader>()
        Values = System.Collections.Generic.Dictionary<int*int,CompositeCell>()
    }

    /// Will return true or false if table is valid. 
    ///
    /// Set `raiseException` = `true` to raise exception.
    member this.Validate(?raiseException: bool) = 
        let mutable isValid: bool = true
        for columnIndex in 0 .. (this.ColumnCount - 1) do
            let column : CompositeColumn = this.GetColumn(columnIndex)
            isValid <- column.validate(?raiseException=raiseException)
        isValid

    /// Will return true or false if table is valid. 
    ///
    /// Set `raiseException` = `true` to raise exception.
    static member validate(?raiseException: bool) =
        fun (table:ArcTable) ->
            table.Validate(?raiseException=raiseException)


    member this.ColumnCount 
        with get() = ArcTableAux.getColumnCount this.Headers

    member this.RowCount 
        with get() = ArcTableAux.getRowCount this.Values

    member this.Columns 
        with get() = [for i = 0 to this.ColumnCount - 1 do this.GetColumn(i)] 

    member this.Copy() : ArcTable = 
        ArcTable.create(
            this.Name,
            ResizeArray(this.Headers), 
            Dictionary(this.Values)
        )


    /// Returns a cell at given position if it exists, else returns None.
    member this.TryGetCellAt (column: int,row: int) = ArcTableAux.Unchecked.tryGetCellAt (column,row) this.Values
    
    static member tryGetCellAt  (column: int,row: int) =
        fun (table:ArcTable) ->
            table.TryGetCellAt(column, row)


    // - Cell API - //
    // TODO: And then directly a design question. Is a column with rows containing both CompositeCell.Term and CompositeCell.Unitized allowed?
    member this.UpdateCellAt(columnIndex, rowIndex,c : CompositeCell) =
        SanityChecks.validateColumnIndex columnIndex this.ColumnCount false
        SanityChecks.validateRowIndex rowIndex this.RowCount false
        SanityChecks.validateColumn <| CompositeColumn.create(this.Headers.[columnIndex],[|c|])
        Unchecked.setCellAt(columnIndex, rowIndex,c) this.Values

    static member updateCellAt(columnIndex: int, rowIndex: int, cell: CompositeCell) =
        fun (table:ArcTable) ->
            let newTable = table.Copy()
            newTable.UpdateCellAt(columnIndex,rowIndex,cell)
            newTable

    // - Header API - //
    member this.UpdateHeader (index:int, newHeader: CompositeHeader, ?forceConvertCells: bool) =
        let forceConvertCells = Option.defaultValue false forceConvertCells
        ArcTableAux.SanityChecks.validateColumnIndex index this.ColumnCount false
        /// remove to be replaced header, this is only used to check if any OTHER header is of the same unique type as column.Header
        /// MUST USE "Seq.removeAt" to not remove in mutable object!
        let otherHeaders = this.Headers |> Seq.removeAt index
        ArcTableAux.SanityChecks.validateNoDuplicateUnique newHeader otherHeaders
        let c = { this.GetColumn(index) with Header = newHeader }
        // Test if column is still valid with new header, if so insert header at index
        if c.validate() then
            let setHeader = this.Headers.[index] <- newHeader
            ()
        // if we force convert cells, we want to convert the existing cells to a valid cell type for the new header
        elif forceConvertCells then
            let convertedCells =
                match newHeader with
                | isTerm when newHeader.IsTermColumn -> c.Cells |> Array.map (fun c -> c.ToTermCell())
                | _ -> c.Cells |> Array.map (fun c -> c.ToFreeTextCell())
            this.UpdateColumn(index, newHeader, convertedCells)
        else
            failwith "Tried setting header for column with invalid type of cells. Set `forceConvertCells` flag to automatically convert cells into valid CompositeCell type."

    static member updateHeader (index:int, header:CompositeHeader) = 
        fun (table:ArcTable) ->
            let newTable = table.Copy()
            newTable.UpdateHeader(index, header)
            newTable

    // - Column API - //
    //[<NamedParams>]
    member this.AddColumn (header:CompositeHeader, ?cells: CompositeCell [], ?index: int, ?forceReplace: bool) : unit = 
        let index = 
            defaultArg index this.ColumnCount
        let cells = 
            defaultArg cells [||]
        let forceReplace = defaultArg forceReplace false 
        SanityChecks.validateColumnIndex index this.ColumnCount true
        SanityChecks.validateColumn(CompositeColumn.create(header, cells))
        // 
        Unchecked.addColumn header cells index forceReplace this.Headers this.Values
        Unchecked.fillMissingCells this.Headers this.Values

    static member addColumn (header: CompositeHeader, ?cells: CompositeCell [],?index: int ,?forceReplace : bool) : (ArcTable -> ArcTable) =
        fun (table: ArcTable) ->
            let newTable = table.Copy()
            newTable.AddColumn(header, ?cells = cells, ?index = index, ?forceReplace = forceReplace)
            newTable

    // - Column API - //
    member this.UpdateColumn (columnIndex:int, header: CompositeHeader, ?cells: CompositeCell []) =
        SanityChecks.validateColumnIndex columnIndex this.ColumnCount false
        let column = CompositeColumn.create(header, ?cells=cells)
        SanityChecks.validateColumn(column)
        /// remove to be replaced header, this is only used to check if any OTHER header is of the same unique type as column.Header
        /// MUST USE "Seq.removeAt" to not remove in mutable object!
        let otherHeaders = this.Headers |> Seq.removeAt columnIndex
        SanityChecks.validateNoDuplicateUnique column.Header otherHeaders
        // Must remove first, so no leftover rows stay when setting less rows than before.
        Unchecked.removeHeader columnIndex this.Headers
        Unchecked.removeColumnCells columnIndex this.Values
        // nextHeader 
        this.Headers.Insert(columnIndex,column.Header)
        // nextBody
        column.Cells |> Array.iteri (fun rowIndex v -> Unchecked.setCellAt(columnIndex,rowIndex,v) this.Values)
        Unchecked.fillMissingCells this.Headers this.Values

    static member updatetColumn (columnIndex:int, header: CompositeHeader, ?cells: CompositeCell []) = 
        fun (table:ArcTable) ->
            let newTable = table.Copy()
            newTable.UpdateColumn(columnIndex, header, ?cells=cells)
            newTable

    // - Column API - //
    member this.InsertColumn (header:CompositeHeader, index: int, ?cells: CompositeCell []) =
        this.AddColumn(header, index = index,?cells = cells, forceReplace = false)

    static member insertColumn (header:CompositeHeader, index: int, ?cells: CompositeCell []) =
        fun (table: ArcTable) ->
            let newTable = table.Copy()
            newTable.InsertColumn(header, index, ?cells = cells)
            newTable

    // - Column API - //
    member this.AppendColumn (header:CompositeHeader, ?cells: CompositeCell []) =
        this.AddColumn(header, ?cells = cells, index = this.ColumnCount, forceReplace = false)

    static member appendColumn (header:CompositeHeader, ?cells: CompositeCell []) =
        fun (table: ArcTable) ->
            let newTable = table.Copy()
            newTable.AppendColumn(header, ?cells = cells)
            newTable

    // - Column API - //
    member this.AddColumns (columns: CompositeColumn [], ?index: int, ?forceReplace: bool) : unit = 
        let mutable index = defaultArg index this.ColumnCount
        let forceReplace = defaultArg forceReplace false
        SanityChecks.validateColumnIndex index this.ColumnCount true
        SanityChecks.validateNoDuplicateUniqueColumns columns
        columns |> Array.iter (fun x -> SanityChecks.validateColumn x)
        columns
        |> Array.iter (fun col -> 
            let prevHeadersCount = this.Headers.Count
            Unchecked.addColumn col.Header col.Cells index forceReplace this.Headers this.Values
            // Check if more headers, otherwise `ArcTableAux.insertColumn` replaced a column and we do not need to increase index.
            if this.Headers.Count > prevHeadersCount then index <- index + 1
        )
        Unchecked.fillMissingCells this.Headers this.Values

    static member addColumns (columns: CompositeColumn [],?index: int) =
        fun (table:ArcTable) ->
            let newTable = table.Copy()
            newTable.AddColumns(columns, ?index = index)
            newTable

    // - Column API - //
    member this.RemoveColumn (index:int) =
        ArcTableAux.SanityChecks.validateColumnIndex index this.ColumnCount false
        /// Set ColumnCount here to avoid changing columnCount by changing header count
        let columnCount = this.ColumnCount
        // removeHeader 
        Unchecked.removeHeader(index) this.Headers
        // removeCell
        Unchecked.removeColumnCells_withIndexChange(index) columnCount this.RowCount this.Values

    static member removeColumn (index:int) =
        fun (table:ArcTable) ->
            let newTable = table.Copy()
            newTable.RemoveColumn(index)
            newTable

    // - Column API - //
    member this.RemoveColumns(indexArr:int []) =
        // Sanity check here too, to avoid removing things from mutable to fail in the middle
        Array.iter (fun index -> SanityChecks.validateColumnIndex index this.ColumnCount false) indexArr
        /// go from highest to lowest so no wrong column gets removed after index shift
        let indexArr = indexArr |> Array.sortDescending
        Array.iter (fun index -> this.RemoveColumn index) indexArr

    static member removeColumns(indexArr:int []) =
        fun (table:ArcTable) ->
            let newTable = table.Copy()
            newTable.RemoveColumns(indexArr)
            newTable

    // - Column API - //
    // GetColumnAt?
    member this.GetColumn(columnIndex:int) =
        SanityChecks.validateColumnIndex columnIndex this.ColumnCount false
        let h = this.Headers.[columnIndex]
        let cells = [|
            for i = 0 to this.RowCount - 1 do 
                this.TryGetCellAt(columnIndex, i).Value
        |]
        CompositeColumn.create(h, cells)

    static member getColumn (index:int) = 
        fun (table:ArcTable) ->
            table.GetColumn(index)

    member this.GetColumnByHeader (header:CompositeHeader) =
        let index = this.Headers |> Seq.findIndex (fun x -> x = header)
        this.GetColumn(index)

    // - Row API - //
    member this.AddRow (?cells: CompositeCell [], ?index: int) : unit = 
        let index = defaultArg index this.RowCount
        let cells = 
            if cells.IsNone then
                // generate default cells. Uses the same logic as extending missing row values.
                [|
                    for columnIndex in 0 .. this.ColumnCount-1 do
                        let h = this.Headers.[columnIndex]
                        let tryFirstCell = Unchecked.tryGetCellAt(columnIndex,0) this.Values
                        yield Unchecked.getEmptyCellForHeader h tryFirstCell
                |]
            else 
                cells.Value
        // Sanity checks
        SanityChecks.validateRowIndex index this.RowCount true
        SanityChecks.validateRowLength cells this.ColumnCount
        for columnIndex in 0 .. this.ColumnCount-1 do
            let h = this.Headers.[columnIndex]
            let column = CompositeColumn.create(h,[|cells.[columnIndex]|])
            SanityChecks.validateColumn column
        // Sanity checks - end
        Unchecked.addRow index cells this.Headers this.Values

    static member addRow (?cells: CompositeCell [], ?index: int) =
        fun (table:ArcTable) ->
            let newTable = table.Copy()
            newTable.AddRow(?cells=cells,?index=index)
            newTable

    // - Row API - //
    member this.UpdateRow(rowIndex: int, cells: CompositeCell []) =
        SanityChecks.validateRowIndex rowIndex this.RowCount false
        SanityChecks.validateRowLength cells this.RowCount
        cells
        |> Array.iteri (fun i cell ->
            let h = this.Headers.[i]
            let column = CompositeColumn.create(h,[|cell|])
            SanityChecks.validateColumn column
        )
        cells
        |> Array.iteri (fun columnIndex cell ->
            Unchecked.setCellAt(columnIndex, rowIndex, cell) this.Values
        )

    static member updateRow(rowIndex: int, cells: CompositeCell []) =
        fun (table: ArcTable) ->
            let newTable = table.Copy()
            newTable.UpdateRow(rowIndex, cells)
            newTable

    // - Row API - //
    member this.AppendRow (?cells: CompositeCell []) =
        this.AddRow(?cells=cells,index = this.RowCount)

    static member appendRow (?cells: CompositeCell []) =
        fun (table:ArcTable) ->
            let newTable = table.Copy()
            newTable.AppendRow(?cells=cells)
            newTable

    // - Row API - //
    member this.InsertRow (index: int, ?cells: CompositeCell []) =
        this.AddRow(index=index, ?cells=cells)

    static member insertRow (index: int, ?cells: CompositeCell []) =
        fun (table:ArcTable) ->
            let newTable = table.Copy()
            newTable.AddRow(index=index, ?cells=cells)
            newTable

    // - Row API - //
    member this.AddRows (rows: CompositeCell [] [], ?index: int) =
        let mutable index = defaultArg index this.RowCount
        // Sanity checks
        SanityChecks.validateRowIndex index this.RowCount true
        rows |> Array.iter (fun row -> SanityChecks.validateRowLength row this.ColumnCount)
        for row in rows do
            for columnIndex in 0 .. this.ColumnCount-1 do
                let h = this.Headers.[columnIndex]
                let column = CompositeColumn.create(h,[|row.[columnIndex]|])
                SanityChecks.validateColumn column
        // Sanity checks - end
        rows
        |> Array.iter (fun row ->
            Unchecked.addRow index row this.Headers this.Values
            index <- index + 1
        )

    static member addRows (rows: CompositeCell [] [], ?index: int) =
        fun (table:ArcTable) ->
            let newTable = table.Copy()
            newTable.AddRows(rows,?index=index)
            newTable

    // - Row API - //
    member this.AddRowsEmpty (rowCount: int, ?index: int) =
        let row = [|
            for columnIndex in 0 .. this.ColumnCount-1 do
                let h = this.Headers.[columnIndex]
                let tryFirstCell = Unchecked.tryGetCellAt(columnIndex,0) this.Values
                yield Unchecked.getEmptyCellForHeader h tryFirstCell
        |]
        let rows = Array.init rowCount (fun _ -> row)
        this.AddRows(rows,?index=index)

    static member addRowsEmpty (rowCount: int, ?index: int) =
        fun (table:ArcTable) ->
            let newTable = table.Copy()
            newTable.AddRowsEmpty(rowCount, ?index=index)
            newTable

    // - Row API - //
    member this.RemoveRow (index:int) =
        ArcTableAux.SanityChecks.validateRowIndex index this.RowCount false
        let removeCells = Unchecked.removeRowCells_withIndexChange index this.ColumnCount this.RowCount this.Values
        ()

    static member removeRow (index:int) =
        fun (table:ArcTable) ->
            let newTable = table.Copy()
            newTable.RemoveRow (index)
            newTable

    // - Row API - //
    member this.RemoveRows (indexArr:int []) =
        // Sanity check here too, to avoid removing things from mutable to fail in the middle
        Array.iter (fun index -> ArcTableAux.SanityChecks.validateRowIndex index this.RowCount false) indexArr
        /// go from highest to lowest so no wrong column gets removed after index shift
        let indexArr = indexArr |> Array.sortDescending
        Array.iter (fun index -> this.RemoveRow index) indexArr
        
    static member removeRows (indexArr:int []) =
        fun (table:ArcTable) ->
            let newTable = table.Copy()
            newTable.RemoveColumns indexArr
            newTable

    // - Row API - //
    member this.GetRow(rowIndex : int) =
        SanityChecks.validateRowIndex rowIndex this.RowCount false
        [|
            for columnIndex = 0 to this.ColumnCount - 1 do 
                this.TryGetCellAt(columnIndex, rowIndex).Value
        |]       
        
    static member getRow (index:int) = 
        fun (table:ArcTable) ->
            table.GetRow(index)

    //member this.InsertParameterValue () =
    //    this.in

    static member insertParameterValue (t : ArcTable) (p : ProcessParameterValue) : ArcTable = 
        raise (System.NotImplementedException())

    static member getParameterValues (t : ArcTable) : ProcessParameterValue [] = 
        raise (System.NotImplementedException())

    ///
    member this.AddProtocolTypeColumn(?types : OntologyAnnotation [], ?index : int) =
        let header = CompositeHeader.ProtocolType
        let cells = types |> Option.map (Array.map CompositeCell.Term)
        this.AddColumn(header, ?cells = cells, ?index = index)

    member this.AddProtocolVersionColumn(?versions : string [], ?index : int) =
        let header = CompositeHeader.ProtocolVersion
        let cells = versions |> Option.map (Array.map CompositeCell.FreeText)
        this.AddColumn(header, ?cells = cells, ?index = index)

    member this.AddProtocolUriColumn(?uris : string [], ?index : int) =
        let header = CompositeHeader.ProtocolUri
        let cells = uris |> Option.map (Array.map CompositeCell.FreeText)
        this.AddColumn(header, ?cells = cells, ?index = index)

    member this.AddProtocolDescriptionColumn(?descriptions : string [], ?index : int) =
        let header = CompositeHeader.ProtocolDescription
        let cells = descriptions |> Option.map (Array.map CompositeCell.FreeText)
        this.AddColumn(header, ?cells = cells, ?index = index)

    member this.AddProtocolNameColumn(?names : string [], ?index : int) =
        let header = CompositeHeader.ProtocolREF
        let cells = names |> Option.map (Array.map CompositeCell.FreeText)
        this.AddColumn(header, ?cells = cells, ?index = index)

    /// Get functions for the protocol columns
    member this.GetProtocolTypeColumn() =
        this.GetColumnByHeader(CompositeHeader.ProtocolType)

    member this.GetProtocolVersionColumn() =
        this.GetColumnByHeader(CompositeHeader.ProtocolVersion)

    member this.GetProtocolUriColumn() =
        this.GetColumnByHeader(CompositeHeader.ProtocolUri)

    member this.GetProtocolDescriptionColumn() =
        this.GetColumnByHeader(CompositeHeader.ProtocolDescription)

    member this.GetProtocolNameColumn() =
        this.GetColumnByHeader(CompositeHeader.ProtocolREF)

    member this.GetComponentColumns() =
        this.Headers
        |> Seq.filter (fun h -> h.isComponent)
        |> Seq.toArray
        |> Array.map (fun h -> this.GetColumnByHeader(h))

    /// Create a new table from an ISA protocol.
    ///
    /// The table will have at most one row, with the protocol information and the component values
    static member fromProtocol (p : Protocol) : ArcTable = 
        
        let t = ArcTable.init (p.Name |> Option.defaultValue (Identifier.createMissingIdentifier()))

        for pp in p.Parameters |> Option.defaultValue [] do

            //t.AddParameterColumn(pp, ?index = pp.TryGetColumnIndex())

            t.AddColumn(CompositeHeader.Parameter pp.ParameterName.Value, ?index = pp.TryGetColumnIndex())

        for c in p.Components |> Option.defaultValue [] do
            let v = c.ComponentValue |> Option.map ((fun v -> CompositeCell.fromValue(v,?unit = c.ComponentUnit)) >> Array.singleton)
            t.AddColumn(
                CompositeHeader.Parameter c.ComponentType.Value, 
                ?cells = v,
                ?index = c.TryGetColumnIndex())
        p.Description   |> Option.map (fun d -> t.AddProtocolDescriptionColumn([|d|]))  |> ignore
        p.Version       |> Option.map (fun d -> t.AddProtocolVersionColumn([|d|]))      |> ignore
        p.ProtocolType  |> Option.map (fun d -> t.AddProtocolTypeColumn([|d|]))         |> ignore
        p.Uri           |> Option.map (fun d -> t.AddProtocolUriColumn([|d|]))          |> ignore
        p.Name          |> Option.map (fun d -> t.AddProtocolNameColumn([|d|]))         |> ignore
        t

    /// Returns the list of protocols executed in this ArcTable
    member this.GetProtocols() : Protocol list = 

        if this.RowCount = 0 then
            this.Headers
            |> Seq.fold (fun (p : Protocol) h -> 
                match h with
                | CompositeHeader.ProtocolType -> 
                    Protocol.setProtocolType p OntologyAnnotation.empty
                | CompositeHeader.ProtocolVersion -> Protocol.setVersion p ""
                | CompositeHeader.ProtocolUri -> Protocol.setUri p ""
                | CompositeHeader.ProtocolDescription -> Protocol.setDescription p ""
                | CompositeHeader.ProtocolREF -> Protocol.setName p ""
                | CompositeHeader.Parameter oa -> 
                    let pp = ProtocolParameter.create(ParameterName = oa)
                    Protocol.addParameter (pp) p
                | CompositeHeader.Component oa -> 
                    let c = Component.create(ComponentType = oa)
                    Protocol.addComponent c p
                | _ -> p
            ) (Protocol.create(Name = this.Name))
            |> List.singleton
        else
            List.init this.RowCount (fun i ->
                this.GetRow(i) 
                |> Seq.zip this.Headers
                |> CompositeRow.toProtocol this.Name                   
            )
            |> List.distinct

    /// Returns the list of processes specidified in this ArcTable
    member this.GetProcesses() : Process list = 
        let getter = ProcessParsing.getProcessGetter this.Name this.Headers
        [
            for i in 0..this.RowCount-1 do
                yield getter this.Values i        
        ]

    /// Create a new table from a list of processes
    ///
    /// The name will be used as the sheet name
    /// 
    /// The processes SHOULD have the same headers, or even execute the same protocol
    static member fromProcesses name (ps : Process list) : ArcTable = 
        ps
        |> List.collect (fun p -> ProcessParsing.processToRows p)
        |> fun rows -> ProcessParsing.alignByHeaders rows
        |> fun (headers, rows) -> ArcTable.create(name,headers,rows)

    /// This method is meant to update an ArcTable stored as a protocol in a study or investigation file with the information from an ArcTable actually stored as an annotation table
    member this.UpdateReferenceByAnnotationTable(table:ArcTable) =
        ArcTableAux.Unchecked.extendToRowCount table.RowCount this.Headers this.Values
        for c in table.Columns do
            this.AddColumn(c.Header, cells = c.Cells,forceReplace = true)

    /// Pretty printer 
    override this.ToString() =
        [
            $"Table: {this.Name}"
            "-------------"
            this.Headers |> Seq.map (fun x -> x.ToString()) |> String.concat "\t|\t"
            for rowI = 0 to this.RowCount-1 do
                this.GetRow(rowI) |> Seq.map (fun x -> x.ToString()) |> String.concat "\t|\t"
        ]
        |> String.concat "\n"

    // custom check
    override this.Equals other =
        match other with
        | :? ArcTable as table -> 
            let sameName = this.Name = table.Name
            let h1 = this.Headers 
            let h2 = table.Headers
            let sameHeaderLength = h1.Count = h2.Count
            let sameHeaders = Seq.forall2 (fun x y -> x = y) h1 h2 
            let b1 = this.Values |> Seq.sortBy (fun kv -> kv.Key)
            let b2 = table.Values |> Seq.sortBy (fun kv -> kv.Key)
            let sameBodyLength = (Seq.length b1) = (Seq.length b2)
            let sameBodyCells = Seq.forall2 (fun x y -> x = y) b1 b2
            sameName && sameHeaderLength && sameHeaders && sameBodyLength && sameBodyCells
        | _ -> false

    // it's good practice to ensure that this behaves using the same fields as Equals does:
    override this.GetHashCode() = 
        let name = this.Name.GetHashCode()
        let headers = this.Headers |> Seq.fold (fun state ele -> state + ele.GetHashCode()) 0
        let bodyCells = this.Values |> Seq.fold (fun state ele -> state + ele.GetHashCode()) 0
        name + headers + bodyCells