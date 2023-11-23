import state from "/lib/State.js";

class CanvasInterop {
    parameter;
    divId;
    canvas;
    static getInstance(divId) {
        if (!state.instances.hasOwnProperty(divId)) {
            state.instances[divId] = new CanvasInterop();
        }
        return state.instances[divId];
    }
    static removeInstance(divId) {
        delete state.instances[divId];
    }
    createObjectParameter(left = 0, top = 0, tag = null, angle = 0, textSize = null) {
        return { "left": left, "top": top, "tag": tag, "angle": angle, "textSize": textSize };
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
            instance.createObjectParameter(evt.target.left, evt.target.top, evt.target.tag, evt.target.angle, textSize));
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
                    evt.selected[0].tag ?? evt.selected[0].toObject().tag, evt.selected[0].angle, textSize));
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
export function initialize(divId, params) {
    const instance = CanvasInterop.getInstance(divId);
    instance.parameter = params;
    instance.canvas = new fabric.Canvas(divId);
    instance.canvas.on("selection:created", instance.onElementSelected);
    instance.canvas.on("selection:updated", instance.onElementSelected);
    instance.canvas.on("object:rotating", instance.onElementMoved);
    instance.canvas.on("object:moving", instance.onElementMoved);
    instance.canvas.on("selection:cleared", instance.onSelectionCleared);
    instance.divId = divId;
}
export function selectObject(index, divId) {
    const instance = CanvasInterop.getInstance(divId);
    instance.canvas.setActiveObject(instance.canvas.item(index));

    instance.canvas.renderAll();
}
export function addFilter(index, divId) {
    const instance = CanvasInterop.getInstance(divId);
    const applyTo = instance.canvas.getActiveObject();
    const filterImage = instance.canvas.item(index);
    const filter = new ImageFilter();
    const newImage = filter._drawImage(applyTo._element, filterImage._element, applyTo, filterImage);
    this.removeObject(divId);
    this.drawPicture(0, 0, "test", "test", newImage, divId);
}
export function getObjectParameterfunction(divId) {
    const instance = CanvasInterop.getInstance(divId);
    const object = instance.canvas.getActiveObject();
    let textSize = -1;
    if (object?.type != "textbox" && object?.type != "image") return instance.createObjectParameter();
    if (object?.type == "textbox") {
        textSize = object.styles?.[0]?.[0]?.["fontSize"] ?? object.fontSize ?? null;
    }
    return instance.createObjectParameter(object.left, object.top, object.tag, object.angle, textSize);
}
export function bringForward(index, divId) {
    const instance = CanvasInterop.getInstance(divId);
    instance.canvas.bringForward(instance.canvas.item(index));
    instance.canvas.discardActiveObject().renderAll();
    return Math.min(instance.canvas._objects.length, index + 1);
}
export function sendBackwards(index, divId) {
    const instance = CanvasInterop.getInstance(divId);
    instance.canvas.sendBackwards(instance.canvas.item(index));
    instance.canvas.discardActiveObject().renderAll();
}
export function applyFont(styleName, value, divId) {
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
}
export function drawText(xPos, yPos, text, tag, fontSize, divId) {
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
}
export function onKeyDown(key, divId) {
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
}
export function drawPicture(xPos, yPos, pictureId, name, image, divId) {
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
}
export function setCoordinates(divId, left, top, angle) {
    const instance = CanvasInterop.getInstance(divId);
    var activeObject = instance.canvas.getActiveObject();
    if (activeObject == null) return;
    activeObject.set({ left: left, top: top, angle: angle });
    activeObject.setCoords();
    instance.canvas.renderAll();
}

export function removeObject(divId) {
    const instance = CanvasInterop.getInstance(divId);
    const objects = instance.canvas.getActiveObjects();
    for (let i = 0; i < objects.length; i++) {
        instance.canvas.remove(objects[i]);
    }
}
export function centerObjects(divId) {
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
}
export function exportJson(divId) {
    const instance = CanvasInterop.getInstance(divId);
    let result = instance.canvas.toJSON(["tag", "pictureId", "name", "lockScalingY"]);
    result.objects = result.objects.map(o => { o.src = ""; return o; });
    return result;
}
export function reset(divId) {
    const json = "{\"version\":\"5.3.0\",\"objects\":[]}";
    const instance = CanvasInterop.getInstance(divId);
    return instance.canvas.loadFromJSON(json);
}

