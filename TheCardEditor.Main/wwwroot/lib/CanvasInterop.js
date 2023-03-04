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
    getContext() {
        return document.getElementById(this.divId).getContext("2d");
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
    drawText: function (xPos, yPos, text, divId) {
        const instance = CanvasInterop.getInstance(divId);
        const name = "Lovely Shape " + Math.round(Math.random() * 100);
        const canvasText = new fabric.Text(text, {
            fontSize: 30,
            left: xPos,
            top: yPos,
            styles: {
                comment: "Item 1",
                0: {
                    0: { fontWeight: "bold", fontSize: 100 }
                }
            }
        });
        canvasText.toObject = (function (toObject) {
            return function () {
                return fabric.util.object.extend(toObject.call(this), { name: name });
            };
        })(canvasText.toObject);
        instance.canvas.add(canvasText);
    },
    drawImage: function (xPos, yPos, image, divId) {
        const instance = CanvasInterop.getInstance(divId);
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