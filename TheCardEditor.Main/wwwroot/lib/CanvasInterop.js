class CanvasInterop {
    parameter;
    divId;
    static opencv;
    canvas;
    async getOpenCv() {
        if (this.opencv == undefined) this.opencv = await import("/lib/OpenCvInterop.js");
        return this.opencv;
    }
    static getInstance(divId) {
        if (!window.canvasInteropFunctions.instance.hasOwnProperty(divId)) {
            window.canvasInteropFunctions.instance[divId] = new CanvasInterop();
        }
        return window.canvasInteropFunctions.instance[divId];
    }
    static removeInstance(divId) {
        delete window.canvasInteropFunctions.instance[divId];
    }
    createObjectParameter(left = 0, top = 0, tag = null, angle = 0, textSize = null, pictureId = -1) {
        return { "left": left, "top": top, "tag": tag, "angle": angle, "textSize": textSize, "pictureId": pictureId };
    }
    onSelectionCleared(evt) {
        const id = evt.hasOwnProperty("target") ? evt.target.canvas.lowerCanvasEl.id : evt.deselected[0].canvas.lowerCanvasEl.id;
        const instance = CanvasInterop.getInstance(id);
        instance.parameter.dotnetReference.invokeMethodAsync(instance.parameter.objectDeselectionHandler);
    }
    onElementMoved(evt) {
        if (evt.target.hasOwnProperty("_objects") && evt.target._objects.length > 1) return;
        const id = evt.target.canvas.lowerCanvasEl.id;
        const instance = CanvasInterop.getInstance(id);
        const textSize = evt.target.styles?.[0]?.[0]?.["fontSize"] ?? evt.target.fontSize ?? null;
        instance.parameter.dotnetReference.invokeMethodAsync(
            instance.parameter.objectSelectionHandler,
            instance.createObjectParameter(evt.target.left, evt.target.top, evt.target.tag, evt.target.angle, textSize, evt.target.pictureId));
    }
    onElementSelected(evt) {
        const id = evt.selected[0].canvas.lowerCanvasEl.id;
        const instance = CanvasInterop.getInstance(id);
        if (evt.selected.length > 1) {
            instance.parameter.dotnetReference.invokeMethodAsync(instance.parameter.multiObjectSelectionHandler);
        }
        else {
            const textSize = evt.selected[0].styles?.[0]?.[0]?.["fontSize"] ?? evt.selected[0].fontSize ?? null;
            instance.parameter.dotnetReference.invokeMethodAsync(
                instance.parameter.objectSelectionHandler,
                instance.createObjectParameter(evt.selected[0].left, evt.selected[0].top,
                    evt.selected[0].tag ?? evt.selected[0].toObject().tag, evt.selected[0].angle, textSize, evt.selected[0].pictureId));
        }
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
    initialize: function (divId, params) {
        const instance = CanvasInterop.getInstance(divId);
        instance.parameter = params;
        instance.canvas = new fabric.Canvas(divId);
        instance.canvas.on("selection:created", instance.onElementSelected);
        instance.canvas.on("selection:updated", instance.onElementSelected);
        instance.canvas.on("object:rotating", instance.onElementMoved);
        instance.canvas.on("object:moving", instance.onElementMoved);
        instance.canvas.on("selection:cleared", instance.onSelectionCleared);
        instance.divId = divId;
    },
    selectObject: function (index, divId) {
        const instance = CanvasInterop.getInstance(divId);
        instance.canvas.setActiveObject(instance.canvas.item(index));
        instance.canvas.renderAll();
    },
    updateImage: function (newImage, filterPipeline, divId) {
        const instance = CanvasInterop.getInstance(divId);
        const applyTo = instance.canvas.getActiveObject();
        applyTo.setSrc(newImage, function () {
            instance.canvas.renderAll();
        });
        applyTo.pictureFilter = JSON.stringify(filterPipeline);
    },
    getObjectParameter: function (divId) {
        const instance = CanvasInterop.getInstance(divId);
        const object = instance.canvas.getActiveObject();
        let textSize = -1;
        if (object?.type != "textbox" && object?.type != "image") return instance.createObjectParameter();
        if (object?.type == "textbox") {
            textSize = object.styles?.[0]?.[0]?.["fontSize"] ?? object.fontSize ?? null;
        }
        return instance.createObjectParameter(object.left, object.top, object.tag, object.angle, textSize);
    },
    bringForward: function (index, divId) {
        const instance = CanvasInterop.getInstance(divId);
        instance.canvas.bringForward(instance.canvas.item(index));
        instance.canvas.discardActiveObject().renderAll();
        return Math.min(instance.canvas._objects.length, index + 1);
    },
    zoom: function (zoom, divId) {
        const instance = CanvasInterop.getInstance(divId);
        instance.canvas.setZoom(zoom / 100.0);
    },
    sendBackwards: function (index, divId) {
        const instance = CanvasInterop.getInstance(divId);
        instance.canvas.sendBackwards(instance.canvas.item(index));
        instance.canvas.discardActiveObject().renderAll();
    },
    applyFont: function (styleName, value, divId) {
        const instance = CanvasInterop.getInstance(divId);
        const object = instance.canvas.getActiveObject();
        if (object?.type == "image" && styleName == "clear") {
            object.scaleX = 1;
            object.scaleY = 1;
            object.angle = 0;
            instance.canvas.renderAll();
        }
        if (object?.type != "textbox") return;
        if (styleName == "textAlign") {
            object.textAlign = value;
            instance.canvas.renderAll();
            return;
        }
        let line = 0;
        let index = 0;
        for (let i = 0; i < object.text.length; i++) {
            if (!object.isEditing || (object.isEditing && object.selectionStart <= i && object.selectionEnd > i)) {
                const lineDefined = object.styles[line] != undefined;
                if (!lineDefined) object.styles[line] = {};
                const indexDefined = object.styles[line][index] != undefined;
                if (!indexDefined) object.styles[line][index] = {};
                if (styleName == "clear") {
                    const fontSize = object.styles[line][index]["fontSize"];
                    object.styles[line][index] = {};
                    object.styles[line][index]["fontSize"] = fontSize;
                }
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
    drawText: function (xPos, yPos, text, tag, fontSize, divId) {
        const instance = CanvasInterop.getInstance(divId);
        const canvasText = new fabric.Textbox(text, {
            fontSize: fontSize,
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
    drawPicture: function (xPos, yPos, pictureId, name, image, pictureFilter, divId) {
        const instance = CanvasInterop.getInstance(divId);
        fabric.Image.fromURL(image, function (img) {
            img.set({ left: xPos, top: yPos });
            img.toObject = (function (toObject) {
                return function () {
                    return fabric.util.object.extend(toObject.call(this), { pictureId: pictureId, pictureFilter: pictureFilter, name: name });
                };
            })(img.toObject);
            instance.canvas.add(img);
        })
    },
    setCoordinates: function (divId, left, top, angle) {
        const instance = CanvasInterop.getInstance(divId);
        var activeObject = instance.canvas.getActiveObject();
        if (activeObject == null) return;
        activeObject.set({ left: left, top: top, angle: angle });
        activeObject.setCoords();
        instance.canvas.renderAll();
    },

    removeObject: function (divId) {
        const instance = CanvasInterop.getInstance(divId);
        const objects = instance.canvas.getActiveObjects();
        for (let i = 0; i < objects.length; i++) {
            instance.canvas.remove(objects[i]);
        }
    },
    centerObjects: function (divId) {
        const instance = CanvasInterop.getInstance(divId);
        const objects = instance.canvas.getActiveObjects();
        if (objects.length > 1) {
            objects[0].group.viewportCenterH();
            for (var i = 0; i < objects.length; i++) {
                objects[i].set({ left: - objects[i].width / 2 });
                objects[i].setCoords();
            }
        }
        else {
            objects[0].viewportCenterH();
        }
        instance.canvas.renderAll();
    },
    exportJson: function (divId) {
        const instance = CanvasInterop.getInstance(divId);
        let result = instance.canvas.toJSON(["tag", "pictureFilter", "pictureId", "name", "lockScalingY"]);
        result.objects = result.objects.map((o, i) => {
            if (o.type == "textbox") o.styles = instance.canvas._objects[i].styles;
            o.src = "";
            return o;
        });
        return result;
    },

    reset: function (divId) {
        const json = "{\"version\":\"5.3.0\",\"objects\":[]}";
        const instance = CanvasInterop.getInstance(divId);
        return instance.canvas.loadFromJSON(json);
    },

    importJson: async function (json, pictureData, divId) {
        const instance = CanvasInterop.getInstance(divId);
        const opencv = await instance.getOpenCv();
        if (json == null || Object.keys(json).length == 0) return;
        for (let i = 0; i < json.objects.length; i++) {
            const object = json.objects[i];
            if (!pictureData.hasOwnProperty(object.pictureId)) continue;
            const filter = JSON.parse(object.pictureFilter ?? "{}");
            if (filter?.filters?.length > 0) object.src = await opencv.ApplyFilterPipeline(pictureData[object.pictureId], filter);
            else object.src = pictureData[object.pictureId];
        }
        return instance.canvas.loadFromJSON(json);
    },
    exportCanvas: function (divId) {
        const instance = CanvasInterop.getInstance(divId);
        const result = instance.getDataUrl();
        return result;
    },
    dispose: function (divId) {
        CanvasInterop.removeInstance(divId);
    }
}