class CanvasInterop {
    divId;
    canvas;
    static getInstance(divId) {
        if (!window.canvasInteropFunctions.instance.hasOwnProperty(divId)) {
            window.canvasInteropFunctions.instance[divId] = new CanvasInterop();
        }
        return window.canvasInteropFunctions.instance[divId];
    }
    static removeInstance(divId) {
        delete window.canvasInteropFunctions.instance[divId];
    }
    getElement() {
        return document.getElementById(this.divId);
    }
    getDataUrl() {
        const canvas = document.getElementById(this.divId);
        return canvas.toDataURL("image/png");
    }
}
window.canvasInteropFunctions = {
    instance: {},
    initialize: function (divId) {
        const instance = CanvasInterop.getInstance(divId);
        instance.canvas = new fabric.Canvas(divId);
        instance.divId = divId;
    },
    drawText: function (xPos, yPos, text, tag, divId) {
        const instance = CanvasInterop.getInstance(divId);
        const canvasText = new fabric.Textbox(text, {
            fontSize: 30,
            left: xPos,
            editable: true,
            top: yPos,
        });
        canvasText.toObject = (function (toObject) {
            return function () {
                return fabric.util.object.extend(toObject.call(this), { tag: tag });
            };
        })(canvasText.toObject);
        instance.canvas.add(canvasText);
    },
    onKeyDown(key, divId) {
        const canvas = CanvasInterop.getInstance(divId).canvas;
        const step = 1;
        const activeGroup = canvas.getActiveObjects();
        if (Array.isArray(activeGroup) && activeGroup.length > 0) {
            activeGroup.forEach(obj => {
                switch (key) {
                    case "ArrowLeft":
                        obj.left = obj.left - step;
                        break;
                    case "ArrowUp":
                        obj.top = obj.top - step;
                        break;
                    case "ArrowRight":
                        obj.left = obj.left + step;
                        break;
                    case "ArrowDown":
                        obj.top = obj.top + step;
                        break;
                }
                obj.setCoords();
            });
            canvas.renderAll();
        }
    },
    drawImage: function (xPos, yPos, image, divId) {
        const instance = CanvasInterop.getInstance(divId);
    },
    exportJson: function (divId) {
        const instance = CanvasInterop.getInstance(divId);
        return instance.canvas.toJSON(["tag"]);
    },
    importJson: function (divId, json) {
        const instance = CanvasInterop.getInstance(divId);
        return instance.canvas.loadFromJSON(json);
    },
    exportCanvas: function (divId) {
        const instance = CanvasInterop.getInstance(divId);
        const result = instance.getDataUrl();
        console.log(result);
        return result;
    },
    dispose: function (divId) {
        CanvasInterop.removeInstance(divId);
    }
}