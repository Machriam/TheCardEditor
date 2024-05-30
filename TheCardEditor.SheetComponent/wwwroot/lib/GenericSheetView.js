class GenericSheetView {
    static objectIsNullOrEmpty(obj) {
        if (obj === null) return true;
        if (obj === undefined) return true;
        if (obj && Object.keys(obj).length === 0 && obj.constructor === Object) return true;
        if (obj === "") return true;
        return false;
    };
    static rgbaToRgb(hex) {
        if (hex.startsWith("#")) return this.hexToRgb(hex);
        let result = /^rgb*.(\d+),(\d+),(\d+).*$/i.exec(hex);
        return result ? [parseInt(result[1]), parseInt(result[2]), parseInt(result[3])] : null;
    };
    static hexToRgb(hex) {
        var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result ? [parseInt(result[1], 16), parseInt(result[2], 16), parseInt(result[3], 16)] : null;
    };
    static rgbToHex(arr) {
        return "#" + ((1 << 24) + (arr[0] << 16) + (arr[1] << 8) + arr[2]).toString(16).slice(1);
    };
    static listToDictionary(list, keySelector, valueSelector) {
        return Object.assign({}, ...Object.entries(list)
            .map(([a, v]) => ({ [keySelector(v)]: valueSelector(v) })));
    }
    static isNumeric(str) {
        return !isNaN(str) && !isNaN(parseFloat(str))
    };
    static findNearestMatch(values, input) {
        let valueLookup = values.map(v => [...v.toLowerCase()]
            .reduce((a, e) => { a[e] = a[e] ? a[e] + 1 : 1; return a }, {}));
        let inputLookup = [...input.toLowerCase()]
            .reduce((a, e) => { a[e] = a[e] ? a[e] + 1 : 1; return a }, {});
        let similarity = {};
        valueLookup.forEach((dic, i) => {
            let sim = 0;
            Object.getOwnPropertyNames(dic).forEach((v) => {
                if (inputLookup.hasOwnProperty(v)) sim += Math.min(inputLookup[v], dic[v]);
            });
            similarity[i] = sim;
        });
        let bestMatchIndex = Object.entries(similarity).reduce((a, e) => a[1] > e[1] ? a : e)[0];
        return values[bestMatchIndex];
    };
    static convertNumericRange(input, decimalDigits, minimum = 0, maximum = 999999.99) {
        var splittedData = input.split('-');
        if (splittedData.length == 1) return GenericSheetView.convertNumeric(input);
        return GenericSheetView.formattedNumericConversion(splittedData[0], decimalDigits, minimum, maximum) + "-" +
            GenericSheetView.formattedNumericConversion(splittedData[1], decimalDigits, minimum, maximum);
    };
    static formattedNumericConversion(input, decimalDigits, minimum = 0, maximum = 999999.99) {
        return GenericSheetView.convertNumeric(input, minimum, maximum)
            .toLocaleString("en-US", { useGrouping: false, minimumFractionDigits: decimalDigits });
    }
    static convertNumeric(input, minimum = 0, maximum = 999999.99) {
        if (GenericSheetView.objectIsNullOrEmpty(input)) return minimum;
        if (typeof (input) == "number") return Math.min(Math.max(input, minimum), maximum);
        let result = "";
        let i = 0;
        let lastSeparator = -1;
        while (i < input.length) {
            let char = input.charCodeAt(i);
            if (char <= 57 && char >= 48) {
                result += input[i];
            }
            else if (char == 44 || char == 46) {
                result += ".";
                if (lastSeparator > -1) result = result.slice(0, lastSeparator) + result.slice(lastSeparator + 1, result.length);
                lastSeparator = result.length - 1;
            }
            i++;
        }
        return result == "" ? 0 : Math.min(Math.max(parseFloat(result), minimum), maximum);
    };
    static arrowNavigation(params, grid) {
        var previousCell = params.previousCellPosition;
        var suggestedNextCell = params.nextCellPosition;

        var KEY_UP = 38;
        var KEY_DOWN = 40;
        var KEY_LEFT = 37;
        var KEY_RIGHT = 39;

        switch (params.event.keyCode) {
            case KEY_DOWN:
                previousCell = params.previousCellPosition;
                grid.gridOptions.api.forEachNode(function (node) {
                    if (previousCell.rowIndex + 1 === node.rowIndex) {
                        node.setSelected(true);
                    }
                });
                return suggestedNextCell;
            case KEY_UP:
                previousCell = params.previousCellPosition;
                grid.gridOptions.api.forEachNode(function (node) {
                    if (previousCell.rowIndex - 1 === node.rowIndex) {
                        node.setSelected(true);
                    }
                });
                return suggestedNextCell;
            case KEY_LEFT:
            case KEY_RIGHT:
                return suggestedNextCell;
            default:
                throw "this will never happen, navigation is always one of the 4 keys above";
        }
    }

    sheetData = {};
    converter = {
        "numeric": GenericSheetView.convertNumeric,
        "default": (v) => v,
        "": (v) => v
    };
    xs;
    timeout;
    onCellSelected() {
        var rows = this.xs.datas[0].rows["_"];
        const updateNumber = Math.random();
        Object.getOwnPropertyNames(rows).forEach(ri => {
            if (!GenericSheetView.isNumeric(ri) || ri == "0") return;
            Object.getOwnPropertyNames(rows[ri].cells).forEach(ci => {
                if (typeof this.xs.cell(ri, ci).text === 'string')
                    this.xs.cell(ri, ci).text = this.xs.cell(ri, ci).text.trim();
                let result = this.validate(ri, ci);
                if (result[0]) this.xs.cell(ri, ci).text = result[1];
                this.hightlightCells(ri, ci, updateNumber);
            });
        });
        this.xs.reRender();
    };
    hightlightCells(ri, ci, updateNumber) {
        const cell = this.xs.cell(ri, ci);
        if (cell.updateNumber != updateNumber) delete cell.style;
        if (this.parameter.ColumnDefinitions.length <= ci) return;
        if (!this.parameter.HighlightCellsDictionary?.hasOwnProperty(this.parameter.ColumnDefinitions[ci].PropertyName)) return;
        const dictionary = this.parameter.HighlightCellsDictionary[this.parameter.ColumnDefinitions[ci].PropertyName];
        if (!dictionary.hasOwnProperty(cell.text)) return;
        const styleIndex = this.styleByColor[dictionary[cell.text].Color].index;
        if (dictionary[cell.text].WholeRow) {
            for (let i = 0; i < this.parameter.ColumnDefinitions.length; i++) {
                this.xs.cell(ri, i).style = styleIndex;
                this.xs.cell(ri, i).updateNumber = updateNumber;
            }
        }
        else {
            cell.style = styleIndex;
            cell.updateNumber = updateNumber;
        }
    }
    validate(ri, ci) {
        let value = this.xs.getParsedData(this.xs.cell(ri, ci).text);
        if (this.parameter.ColumnDefinitions.length <= ci) return [false, ""];
        if (this.parameter.AllowedValuesFor?.hasOwnProperty(this.parameter.ColumnDefinitions[ci].HeaderName)) {
            let allowedValues = this.parameter.AllowedValuesFor[this.parameter.ColumnDefinitions[ci].HeaderName];
            return [true, GenericSheetView.findNearestMatch(allowedValues, value)];
        }
        return [true, this.converter[this.parameter.ColumnDefinitions[ci].Converter](value)];
    };
    static getInstance() {
        if (!window.genericSheetFunctions.instance
            || (Object.keys(window.genericSheetFunctions.instance).length === 0
                && window.genericSheetFunctions.instance.constructor === Object)) {
            window.genericSheetFunctions.instance = new GenericSheetView();
        }
        return window.genericSheetFunctions.instance;
    }
    static resetInstance() {
        window.genericSheetFunctions.instance = {};
    }
    loadData(data, parameter, gridId) {
        let instance = GenericSheetView.getInstance();
        instance.sheetData = JSON.parse(data);
        instance.parameter = JSON.parse(parameter);
        instance.createStyles(parameter);
        data = instance.sheetData;
        parameter = instance.parameter;
        let cols = {
            len: parameter.ColumnDefinitions.filter(cd => !cd.Hide).length,
        }
        parameter.ColumnDefinitions.filter(cd => !cd.Hide).forEach((cd, i) => cols[i] = { width: cd.Width });
        let headers = parameter.ColumnDefinitions.filter(cd => !cd.Hide).map(cd => cd.HeaderName);
        let rows = { len: data.length >= parameter.MinimumRows ? data.length + 1 : parameter.MinimumRows };
        rows["0"] = { cells: {} };
        headers.forEach((v, i) => rows["0"].cells[i] = { text: v });
        data.forEach(function (item, i) {
            rows[i + 1] = { cells: {} };
            Object.values(item).forEach(function (value, j) {
                if (parameter.ColumnDefinitions[j].Hide) return;
                rows[i + 1].cells[j] = { text: value, editable: parameter.ColumnDefinitions[j].Editable };
            });
        });
        for (let i = data.length; i < parameter.MinimumRows; i++) {
            rows[i + 1] = { cells: {} };
        }
        instance.xs = x_spreadsheet("#" + gridId, {
            showToolbar: false,
            showGrid: true,
            showContextmenu: false,
            view: {
                height: () => document.getElementById(gridId).offsetHeight,
                width: () => document.getElementById(gridId).offsetWidth,
            }
        }).loadData([{
            freeze: 'A2',
            styles: Object.keys(instance.styleByColor).map(k => instance.styleByColor[k].style),
            cols, rows,
        }]);
        instance.xs.on("cell-selected", () => {
            clearTimeout(this.timeout);
            this.timeout = setTimeout(() => instance.onCellSelected(), 200);
        });
        document.getElementsByClassName("x-spreadsheet-bottombar")[0].remove();
        instance.onCellSelected();
    }

    createStyles() {
        let styleCounter = 0;
        const createStyle = function (color) {
            return {
                bgcolor: color,
                textwrap: true,
                color: '#000000',
                border: {
                    top: ['thin', '#0366d6'],
                    bottom: ['thin', '#0366d6'],
                    right: ['thin', '#0366d6'],
                    left: ['thin', '#0366d6'],
                },
            };
        };
        this.styleByColor = {};
        const colors = new Set(Object.keys(this.parameter.HighlightCellsDictionary ?? [])
            .flatMap(x => Object.keys(this.parameter.HighlightCellsDictionary[x])
                .map(y => this.parameter.HighlightCellsDictionary[x][y].Color)));
        colors.forEach(c => {
            this.styleByColor[`${c}`] = { index: styleCounter++, style: createStyle(c) };
        });
    }
}

