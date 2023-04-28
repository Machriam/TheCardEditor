class GenericGrid {
    dotNetInstance;
    gridOptions;
    rowSelectionHandler;
    multipleRowSelect = false;
    enumParameter;
    cellRenderer = {
        "newLine": (param) => "<span>" + (param.value?.replaceAll("\n", "<br/>") ?? "") + "</span>",
        "": (param) => param?.value,
        "xAmount": (param) => Shared.convertToXAmount(param?.value, true, true),
        "percent": (param) => Shared.purityFoundConverter(param?.value),
        "enum": (param) => {
            let value = (param?.value ?? "").toUpperCase();
            let allowedValues = GenericGrid.getInstance(param.api.gridBodyCtrl.gridOptionsWrapper.eGridDiv.id).enumParameter[param.column.colId];
            return allowedValues.indexOf(value) > -1 ? value : "";
        },
        "bar": (param) => {
            let splittedData = param?.value.split(';')
                .map(v => { let res = v.split(":"); return [parseInt(res[0]), res[1]]; })
                .filter(v => !Shared.objectIsNullOrEmpty(v[1]));
            let total = splittedData.map(v => v[0]).reduce((pv, cv) => pv + cv, 0);
            let widthPerValue = (param.column.actualWidth - 37) / total;
            return `<span style="display:inline-block">` +
                splittedData.filter(v => v[0] > 0)
                    .map(v => `<span style="text-align:center;display:inline-block;background-color: ${v[1]};width:${v[0] * widthPerValue}px">${v[0]}</span>`)
                    .join("") +
                `</span>`;
        },
        "barPercentage": (param) => {
            let splittedData = param?.value.split(';')
                .map(v => { let res = v.split(":"); return [parseInt(res[0]), res[1]]; })
                .filter(v => !Shared.objectIsNullOrEmpty(v[1]));
            let total = splittedData.map(v => v[0]).reduce((pv, cv) => pv + cv, 0);
            let widthPerValue = (param.column.actualWidth - 37) / total;
            return `<span style="display:inline-block">` +
                splittedData.filter(v => v[0] > 0)
                    .map(v => {
                        let caption = Math.round(v[0] / total * 100);
                        if (caption == 0) caption = "";
                        return `<span style="text-align:center;display:inline-block;background-color: ${v[1]};` +
                            `width:${v[0] * widthPerValue}px">${caption}</span>`;
                    })
                    .join("") +
                `</span>`;
        }
    };
    onSelectionChanged(params) {
        let instance = GenericGrid.getInstance(params.api.gridBodyCtrl.gridOptionsWrapper.eGridDiv.id);
        var selectedRows = instance.gridOptions.api.getSelectedRows();
        if (instance.multipleRowSelect) {
            instance.dotNetInstance.invokeMethodAsync(instance.rowSelectionHandler, selectedRows.map(s => s.id));
        }
        else {
            instance.dotNetInstance.invokeMethodAsync(instance.rowSelectionHandler, selectedRows[0].id);
        }
    };
    arrowNavigation(params) {
        let divId = null;
        if (params.nextCellPosition != null) divId = params.nextCellPosition.column.gridOptionsWrapper.eGridDiv.id
        if (params.previousCellPosition != null) divId = params.previousCellPosition.column.gridOptionsWrapper.eGridDiv.id
        if (divId == null) return;
        let instance = GenericGrid.getInstance(divId);
        return Shared.arrowNavigation(params, instance);
    };
    static getInstance(divId) {
        if (!window.genericGridFunctions.instance.hasOwnProperty(divId)) {
            window.genericGridFunctions.instance[divId] = new GenericGrid();
        }
        return window.genericGridFunctions.instance[divId];
    };
    static removeInstance(divId) {
        delete window.genericGridFunctions.instance[divId];
    }
}