export function importJson(json, pictureData, divId) {
    if (json == null || Object.keys(json).length == 0) return;
    json.objects.map(o => o.src = pictureData.hasOwnProperty(o.pictureId) ? pictureData[o.pictureId] : "");
    const instance = CanvasInterop.getInstance(divId);
    return instance.canvas.loadFromJSON(json);
}
export function exportCanvas(divId) {
    const instance = CanvasInterop.getInstance(divId);
    const result = instance.getDataUrl();
    console.log(result);
    return result;
}
export function dispose(divId) {
    CanvasInterop.removeInstance(divId);
}

/** @typedef {object} ImagePosition
* @property {number} left
* @property {number} top
* @property {number} scaleX
* @property {number} scaleY
* @property {number} angle
*/
class ImageFilter {
    /**
     * @param {any} applyTo
     * @param {any} filter
     * @param {ImagePosition} applyToPosition
     * @param {ImagePosition} filterPosition
     * @returns
     */
    _drawImage(applyTo, filter, applyToPosition, filterPosition) {
        var vertexShaderSource = `#version 300 es
        in vec2 a_position;
        in vec2 a_texCoord;
        uniform vec2 u_rotation;
        uniform vec2 u_resolution;
        uniform vec2 u_translation;
        out vec2 v_texCoord;
        void main() {
          vec2 rotatedPosition = vec2(
             a_position.x * u_rotation.y + a_position.y * u_rotation.x,
             a_position.y * u_rotation.y - a_position.x * u_rotation.x);
          vec2 position = rotatedPosition + u_translation;
          vec2 zeroToOne = position / u_resolution;
          vec2 zeroToTwo = zeroToOne * 2.0;
          vec2 clipSpace = zeroToTwo - 1.0;
          gl_Position = vec4(clipSpace * vec2(1, -1), 0, 1);
          v_texCoord = a_texCoord;
        }
        `;
        var fragmentShaderSource = `#version 300 es
        precision highp float;
        uniform sampler2D u_image0;
        uniform sampler2D u_image1;
        in vec2 v_texCoord;
        out vec4 outColor;
        void main() {
          vec4 color0 = texture(u_image0, v_texCoord);
          vec4 color1 = texture(u_image1, v_texCoord);
          outColor = color0 * color1;
        }
        `;
        function render(image, filter, canvas) {
            const images = [image, filter];
            var gl = canvas.getContext("webgl2");
            if (!gl) return;
            var program = webglUtils.createProgramFromSources(gl, [vertexShaderSource, fragmentShaderSource]);
            var positionAttributeLocation = gl.getAttribLocation(program, "a_position");
            var texCoordAttributeLocation = gl.getAttribLocation(program, "a_texCoord");
            var resolutionLocation = gl.getUniformLocation(program, "u_resolution");
            var translationLocation = gl.getUniformLocation(program, "u_translation");
            var rotationLocation = gl.getUniformLocation(program, "u_rotation");
            var vao = gl.createVertexArray();
            gl.bindVertexArray(vao);
            var positionBuffer = gl.createBuffer();
            gl.enableVertexAttribArray(positionAttributeLocation);
            gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);
            gl.vertexAttribPointer(positionAttributeLocation, 2, gl.FLOAT, false, 0, 0);
            var texCoordBuffer = gl.createBuffer();
            gl.bindBuffer(gl.ARRAY_BUFFER, texCoordBuffer);
            gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([
                0.0, 0.0,
                1.0, 0.0,
                0.0, 1.0,
                0.0, 1.0,
                1.0, 0.0,
                1.0, 1.0,
            ]), gl.STATIC_DRAW);
            gl.enableVertexAttribArray(texCoordAttributeLocation);
            gl.vertexAttribPointer(texCoordAttributeLocation, 2, gl.FLOAT, false, 0, 0);
            var u_image0Location = gl.getUniformLocation(program, "u_image0");
            var u_image1Location = gl.getUniformLocation(program, "u_image1");
            var textures = [];
            for (var ii = 0; ii < images.length; ++ii) {
                var texture = gl.createTexture();
                gl.bindTexture(gl.TEXTURE_2D, texture);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
                gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, images[ii]);
                textures.push(texture);
            }
            gl.useProgram(program);
            gl.bindVertexArray(vao);
            gl.uniform2f(resolutionLocation, gl.canvas.width, gl.canvas.height);
            gl.uniform2f(translationLocation, applyToPosition.left, applyToPosition.top);
            gl.uniform2fv(rotationLocation, [Math.sin(applyToPosition.angle), Math.cos(applyToPosition.angle)]);
            gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);
            setRectangle(gl, 0, 0, image.width, image.height);
            gl.uniform1i(u_image0Location, 0);
            gl.uniform1i(u_image1Location, 1);
            gl.activeTexture(gl.TEXTURE0);
            gl.bindTexture(gl.TEXTURE_2D, textures[0]);
            gl.activeTexture(gl.TEXTURE1);
            gl.bindTexture(gl.TEXTURE_2D, textures[1]);
            var primitiveType = gl.TRIANGLES;
            var offset = 0;
            var count = 6;
            gl.drawArrays(primitiveType, offset, count);
        }

        function setRectangle(gl, x, y, width, height) {
            var x1 = x;
            var x2 = x + width;
            var y1 = y;
            var y2 = y + height;
            gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([
                x1, y1,
                x2, y1,
                x1, y2,
                x1, y2,
                x2, y1,
                x2, y2,
            ]), gl.STATIC_DRAW);
        }
        const canvas = document.createElement("canvas");
        canvas.width = applyTo.width;
        canvas.height = applyTo.height;
        render(applyTo, filter, canvas);
        return canvas.toDataURL("image/png");
    }
    _drawRectangle(applyTo) {
        const vertexShaderSource = `#version 300 es
        in vec4 a_position;
        void main() {
          gl_Position = a_position;
        }
        `;
        var fragmentShaderSource = `#version 300 es
        precision highp float;
        out vec4 outColor;
        void main() {
          outColor = vec4(1, 0, 0.5, 1);
        }
        `;
        function createShader(gl, type, source) {
            var shader = gl.createShader(type);
            gl.shaderSource(shader, source);
            gl.compileShader(shader);
            var success = gl.getShaderParameter(shader, gl.COMPILE_STATUS);
            if (success) {
                return shader;
            }
            gl.deleteShader(shader);
            return undefined;
        }

        function createProgram(gl, vertexShader, fragmentShader) {
            var program = gl.createProgram();
            gl.attachShader(program, vertexShader);
            gl.attachShader(program, fragmentShader);
            gl.linkProgram(program);
            var success = gl.getProgramParameter(program, gl.LINK_STATUS);
            if (success) return program;
            gl.deleteProgram(program);
            return undefined;
        }

        function main(canvas) {
            const gl = canvas.getContext("webgl2");
            if (!gl) return;
            const vertexShader = createShader(gl, gl.VERTEX_SHADER, vertexShaderSource);
            const fragmentShader = createShader(gl, gl.FRAGMENT_SHADER, fragmentShaderSource);
            const program = createProgram(gl, vertexShader, fragmentShader);
            const positionAttributeLocation = gl.getAttribLocation(program, "a_position");
            const positionBuffer = gl.createBuffer();
            gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);
            var positions = [
                0, 0,
                0, 0.5,
                0.7, 0,
            ];
            gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(positions), gl.STATIC_DRAW);
            var vao = gl.createVertexArray();
            gl.bindVertexArray(vao);
            gl.enableVertexAttribArray(positionAttributeLocation);
            gl.vertexAttribPointer(positionAttributeLocation, 2, gl.FLOAT, false, 0, 0);
            gl.useProgram(program);
            gl.bindVertexArray(vao);
            var primitiveType = gl.TRIANGLES;
            gl.drawArrays(primitiveType, 0, 3);
        }
        const canvas = document.createElement("canvas");
        canvas.width = applyTo.canvas.width;
        canvas.height = applyTo.canvas.height;
        main(canvas);
        return canvas.toDataURL("image/png");
    }
    applyFilter(applyTo, filters) {
        const afterLoad = function (error, texture, source) {
            debugger;
        }
        const canvas = document.createElement("canvas");
        canvas.width = applyTo.canvas.width;
        canvas.height = applyTo.canvas.height;
        const gl = canvas.getContext("webgl2");
        if (!gl) return;
        const vs = `#version 300 es
        in vec2 a_texCoord;
        out vec2 v_texCoord;
        void main() {
           v_texCoord = a_texCoord;
        }
        `;
        const fs = `#version 300 es
        precision highp float;
        uniform sampler2D u_image;
        in vec2 v_texCoord;
        out vec4 outColor;
        void main() {
           outColor = texture(u_image, v_texCoord);
        }
        `;
        const program = twgl.createProgramInfo(gl, [vs, fs]);
        twgl.createTexture(gl, { src: applyTo.src }, afterLoad);
        debugger;
    }
}