window.genericSheetFunctions = {
    instance: new GenericSheetView(),
    update: function (data, parameter, gridId) {
        this.dispose();
        document.getElementById(gridId).innerHTML = "";
        let instance = GenericSheetView.getInstance();
        instance.loadData(data, parameter, gridId);
    },
    initialize: function (data, parameter, gridId) {
        let instance = GenericSheetView.getInstance();
        instance.loadData(data, parameter, gridId);
    },
    getSheetData: function (gridId) {
        let instance = GenericSheetView.getInstance(gridId);
        let data = instance.xs.datas[0].rows["_"];
        let rows = Object.getOwnPropertyNames(data);
        let result = [];
        let rowData = {};
        rows.forEach(row => {
            if (row == "0") return;
            rowData = {};
            instance.parameter.ColumnDefinitions.forEach((e, ci) => {
                if (data[row].cells[ci] !== undefined) rowData[e.IsDynamicColumn ? e.HeaderName : e.PropertyName] = instance.validate(row, ci)[1];
            });
            if (Object.entries(rowData).length > 0) result.push(rowData);
        });
        return JSON.stringify(result);
    },
    copyDataToClipboard: async function () {
        let instance = GenericSheetView.getInstance();
        let selectionRange = instance.xs.datas[0].selector.range;
        let data = instance.xs.datas[0].rows["_"];
        let rows = Object.getOwnPropertyNames(data);
        if (selectionRange.sci == selectionRange.eci && selectionRange.sri == selectionRange.eri) {
            selectionRange.sci = 0;
            selectionRange.eci = Object.getOwnPropertyNames(instance.columns).length - 1;
            selectionRange.sri = 0;
            selectionRange.eri = rows.length - 1;
        }
        let result = "";
        for (let ri = selectionRange.sri; ri <= selectionRange.eri; ri++) {
            rowData = "";
            for (let ci = selectionRange.sci; ci <= selectionRange.eci; ci++) {
                if (data[ri].cells[ci] !== undefined) {
                    rowData += instance.xs.getParsedData(data[ri].cells[ci].text) + "\t";
                }
                else {
                    rowData += "\t";
                }
            }
            result += rowData + "\r\n";
        }
        await navigator.clipboard.writeText(result);
    },
    dispose: function () {
        GenericSheetView.resetInstance();
        if (GenericSheetView.objectIsNullOrEmpty(window.xSheetFunctions)) return;
        Object.getOwnPropertyNames(window.xSheetFunctions).forEach(f => window.removeEventListener(f, window.xSheetFunctions[f]))
        window.xSheetFunctions = null;
    },
}