window.genericGridFunctions = {
    instance: {},
    initialize: function (data, parameter, ref, divId) {
        let instance = GenericGrid.getInstance(divId);
        instance.rowSelectionHandler = parameter.rowSelectionHandler;
        instance.multipleRowSelect = parameter.multipleRowSelect;
        let enumKeys = Object.keys(parameter.enumParameter ?? []);
        let upperCaseEnums = {};
        for (var i = 0; i < enumKeys.length; i++) {
            let key = enumKeys[i];
            upperCaseEnums[key] = parameter.enumParameter[key].map(v => v.toUpperCase());
        }
        instance.enumParameter = upperCaseEnums;
        columnDefinitions = parameter.columnDefinitions;
        instance.gridOptions = {
            navigateToNextCell: instance.arrowNavigation,
            columnDefs: columnDefinitions,
            enableBrowserTooltips: true,
            defaultColDef: {
                filter: parameter.showFilter,
                floatingFilter: parameter.showFilter,
                floatingFilterComponentParams: {
                    suppressFilterButton: parameter.suppressFilterButton
                },
            },
            rowData: data,
            tooltipShowDelay: 0,
            rowSelection: instance.multipleRowSelect ? "multiple" : "single",
            onSelectionChanged: instance.onSelectionChanged,
            getRowId: parameter.useUserIds ? (params) => params.data.id : undefined,
            rowHeight: parameter.rowSize
        };
        instance.gridOptions.columnDefs.forEach(c => {
            c.cellRenderer = instance.cellRenderer[c.cellRenderer];
            c.tooltipField = "rowTooltip";
        });
        instance.gridOptions.getRowClass = function (params) {
            return params.data.rowColorClass;
        };
        if (instance.multipleRowSelect) {
            let firstCol = instance.gridOptions.columnDefs.find(col => !col.hide);
            firstCol["headerCheckboxSelection"] = true;
            firstCol["headerCheckboxSelectionFilteredOnly"] = true;
            firstCol["checkboxSelection"] = true;
        }
        instance.dotNetInstance = ref;
        let grid = document.getElementById(divId);
        new agGrid.Grid(grid, instance.gridOptions);
    },
    insertRow: function (data, divId, before = false) {
        let instance = GenericGrid.getInstance(divId);
        instance.gridOptions.rowData = before ? [data, ...instance.gridOptions.rowData] : instance.gridOptions.rowData.push(data);
        instance.gridOptions.api.setRowData(instance.gridOptions.rowData);
    },
    updateData: function (data, divId) {
        let instance = GenericGrid.getInstance(divId);
        instance.gridOptions.rowData = data;
        instance.gridOptions.api.setRowData(data);
    },
    selectData: function (data, divId) {
        let instance = GenericGrid.getInstance(divId);
        let set = new Set(data);
        instance.gridOptions.api.forEachNode(node => set.has(node.data.id) ? node.setSelected(true) : node.setSelected(false));
    },
    dispose: function (divId) {
        GenericGrid.removeInstance(divId);
    },
    getData: function (divId) {
        let instance = GenericGrid.getInstance(divId);
        return instance.gridOptions.rowData;
    },
    getParsedData: function (divId) {
        let instance = GenericGrid.getInstance(divId);
        let result = [];
        instance.gridOptions.rowData.forEach((r, i) => {
            result[i] = {};
            instance.gridOptions.columnDefs.forEach(c => {
                result[i][c.field] = c.cellRenderer({ value: r[c.field] });
            });
        });
        return result;
    },
    getCsvExport: function (name, divId, onlyFilteredRows = false) {
        let instance = GenericGrid.getInstance(divId);
        let data = [];
        onlyFilteredRows ? instance.gridOptions.api.forEachNodeAfterFilter(e => data.push(e.data)) : data = instance.gridOptions.rowData;
        let hideColumnsByName = instance.gridOptions.columnDefs
            .reduce((a, v) => ({ ...a, [v.field]: v.hide }), {});
        hideColumnsByName["rowColorClass"] = true;
        hideColumnsByName["rowTooltip"] = true;
        let result = instance.gridOptions.columnDefs
            .reduce((a, v) => a += v.hide ? "" : ('"' + v.headerName + '"\t'), "").trim() + "\n";
        for (let i = 0; i < data.length; i++) {
            result += Object.entries(data[i]).reduce((a, v) => a += hideColumnsByName[v[0]] ? "" :
                '"' + v[1] + '"\t', "").trim() + "\n";
        }
        window.downloadFromByteArray({
            fileName: name + ".tsv", contentType: "data:text/tsv;charset=utf-8",
            byteArray: window.b64EncodeUnicode(result)
        });
    },
    applyFilter: function (filter, divId) {
        let instance = GenericGrid.getInstance(divId);
        let filterInstance = instance.gridOptions.api.getFilterInstance(filter.name);
        filterInstance.setModelIntoUi(filter.filter);
    },
}