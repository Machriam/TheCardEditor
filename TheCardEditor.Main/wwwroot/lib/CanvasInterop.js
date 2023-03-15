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
    selectObject: function (index, divId) {
        const instance = CanvasInterop.getInstance(divId);
        instance.canvas.setActiveObject(instance.canvas.item(index));
        instance.canvas.renderAll();
    },
    bringForward: function (index, divId) {
        const instance = CanvasInterop.getInstance(divId);
        instance.canvas.bringForward(instance.canvas.item(index));
        instance.canvas.discardActiveObject().renderAll();
        return Math.min(instance.canvas._objects.length, index + 1);
    },
    sendBackwards: function (index, divId) {
        const instance = CanvasInterop.getInstance(divId);
        instance.canvas.sendBackwards(instance.canvas.item(index));
        instance.canvas.discardActiveObject().renderAll();
    },
    applyFont: function (styleName, value, divId) {
        const instance = CanvasInterop.getInstance(divId);
        const object = instance.canvas.getActiveObject();
        if (object?.type != "textbox") return;
        let line = 0;
        let index = 0;
        for (let i = 0; i < object.text.length; i++) {
            if (object.selectionStart <= i && object.selectionEnd > i) {
                const lineDefined = object.styles[line] != undefined;
                if (!lineDefined) object.styles[line] = {};
                const indexDefined = object.styles[line][index] != undefined;
                if (!indexDefined) object.styles[line][index] = {};
                if (styleName == "clear") object.styles[line][index] = {};
                else object.styles[line][index][styleName] = value;
            }
            index++;
            if (object.text[i] == '\n') {
                line++;
                index = 0;
            }
        }
        instance.canvas.renderAll();
    },
    drawText: function (xPos, yPos, text, tag, divId) {
        const instance = CanvasInterop.getInstance(divId);
        const canvasText = new fabric.Textbox(text, {
            fontSize: 30,
            left: xPos,
            editable: true,
            top: yPos,
            lockScalingY: true
        });
        canvasText.toObject = (function (toObject) {
            return function () {
                return fabric.util.object.extend(toObject.call(this), { tag: tag, lockScalingY: true });
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
    drawPicture: function (xPos, yPos, pictureId, name, image, divId) {
        const instance = CanvasInterop.getInstance(divId);
        fabric.Image.fromURL(image, function (img) {
            img.set({ left: xPos, top: yPos });
            img.toObject = (function (toObject) {
                return function () {
                    return fabric.util.object.extend(toObject.call(this),
                        { pictureId: pictureId, name: name });
                };
            })(img.toObject);
            instance.canvas.add(img);
        })
    },
    exportJson: function (divId) {
        const instance = CanvasInterop.getInstance(divId);
        return instance.canvas.toJSON(["tag", "pictureId", "name", "lockScalingY"]);
    },
    importJson: function (json, divId) {
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