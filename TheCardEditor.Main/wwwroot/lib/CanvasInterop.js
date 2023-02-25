class CanvasInterop {
    divId;
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
        instance.divId = divId;
    },
    drawText: function (xPos, yPos, text, divId) {
        const instance = CanvasInterop.getInstance(divId);
        const context = instance.getContext();
        context.font = "30px Arial";
        context.strokeText(text, xPos, yPos);
    },
    drawImage: function (xPos, yPos, image, divId) {
        const instance = CanvasInterop.getInstance(divId);
    },
    exportCanvas: function (divId) {
        const instance = CanvasInterop.getInstance(divId);
        return instance.getDataUrl();
    },
    dispose: function (divId) {
        CanvasInterop.removeInstance(divId);
    }
}